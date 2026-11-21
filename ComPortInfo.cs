using System.Text;

namespace ComScanner
{
    /// <summary>
    /// Snapshot of a single COM port and the device behind it.
    /// </summary>
    public sealed record ComPortInfo
    {
        public required string PortName { get; init; }
        public string FriendlyName { get; init; } = "";
        public string Description { get; init; } = "";
        public string Manufacturer { get; init; } = "";
        public string PnpDeviceId { get; init; } = "";
        public string Service { get; init; } = "";
        public string Status { get; init; } = "";
        public int? ErrorCode { get; init; }
        public string Vid { get; init; } = "";
        public string Pid { get; init; } = "";
        public string SerialNumber { get; init; } = "";
        public string Bus { get; init; } = "";
        public string LocationInfo { get; init; } = "";
        public string DeviceObjectName { get; init; } = "";

        public int PortNumber =>
            PortName.StartsWith("COM", StringComparison.OrdinalIgnoreCase) &&
            int.TryParse(PortName.AsSpan(3), out var n) ? n : int.MaxValue;

        public string VidPid => Vid.Length > 0 || Pid.Length > 0 ? $"{Vid}:{Pid}" : "";

        public string DisplayName => FriendlyName.Length > 0 ? FriendlyName : Description;

        public string StatusText => ErrorCode is null or 0
            ? (Status.Length > 0 ? Status : "OK")
            : $"{Status} (Code {ErrorCode}: {DescribeErrorCode(ErrorCode.Value)})";

        public bool HasProblem => ErrorCode is > 0 || (Status.Length > 0 && !Status.Equals("OK", StringComparison.OrdinalIgnoreCase));

        public string ToDetailsText()
        {
            var sb = new StringBuilder();
            void Line(string label, string value) => sb.AppendLine($"{label,-18}: {(string.IsNullOrEmpty(value) ? "—" : value)}");
            Line("Port", PortName);
            Line("Friendly Name", FriendlyName);
            Line("Description", Description);
            Line("Manufacturer", Manufacturer);
            Line("Status", StatusText);
            Line("VID", Vid);
            Line("PID", Pid);
            Line("Serial Number", SerialNumber);
            Line("Bus", Bus);
            Line("Location", LocationInfo);
            Line("Driver Service", Service);
            Line("Kernel Device", DeviceObjectName);
            Line("PnP Device ID", PnpDeviceId);
            return sb.ToString();
        }

        public static string DescribeErrorCode(int code) => code switch
        {
            0 => "Working properly",
            1 => "Not configured correctly",
            3 => "Driver corrupted / low resources",
            10 => "Device cannot start",
            12 => "Not enough free resources",
            14 => "Restart required",
            18 => "Reinstall drivers",
            19 => "Registry configuration problem",
            21 => "Being removed",
            22 => "Device is disabled",
            24 => "Not present / missing drivers",
            28 => "Drivers not installed",
            29 => "Disabled by firmware",
            31 => "Driver could not be loaded",
            37 => "Driver failed to initialize",
            39 => "Driver corrupted or missing",
            43 => "Device reported a problem (stopped)",
            45 => "Device not connected",
            47 => "Safe removal prepared",
            48 => "Driver blocked",
            52 => "Driver signature cannot be verified",
            _ => "See Device Manager",
        };
    }

    public enum PortEventType
    {
        Initial,
        Connected,
        Disconnected,
        Replaced,
        StatusChanged,
        Error,
    }

    public sealed record PortEvent(DateTime Timestamp, PortEventType Type, string PortName, ComPortInfo? Device, string Info, TimeSpan? SincePrevious)
    {
        public string TypeText => Type switch
        {
            PortEventType.Initial => "Present",
            PortEventType.Connected => "Connected",
            PortEventType.Disconnected => "Disconnected",
            PortEventType.Replaced => "Device Replaced",
            PortEventType.StatusChanged => "Status Changed",
            PortEventType.Error => "Error",
            _ => Type.ToString(),
        };
    }
}
