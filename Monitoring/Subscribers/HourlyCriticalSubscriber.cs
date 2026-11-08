using IoTTelemetryLab.Models;

namespace IoTTelemetryLab.Monitoring.Subscribers;

public class HourlyCriticalSubscriber
{
    public Dictionary<int, List<TelemetryReading>> CriticalReadingsByHour { get; } = new();

    public void OnStatusEvaluated(TelemetryReading reading, SafetyStatus status)
    {
        if (status == SafetyStatus.Critical)
        {
            int hour = reading.Timestamp.Hour;

            if (!CriticalReadingsByHour.ContainsKey(hour))
            {
                CriticalReadingsByHour[hour] = new List<TelemetryReading>();
            }

            CriticalReadingsByHour[hour].Add(reading);
        }
    }
}
