using IoTTelemetryLab.Models;
using IoTTelemetryLab.Processing;

namespace IoTTelemetryLab.Data;

public static class DeviceSeeder
{
    private static readonly string[] Sources = { "вентилятор", "двигун", "фон" };

    public static void SeedDevice(AppDbContext context, string deviceName,
        int readingCount, double minValue, double maxValue)
    {
        var device = new Device { Name = deviceName };
        context.Devices.Add(device);
        context.SaveChanges();

        var rawData = TelemetryGenerator.GenerateRawBatch(
            deviceName, readingCount, minValue, maxValue);
        var cleaned = TelemetryCleaner.RemoveInvalid(rawData);

        foreach (var reading in cleaned)
        {
            device.Records.Add(new TelemetryRecord
            {
                Value = reading.Value,
                Timestamp = reading.Timestamp,
                DeviceId = device.Id,
                Source = Sources[Random.Shared.Next(Sources.Length)] // випадковий елемент з набору
            });
        }

        context.SaveChanges();
    }
}
