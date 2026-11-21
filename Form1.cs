using System.Diagnostics;
using System.Media;
using System.Runtime.InteropServices;
using System.Text;

namespace ComScanner
{
    public partial class Form1 : Form
    {
        private const int WM_DEVICECHANGE = 0x0219;
        private const int DBT_DEVNODES_CHANGED = 0x0007;
        private const int DBT_DEVICEARRIVAL = 0x8000;
        private const int DBT_DEVICEREMOVECOMPLETE = 0x8004;
        private const int DBT_DEVTYP_PORT = 0x0003;

        private static readonly int[] PollIntervalsMs = [1000, 2000, 5000, 10000, 30000];

        private readonly Dictionary<string, ComPortInfo> _current = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, DateTime> _connectedSince = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, DateTime> _lastDisconnected = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> _lastPortForDevice = new(StringComparer.OrdinalIgnoreCase);
        private readonly List<PortEvent> _events = [];

        private bool _initialScanDone;
        private bool _scanning;
        private bool _rescanPending;
        private string _pendingTrigger = "";
        private int _scanCount;

        public Form1()
        {
            InitializeComponent();
            cboInterval.SelectedIndex = 1;
        }

        private async void Form1_Load(object? sender, EventArgs e)
        {
            uptimeTimer.Start();
            pollTimer.Enabled = btnPolling.Checked;
            await ScanAsync("Startup");
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_DEVICECHANGE)
            {
                int evt = (int)m.WParam;
                if (evt is DBT_DEVICEARRIVAL or DBT_DEVICEREMOVECOMPLETE or DBT_DEVNODES_CHANGED)
                {
                    string trigger = evt switch
                    {
                        DBT_DEVICEARRIVAL => "Windows: device arrival",
                        DBT_DEVICEREMOVECOMPLETE => "Windows: device removal",
                        _ => "Windows: device tree changed",
                    };
                    var portName = TryGetBroadcastPortName(m.LParam);
                    if (portName is not null)
                        trigger += $" ({portName})";

                    // Port-specific notifications are more informative than the generic devnode one; keep them.
                    if (portName is not null || _pendingTrigger.Length == 0)
                        _pendingTrigger = trigger;

                    debounceTimer.Stop();
                    debounceTimer.Start();
                }
            }
            base.WndProc(ref m);
        }

        private static string? TryGetBroadcastPortName(IntPtr lParam)
        {
            if (lParam == IntPtr.Zero)
                return null;
            try
            {
                // DEV_BROADCAST_HDR: dbch_size, dbch_devicetype, dbch_reserved; DEV_BROADCAST_PORT adds dbcp_name.
                int deviceType = Marshal.ReadInt32(lParam, 4);
                if (deviceType == DBT_DEVTYP_PORT)
                    return Marshal.PtrToStringUni(lParam + 12);
            }
            catch (Exception) { }
            return null;
        }

        private async void debounceTimer_Tick(object? sender, EventArgs e)
        {
            debounceTimer.Stop();
            var trigger = _pendingTrigger;
            _pendingTrigger = "";
            await ScanAsync(trigger.Length > 0 ? trigger : "Windows notification");
        }

        private async void pollTimer_Tick(object? sender, EventArgs e) => await ScanAsync("Poll");

        private async void btnScan_Click(object? sender, EventArgs e) => await ScanAsync("Manual scan");

