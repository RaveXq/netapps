namespace IoTTelemetryLab.Models;

public class TelemetryRecord
{
    public int Id { get; set; }
    public double Value { get; set; }
    public DateTime Timestamp { get; set; }
    public string Source { get; set; } = string.Empty;
    public int DeviceId { get; set; }
    public Device Device { get; set; } = null!;
}
