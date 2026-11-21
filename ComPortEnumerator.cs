using System.Management;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace ComScanner
{
    /// <summary>
    /// Enumerates COM ports using WMI (Win32_PnPEntity) and the registry (SERIALCOMM / Enum).
    /// </summary>
    [SupportedOSPlatform("windows")]
    internal static partial class ComPortEnumerator
    {
        private const string PortsClassGuid = "{4d36e978-e325-11ce-bfc1-08002be10318}";

        [GeneratedRegex(@"\((COM\d+)\)", RegexOptions.IgnoreCase)]
        private static partial Regex ComInNameRegex();

        [GeneratedRegex(@"VID_([0-9A-F]{4})", RegexOptions.IgnoreCase)]
        private static partial Regex VidRegex();

        [GeneratedRegex(@"PID_([0-9A-F]{4})", RegexOptions.IgnoreCase)]
        private static partial Regex PidRegex();

        public static List<ComPortInfo> Enumerate()
        {
            var serialComm = ReadSerialComm();
            var result = new Dictionary<string, ComPortInfo>(StringComparer.OrdinalIgnoreCase);

            const string query =
                "SELECT Name, Description, Manufacturer, PNPDeviceID, Service, Status, ConfigManagerErrorCode " +
                "FROM Win32_PnPEntity WHERE ClassGuid = '" + PortsClassGuid + "' OR Name LIKE '%(COM%'";

            using (var searcher = new ManagementObjectSearcher(query))
            using (var collection = searcher.Get())
            {
                foreach (ManagementObject mo in collection)
                {
                    using (mo)
                    {
                        var info = FromWmi(mo, serialComm);
                        if (info is not null && !result.ContainsKey(info.PortName))
                            result[info.PortName] = info;
                    }
                }
            }

            // Ports registered with the serial subsystem but without a matching PnP device (e.g. some virtual ports).
            foreach (var (port, deviceObject) in serialComm)
            {
                if (result.ContainsKey(port))
                    continue;
                result[port] = new ComPortInfo
                {
                    PortName = port,
                    FriendlyName = $"Serial port ({deviceObject})",
                    Description = "No PnP device found (legacy or virtual port)",
                    DeviceObjectName = deviceObject,
                    Bus = "Virtual / Legacy",
                    Status = "OK",
                };
            }

            return result.Values.OrderBy(p => p.PortNumber).ThenBy(p => p.PortName, StringComparer.OrdinalIgnoreCase).ToList();
        }

        private static ComPortInfo? FromWmi(ManagementObject mo, Dictionary<string, string> serialComm)
        {
            string name = Str(mo, "Name");
            string pnpId = Str(mo, "PNPDeviceID");

            string portName = ReadEnumValue(pnpId, @"Device Parameters", "PortName");
            if (portName.Length == 0)
            {
                var m = ComInNameRegex().Match(name);
                if (m.Success)
                    portName = m.Groups[1].Value.ToUpperInvariant();
            }

            // Skip printer ports and anything we can't map to a port name.
            if (portName.Length == 0 || portName.StartsWith("LPT", StringComparison.OrdinalIgnoreCase))
                return null;

            int? errorCode = null;
            try
            {
                if (mo["ConfigManagerErrorCode"] is { } ec)
                    errorCode = Convert.ToInt32(ec);
            }
            catch (ManagementException) { }

            serialComm.TryGetValue(portName, out var deviceObject);

            return new ComPortInfo
            {
                PortName = portName,
                FriendlyName = name,
                Description = Str(mo, "Description"),
                Manufacturer = Str(mo, "Manufacturer"),
                PnpDeviceId = pnpId,
                Service = Str(mo, "Service"),
                Status = Str(mo, "Status"),
                ErrorCode = errorCode,
                Vid = MatchGroup(VidRegex(), pnpId),
                Pid = MatchGroup(PidRegex(), pnpId),
                SerialNumber = ParseSerialNumber(pnpId),
                Bus = ParseBus(pnpId),
                LocationInfo = ReadEnumValue(pnpId, null, "LocationInformation"),
                DeviceObjectName = deviceObject ?? "",
            };
        }

        private static Dictionary<string, string> ReadSerialComm()
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DEVICEMAP\SERIALCOMM");
                if (key is null)
                    return map;
                foreach (var valueName in key.GetValueNames())
                {
                    if (key.GetValue(valueName) is string port && port.Length > 0)
                        map[port.Trim()] = valueName;
                }
            }
            catch (Exception) { }
            return map;
        }

        private static string ReadEnumValue(string pnpId, string? subKey, string valueName)
        {
            if (string.IsNullOrEmpty(pnpId))
                return "";
            try
            {
                var path = @"SYSTEM\CurrentControlSet\Enum\" + pnpId + (subKey is null ? "" : "\\" + subKey);
                using var key = Registry.LocalMachine.OpenSubKey(path);
                return key?.GetValue(valueName) as string ?? "";
            }
            catch (Exception)
            {
                return "";
            }
        }

        private static string ParseSerialNumber(string pnpId)
        {
            var parts = pnpId.Split('\\');
            if (parts.Length < 3)
                return "";

            // FTDIBUS\VID_0403+PID_6001+A50285BIA\0000
            if (parts[0].Equals("FTDIBUS", StringComparison.OrdinalIgnoreCase))
            {
                var segs = parts[1].Split('+');
                return segs.Length >= 3 ? segs[2] : "";
            }

            // USB\VID_xxxx&PID_xxxx\SERIAL  (an '&' means Windows generated the instance ID — no real serial)
            if (parts[0].Equals("USB", StringComparison.OrdinalIgnoreCase) && !parts[2].Contains('&'))
                return parts[2];

            return "";
        }

        private static string ParseBus(string pnpId)
        {
            var prefix = pnpId.Split('\\')[0].ToUpperInvariant();
            return prefix switch
            {
                "" => "",
                "USB" => "USB",
                "FTDIBUS" => "USB (FTDI)",
                "BTHENUM" => "Bluetooth",
                "ACPI" => "Onboard (ACPI)",
                "PCI" => "PCI",
                "ROOT" => "Virtual (Root)",
                _ when prefix.StartsWith("USB") => "USB",
                _ => prefix,
            };
        }

        private static string MatchGroup(Regex regex, string input)
        {
            var m = regex.Match(input);
            return m.Success ? m.Groups[1].Value.ToUpperInvariant() : "";
        }

        private static string Str(ManagementBaseObject mo, string property)
        {
            try
            {
                return mo[property]?.ToString()?.Trim() ?? "";
            }
            catch (ManagementException)
            {
                return "";
            }
        }
    }
}