        private async void Form1_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                e.Handled = true;
                await ScanAsync("Manual scan");
            }
        }

        private void btnPolling_CheckedChanged(object? sender, EventArgs e) => pollTimer.Enabled = btnPolling.Checked;

        private void cboInterval_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cboInterval.SelectedIndex >= 0)
                pollTimer.Interval = PollIntervalsMs[cboInterval.SelectedIndex];
        }

        private void uptimeTimer_Tick(object? sender, EventArgs e)
        {
            var now = DateTime.Now;
            lvPorts.BeginUpdate();
            foreach (ListViewItem item in lvPorts.Items)
            {
                if (item.Tag is ComPortInfo p && _connectedSince.TryGetValue(p.PortName, out var since))
                    item.SubItems[colSince.Index].Text = FormatDuration(now - since);
            }
            lvPorts.EndUpdate();
        }

        private async Task ScanAsync(string trigger)
        {
            if (_scanning)
            {
                _rescanPending = true;
                return;
            }

            _scanning = true;
            try
            {
                do
                {
                    _rescanPending = false;
                    var sw = Stopwatch.StartNew();
                    List<ComPortInfo> ports;
                    try
                    {
                        ports = await Task.Run(ComPortEnumerator.Enumerate);
                    }
                    catch (Exception ex)
                    {
                        AddEvent(new PortEvent(DateTime.Now, PortEventType.Error, "", null, $"Scan failed: {ex.Message}", SincePrevious()));
                        return;
                    }
                    sw.Stop();
                    _scanCount++;
                    ApplyScan(ports);
                    lblLastScan.Text = $"Last scan {DateTime.Now:HH:mm:ss} ({sw.ElapsedMilliseconds} ms) • scans: {_scanCount}";
                    if (trigger != "Poll")
                        lblTrigger.Text = $"Last trigger: {trigger} at {DateTime.Now:HH:mm:ss.fff}";
                }
                while (_rescanPending);
            }
            finally
            {
                _scanning = false;
            }
        }

        private void ApplyScan(List<ComPortInfo> ports)
        {
            var now = DateTime.Now;
            var newMap = ports.ToDictionary(p => p.PortName, StringComparer.OrdinalIgnoreCase);
            bool connectOrDisconnect = false;

            if (!_initialScanDone)
            {
                foreach (var p in ports)
                {
                    _connectedSince[p.PortName] = now;
                    RememberDevice(p);
                    AddEvent(new PortEvent(now, PortEventType.Initial, p.PortName, p, "Present at startup", SincePrevious()));
                }
                if (ports.Count == 0)
                    AddEvent(new PortEvent(now, PortEventType.Initial, "", null, "No COM ports found at startup", SincePrevious()));
                _initialScanDone = true;
            }
            else
            {
                // Removed ports
                foreach (var (name, old) in _current)
                {
                    if (newMap.ContainsKey(name))
                        continue;
                    string info = _connectedSince.TryGetValue(name, out var since)
                        ? $"Was connected for {FormatDuration(now - since)}"
                        : "";
                    _connectedSince.Remove(name);
                    _lastDisconnected[name] = now;
                    AddEvent(new PortEvent(now, PortEventType.Disconnected, name, old, info, SincePrevious()));
                    connectOrDisconnect = true;
                }

                foreach (var p in ports)
                {
                    if (!_current.TryGetValue(p.PortName, out var old))
                    {
                        AddEvent(new PortEvent(now, PortEventType.Connected, p.PortName, p, DescribeArrival(p, now), SincePrevious()));
                        _connectedSince[p.PortName] = now;
                        RememberDevice(p);
                        connectOrDisconnect = true;
                    }
                    else if (!string.Equals(old.PnpDeviceId, p.PnpDeviceId, StringComparison.OrdinalIgnoreCase))
                    {
                        AddEvent(new PortEvent(now, PortEventType.Replaced, p.PortName, p,
                            $"Was: {old.DisplayName} [{old.PnpDeviceId}]", SincePrevious()));
                        _connectedSince[p.PortName] = now;
                        RememberDevice(p);
                        connectOrDisconnect = true;
                    }
                    else if (old.StatusText != p.StatusText || old.FriendlyName != p.FriendlyName || old.Service != p.Service)
                    {
                        AddEvent(new PortEvent(now, PortEventType.StatusChanged, p.PortName, p, DescribeChange(old, p), SincePrevious()));
                    }
                }
            }

            _current.Clear();
            foreach (var p in ports)
                _current[p.PortName] = p;

            RefreshPortList();

            if (connectOrDisconnect && btnBeep.Checked)
                SystemSounds.Asterisk.Play();
        }

        private string DescribeArrival(ComPortInfo p, DateTime now)
        {
            var parts = new List<string>();
            if (_lastDisconnected.TryGetValue(p.PortName, out var gone))
                parts.Add($"Port was absent for {FormatDuration(now - gone)}");
            if (p.PnpDeviceId.Length > 0 &&
                _lastPortForDevice.TryGetValue(p.PnpDeviceId, out var prevPort) &&
                !prevPort.Equals(p.PortName, StringComparison.OrdinalIgnoreCase))
                parts.Add($"Same device was previously on {prevPort}");
            if (p.HasProblem)
                parts.Add("⚠ Device has a problem — check driver");
            return parts.Count > 0 ? string.Join("; ", parts) : "New port";
        }

        private static string DescribeChange(ComPortInfo old, ComPortInfo p)
        {
            var parts = new List<string>();
            if (old.StatusText != p.StatusText)
                parts.Add($"Status: {old.StatusText} → {p.StatusText}");
            if (old.FriendlyName != p.FriendlyName)
                parts.Add($"Name: {old.FriendlyName} → {p.FriendlyName}");
            if (old.Service != p.Service)
                parts.Add($"Driver: {Dash(old.Service)} → {Dash(p.Service)}");
            return string.Join("; ", parts);
        }

        private void RememberDevice(ComPortInfo p)
        {
            if (p.PnpDeviceId.Length > 0)
                _lastPortForDevice[p.PnpDeviceId] = p.PortName;
        }

        private TimeSpan? SincePrevious() => _events.Count > 0 ? DateTime.Now - _events[^1].Timestamp : null;

        private void AddEvent(PortEvent ev)
        {
            _events.Add(ev);
            var d = ev.Device;
            var item = new ListViewItem(ev.Timestamp.ToString("HH:mm:ss.fff"))
            {
                Tag = ev,
                UseItemStyleForSubItems = true,
            };
            item.SubItems.Add(ev.SincePrevious is { } delta ? "+" + FormatDuration(delta, precise: true) : "");
            item.SubItems.Add(ev.TypeText);
            item.SubItems.Add(ev.PortName);
            item.SubItems.Add(d?.DisplayName ?? "");
            item.SubItems.Add(d?.VidPid ?? "");
            item.SubItems.Add(d?.SerialNumber ?? "");
            item.SubItems.Add(d?.Manufacturer ?? "");
            item.SubItems.Add(d?.StatusText ?? "");
            item.SubItems.Add(ev.Info);

            (item.BackColor, item.ForeColor) = ev.Type switch
            {
                PortEventType.Connected => (Color.FromArgb(220, 245, 220), Color.DarkGreen),
                PortEventType.Disconnected => (Color.FromArgb(250, 220, 220), Color.DarkRed),
                PortEventType.Replaced => (Color.FromArgb(220, 230, 250), Color.DarkBlue),
                PortEventType.StatusChanged => (Color.FromArgb(255, 245, 200), Color.SaddleBrown),
                PortEventType.Error => (Color.MistyRose, Color.Red),
                _ => (SystemColors.Window, Color.DimGray),
            };

            lvEvents.Items.Add(item);
            if (btnAutoScroll.Checked)
                item.EnsureVisible();

            grpEvents.Text = $"Event Timeline ({_events.Count})";
        }

        private void RefreshPortList()
        {
            string? selected = lvPorts.SelectedItems.Count > 0 ? (lvPorts.SelectedItems[0].Tag as ComPortInfo)?.PortName : null;
            var now = DateTime.Now;

            lvPorts.BeginUpdate();
            lvPorts.Items.Clear();
            foreach (var p in _current.Values.OrderBy(p => p.PortNumber).ThenBy(p => p.PortName))
            {
                var item = new ListViewItem(p.PortName) { Tag = p };
                item.SubItems.Add(p.DisplayName);
                item.SubItems.Add(p.StatusText);
                item.SubItems.Add(p.Vid);
                item.SubItems.Add(p.Pid);
                item.SubItems.Add(p.SerialNumber);
                item.SubItems.Add(p.Manufacturer);
                item.SubItems.Add(p.Bus);
                item.SubItems.Add(p.Service);
                item.SubItems.Add(_connectedSince.TryGetValue(p.PortName, out var since) ? FormatDuration(now - since) : "");
                item.SubItems.Add(p.LocationInfo);
                if (p.HasProblem)
                {
                    item.BackColor = Color.FromArgb(255, 235, 200);
                    item.ForeColor = Color.DarkRed;
                }
                lvPorts.Items.Add(item);
                if (selected is not null && p.PortName.Equals(selected, StringComparison.OrdinalIgnoreCase))
                    item.Selected = true;
            }
            lvPorts.EndUpdate();

            grpPorts.Text = $"Current COM Ports ({_current.Count})";
            lblStatus.Text = _current.Count == 0
                ? "No COM ports present"
                : $"{_current.Count} port(s): {string.Join(", ", _current.Keys.OrderBy(k => k.Length).ThenBy(k => k))}";

            if (selected is not null && lvPorts.SelectedItems.Count == 0 && lvEvents.SelectedItems.Count == 0)
                txtDetails.Text = $"{selected} is no longer present.{Environment.NewLine}{Environment.NewLine}{PortHistory(selected)}";
        }

        private void lvPorts_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (lvPorts.SelectedItems.Count == 0 || lvPorts.SelectedItems[0].Tag is not ComPortInfo p)
                return;
            var sb = new StringBuilder();
            sb.AppendLine("=== CURRENT DEVICE ===");
            sb.Append(p.ToDetailsText());
            if (_connectedSince.TryGetValue(p.PortName, out var since))
                sb.AppendLine($"{"Connected Since",-18}: {since:yyyy-MM-dd HH:mm:ss.fff}");
            sb.AppendLine();
            sb.Append(PortHistory(p.PortName));
            txtDetails.Text = sb.ToString();
        }

        private void lvEvents_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (lvEvents.SelectedItems.Count == 0 || lvEvents.SelectedItems[0].Tag is not PortEvent ev)
                return;
            var sb = new StringBuilder();
            sb.AppendLine("=== EVENT ===");
            sb.AppendLine($"{"Time",-18}: {ev.Timestamp:yyyy-MM-dd HH:mm:ss.fff}");
            sb.AppendLine($"{"Event",-18}: {ev.TypeText}");
            sb.AppendLine($"{"Port",-18}: {Dash(ev.PortName)}");
            sb.AppendLine($"{"Info",-18}: {Dash(ev.Info)}");
            if (ev.SincePrevious is { } delta)
                sb.AppendLine($"{"Since Previous",-18}: {FormatDuration(delta, precise: true)}");
            if (ev.Device is not null)
            {
                sb.AppendLine();
                sb.AppendLine("=== DEVICE (at time of event) ===");
                sb.Append(ev.Device.ToDetailsText());
            }
            if (ev.PortName.Length > 0)
            {
                sb.AppendLine();
                sb.Append(PortHistory(ev.PortName));
            }
            txtDetails.Text = sb.ToString();
        }

        private string PortHistory(string portName)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"=== HISTORY FOR {portName} ===");
            foreach (var ev in _events.Where(x => x.PortName.Equals(portName, StringComparison.OrdinalIgnoreCase)))
                sb.AppendLine($"{ev.Timestamp:HH:mm:ss.fff}  {ev.TypeText,-15} {ev.Device?.DisplayName}");
            return sb.ToString();
        }

        private void btnClear_Click(object? sender, EventArgs e)
        {
            _events.Clear();
            lvEvents.Items.Clear();
            txtDetails.Clear();
            grpEvents.Text = "Event Timeline";
        }

        private void btnExport_Click(object? sender, EventArgs e)
        {
            if (_events.Count == 0)
            {
                MessageBox.Show(this, "There are no events to export.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var dlg = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                FileName = $"ComPortLog_{DateTime.Now:yyyyMMdd_HHmmss}.csv",
            };
            if (dlg.ShowDialog(this) != DialogResult.OK)
                return;

            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("Timestamp,Event,Port,DeviceName,VID,PID,SerialNumber,Manufacturer,Status,Bus,Driver,Location,PnpDeviceId,Info");
                foreach (var ev in _events)
                {
                    var d = ev.Device;
                    string[] fields =
                    [
                        ev.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff"), ev.TypeText, ev.PortName,
                        d?.DisplayName ?? "", d?.Vid ?? "", d?.Pid ?? "", d?.SerialNumber ?? "", d?.Manufacturer ?? "",
                        d?.StatusText ?? "", d?.Bus ?? "", d?.Service ?? "", d?.LocationInfo ?? "", d?.PnpDeviceId ?? "", ev.Info,
                    ];
                    sb.AppendLine(string.Join(",", fields.Select(CsvEscape)));
                }
                File.WriteAllText(dlg.FileName, sb.ToString(), Encoding.UTF8);
                lblTrigger.Text = $"Exported {_events.Count} events to {dlg.FileName}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Export failed: {ex.Message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDeviceManager_Click(object? sender, EventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo("devmgmt.msc") { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Could not open Device Manager: {ex.Message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static string CsvEscape(string s) =>
            s.IndexOfAny([',', '"', '\n', '\r']) >= 0 ? "\"" + s.Replace("\"", "\"\"") + "\"" : s;

        private static string Dash(string s) => string.IsNullOrEmpty(s) ? "—" : s;

        private static string FormatDuration(TimeSpan t, bool precise = false)
        {
            if (t < TimeSpan.Zero)
                t = TimeSpan.Zero;
            if (precise && t.TotalSeconds < 60)
                return $"{t.TotalSeconds:0.000}s";
            if (t.TotalDays >= 1)
                return $"{(int)t.TotalDays}d {t:hh\\:mm\\:ss}";
            return t.ToString(@"hh\:mm\:ss");
        }
    }
}
