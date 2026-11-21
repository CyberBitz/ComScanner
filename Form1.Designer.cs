namespace ComScanner
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            toolStrip = new ToolStrip();
            btnScan = new ToolStripButton();
            btnClear = new ToolStripButton();
            btnExport = new ToolStripButton();
            sep1 = new ToolStripSeparator();
            btnAutoScroll = new ToolStripButton();
            btnBeep = new ToolStripButton();
            btnPolling = new ToolStripButton();
            lblInterval = new ToolStripLabel();
            cboInterval = new ToolStripComboBox();
            sep2 = new ToolStripSeparator();
            btnDeviceManager = new ToolStripButton();
            statusStrip = new StatusStrip();
            lblStatus = new ToolStripStatusLabel();
            lblTrigger = new ToolStripStatusLabel();
            lblLastScan = new ToolStripStatusLabel();
            splitMain = new SplitContainer();
            grpPorts = new GroupBox();
            lvPorts = new ListView();
            colPort = new ColumnHeader();
            colName = new ColumnHeader();
            colStatus = new ColumnHeader();
            colVid = new ColumnHeader();
            colPid = new ColumnHeader();
            colSerial = new ColumnHeader();
            colMfr = new ColumnHeader();
            colBus = new ColumnHeader();
            colDriver = new ColumnHeader();
            colSince = new ColumnHeader();
            colLocation = new ColumnHeader();
            splitBottom = new SplitContainer();
            grpEvents = new GroupBox();
            lvEvents = new ListView();
            colEvTime = new ColumnHeader();
            colEvDelta = new ColumnHeader();
            colEvType = new ColumnHeader();
            colEvPort = new ColumnHeader();
            colEvName = new ColumnHeader();
            colEvVidPid = new ColumnHeader();
            colEvSerial = new ColumnHeader();
            colEvMfr = new ColumnHeader();
            colEvStatus = new ColumnHeader();
            colEvInfo = new ColumnHeader();
            grpDetails = new GroupBox();
            txtDetails = new TextBox();
            pollTimer = new System.Windows.Forms.Timer(components);
            debounceTimer = new System.Windows.Forms.Timer(components);
            uptimeTimer = new System.Windows.Forms.Timer(components);
            toolStrip.SuspendLayout();
            statusStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitMain).BeginInit();
            splitMain.Panel1.SuspendLayout();
            splitMain.Panel2.SuspendLayout();
            splitMain.SuspendLayout();
            grpPorts.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitBottom).BeginInit();
            splitBottom.Panel1.SuspendLayout();
            splitBottom.Panel2.SuspendLayout();
            splitBottom.SuspendLayout();
            grpEvents.SuspendLayout();
            grpDetails.SuspendLayout();
            SuspendLayout();
            // 
            // toolStrip
            // 
            toolStrip.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip.Items.AddRange(new ToolStripItem[] { btnScan, btnClear, btnExport, sep1, btnAutoScroll, btnBeep, btnPolling, lblInterval, cboInterval, sep2, btnDeviceManager });
            toolStrip.Location = new Point(0, 0);
            toolStrip.Name = "toolStrip";
            toolStrip.Padding = new Padding(4, 2, 4, 2);
            toolStrip.Size = new Size(1200, 27);
            toolStrip.TabIndex = 0;
            // 
            // btnScan
            // 
            btnScan.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnScan.Name = "btnScan";
            btnScan.Text = "Scan Now (F5)";
            btnScan.Click += btnScan_Click;
            // 
            // btnClear
            // 
            btnClear.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnClear.Name = "btnClear";
            btnClear.Text = "Clear Log";
            btnClear.Click += btnClear_Click;
            // 
            // btnExport
            // 
            btnExport.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnExport.Name = "btnExport";
            btnExport.Text = "Export CSV...";
            btnExport.Click += btnExport_Click;
            // 
            // sep1
            // 
            sep1.Name = "sep1";
            // 
            // btnAutoScroll
            // 
            btnAutoScroll.Checked = true;
            btnAutoScroll.CheckOnClick = true;
            btnAutoScroll.CheckState = CheckState.Checked;
            btnAutoScroll.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnAutoScroll.Name = "btnAutoScroll";
            btnAutoScroll.Text = "Auto-scroll";
            btnAutoScroll.ToolTipText = "Scroll to the newest event automatically";
            // 
            // btnBeep
            // 
            btnBeep.CheckOnClick = true;
            btnBeep.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnBeep.Name = "btnBeep";
            btnBeep.Text = "Sound";
            btnBeep.ToolTipText = "Play a sound when a port is connected or disconnected";
            // 
            // btnPolling
            // 
            btnPolling.Checked = true;
            btnPolling.CheckOnClick = true;
            btnPolling.CheckState = CheckState.Checked;
            btnPolling.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnPolling.Name = "btnPolling";
            btnPolling.Text = "Polling";
            btnPolling.ToolTipText = "Periodically rescan in addition to Windows device notifications (catches status changes)";
            btnPolling.CheckedChanged += btnPolling_CheckedChanged;
            // 
            // lblInterval
            // 
            lblInterval.Name = "lblInterval";
            lblInterval.Text = "every";
            // 
            // cboInterval
            // 
            cboInterval.DropDownStyle = ComboBoxStyle.DropDownList;
            cboInterval.Items.AddRange(new object[] { "1 s", "2 s", "5 s", "10 s", "30 s" });
            cboInterval.Name = "cboInterval";
            cboInterval.Size = new Size(75, 23);
            cboInterval.SelectedIndexChanged += cboInterval_SelectedIndexChanged;
            // 
            // sep2
            // 
            sep2.Name = "sep2";
            // 
            // btnDeviceManager
            // 
            btnDeviceManager.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnDeviceManager.Name = "btnDeviceManager";
            btnDeviceManager.Text = "Device Manager";
            btnDeviceManager.Click += btnDeviceManager_Click;
            // 
            // statusStrip
            // 
            statusStrip.Items.AddRange(new ToolStripItem[] { lblStatus, lblTrigger, lblLastScan });
            statusStrip.Location = new Point(0, 728);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new Size(1200, 22);
            statusStrip.TabIndex = 2;
            // 
            // lblStatus
            // 
            lblStatus.Name = "lblStatus";
            lblStatus.Text = "Starting...";
            // 
            // lblTrigger
            // 
            lblTrigger.Name = "lblTrigger";
            lblTrigger.Spring = true;
            lblTrigger.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblLastScan
            // 
            lblLastScan.Name = "lblLastScan";
            // 
            // splitMain
            // 
            splitMain.Dock = DockStyle.Fill;
            splitMain.Location = new Point(0, 27);
            splitMain.Name = "splitMain";
            splitMain.Orientation = Orientation.Horizontal;
            // 
            // splitMain.Panel1
            // 
            splitMain.Panel1.Controls.Add(grpPorts);
            splitMain.Panel1.Padding = new Padding(4);
            // 
            // splitMain.Panel2
            // 
            splitMain.Panel2.Controls.Add(splitBottom);
            splitMain.Panel2.Padding = new Padding(4);
            splitMain.Size = new Size(1200, 701);
            splitMain.SplitterDistance = 250;
            splitMain.TabIndex = 1;
            // 
            // grpPorts
            // 
            grpPorts.Controls.Add(lvPorts);
            grpPorts.Dock = DockStyle.Fill;
            grpPorts.Name = "grpPorts";
            grpPorts.Text = "Current COM Ports";
            // 
            // lvPorts
            // 
            lvPorts.Columns.AddRange(new ColumnHeader[] { colPort, colName, colStatus, colVid, colPid, colSerial, colMfr, colBus, colDriver, colSince, colLocation });
            lvPorts.Dock = DockStyle.Fill;
            lvPorts.FullRowSelect = true;
            lvPorts.GridLines = true;
            lvPorts.MultiSelect = false;
            lvPorts.Name = "lvPorts";
            lvPorts.TabIndex = 0;
            lvPorts.UseCompatibleStateImageBehavior = false;
            lvPorts.View = View.Details;
            lvPorts.SelectedIndexChanged += lvPorts_SelectedIndexChanged;
            // 
            // column headers (ports)
            // 
            colPort.Text = "Port";
            colPort.Width = 65;
            colName.Text = "Device Name";
            colName.Width = 260;
            colStatus.Text = "Status";
            colStatus.Width = 90;
            colVid.Text = "VID";
            colVid.Width = 50;
            colPid.Text = "PID";
            colPid.Width = 50;
            colSerial.Text = "Serial #";
            colSerial.Width = 110;
            colMfr.Text = "Manufacturer";
            colMfr.Width = 130;
            colBus.Text = "Bus";
            colBus.Width = 85;
            colDriver.Text = "Driver";
            colDriver.Width = 85;
            colSince.Text = "Connected For";
            colSince.Width = 95;
            colLocation.Text = "Location";
            colLocation.Width = 160;
            // 
            // splitBottom
            // 
            splitBottom.Dock = DockStyle.Fill;
            splitBottom.Name = "splitBottom";
            // 
            // splitBottom.Panel1
            // 
            splitBottom.Panel1.Controls.Add(grpEvents);
            // 
            // splitBottom.Panel2
            // 
            splitBottom.Panel2.Controls.Add(grpDetails);
            splitBottom.Size = new Size(1192, 439);
            splitBottom.SplitterDistance = 820;
            splitBottom.TabIndex = 0;
            // 
            // grpEvents
            // 
            grpEvents.Controls.Add(lvEvents);
            grpEvents.Dock = DockStyle.Fill;
            grpEvents.Name = "grpEvents";
            grpEvents.Text = "Event Timeline";
            // 
            // lvEvents
            // 
            lvEvents.Columns.AddRange(new ColumnHeader[] { colEvTime, colEvDelta, colEvType, colEvPort, colEvName, colEvVidPid, colEvSerial, colEvMfr, colEvStatus, colEvInfo });
            lvEvents.Dock = DockStyle.Fill;
            lvEvents.FullRowSelect = true;
            lvEvents.GridLines = true;
            lvEvents.MultiSelect = false;
            lvEvents.Name = "lvEvents";
            lvEvents.TabIndex = 0;
            lvEvents.UseCompatibleStateImageBehavior = false;
            lvEvents.View = View.Details;
            lvEvents.SelectedIndexChanged += lvEvents_SelectedIndexChanged;
            // 
            // column headers (events)
            // 
            colEvTime.Text = "Time";
            colEvTime.Width = 95;
            colEvDelta.Text = "+Δ";
            colEvDelta.Width = 70;
            colEvType.Text = "Event";
            colEvType.Width = 100;
            colEvPort.Text = "Port";
            colEvPort.Width = 60;
            colEvName.Text = "Device Name";
            colEvName.Width = 220;
            colEvVidPid.Text = "VID:PID";
            colEvVidPid.Width = 80;
            colEvSerial.Text = "Serial #";
            colEvSerial.Width = 100;
            colEvMfr.Text = "Manufacturer";
            colEvMfr.Width = 110;
            colEvStatus.Text = "Status";
            colEvStatus.Width = 80;
            colEvInfo.Text = "Info";
            colEvInfo.Width = 250;
            // 
            // grpDetails
            // 
            grpDetails.Controls.Add(txtDetails);
            grpDetails.Dock = DockStyle.Fill;
            grpDetails.Name = "grpDetails";
            grpDetails.Text = "Details";
            // 
            // txtDetails
            // 
            txtDetails.Dock = DockStyle.Fill;
            txtDetails.Font = new Font("Consolas", 9F);
            txtDetails.Multiline = true;
            txtDetails.Name = "txtDetails";
            txtDetails.ReadOnly = true;
            txtDetails.ScrollBars = ScrollBars.Both;
            txtDetails.TabIndex = 0;
            txtDetails.WordWrap = false;
            // 
            // pollTimer
            // 
            pollTimer.Interval = 2000;
            pollTimer.Tick += pollTimer_Tick;
            // 
            // debounceTimer
            // 
            debounceTimer.Interval = 400;
            debounceTimer.Tick += debounceTimer_Tick;
            // 
            // uptimeTimer
            // 
            uptimeTimer.Interval = 1000;
            uptimeTimer.Tick += uptimeTimer_Tick;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1200, 750);
            Controls.Add(splitMain);
            Controls.Add(statusStrip);
            Controls.Add(toolStrip);
            KeyPreview = true;
            MinimumSize = new Size(700, 400);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "COM Port Scanner";
            Load += Form1_Load;
            KeyDown += Form1_KeyDown;
            toolStrip.ResumeLayout(false);
            toolStrip.PerformLayout();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            splitMain.Panel1.ResumeLayout(false);
            splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitMain).EndInit();
            splitMain.ResumeLayout(false);
            grpPorts.ResumeLayout(false);
            splitBottom.Panel1.ResumeLayout(false);
            splitBottom.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitBottom).EndInit();
            splitBottom.ResumeLayout(false);
            grpEvents.ResumeLayout(false);
            grpDetails.ResumeLayout(false);
            grpDetails.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ToolStrip toolStrip;
        private ToolStripButton btnScan;
        private ToolStripButton btnClear;
        private ToolStripButton btnExport;
        private ToolStripSeparator sep1;
        private ToolStripButton btnAutoScroll;
        private ToolStripButton btnBeep;
        private ToolStripButton btnPolling;
        private ToolStripLabel lblInterval;
        private ToolStripComboBox cboInterval;
        private ToolStripSeparator sep2;
        private ToolStripButton btnDeviceManager;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel lblStatus;
        private ToolStripStatusLabel lblTrigger;
        private ToolStripStatusLabel lblLastScan;
        private SplitContainer splitMain;
        private GroupBox grpPorts;
        private ListView lvPorts;
        private ColumnHeader colPort;
        private ColumnHeader colName;
        private ColumnHeader colStatus;
        private ColumnHeader colVid;
        private ColumnHeader colPid;
        private ColumnHeader colSerial;
        private ColumnHeader colMfr;
        private ColumnHeader colBus;
        private ColumnHeader colDriver;
        private ColumnHeader colSince;
        private ColumnHeader colLocation;
        private SplitContainer splitBottom;
        private GroupBox grpEvents;
        private ListView lvEvents;
        private ColumnHeader colEvTime;
        private ColumnHeader colEvDelta;
        private ColumnHeader colEvType;
        private ColumnHeader colEvPort;
        private ColumnHeader colEvName;
        private ColumnHeader colEvVidPid;
        private ColumnHeader colEvSerial;
        private ColumnHeader colEvMfr;
        private ColumnHeader colEvStatus;
        private ColumnHeader colEvInfo;
        private GroupBox grpDetails;
        private TextBox txtDetails;
        private System.Windows.Forms.Timer pollTimer;
        private System.Windows.Forms.Timer debounceTimer;
        private System.Windows.Forms.Timer uptimeTimer;
    }
}
