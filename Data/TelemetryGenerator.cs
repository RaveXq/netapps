using IoTTelemetryLab.Models;

namespace IoTTelemetryLab.Data;

/// <summary>
/// Генерує тестові покази датчика з апаратними помилками.
/// </summary>
public static class TelemetryGenerator
{
    /// <summary>
    /// Створює покази з інтервалом 5 секунд; приблизно 10% містять -999 або NaN.
    /// </summary>
    /// <param name="deviceId">Ідентифікатор датчика.</param>
    /// <param name="count">Кількість показів.</param>
    /// <param name="minValue">Нижня межа діапазону коректних значень.</param>
    /// <param name="maxValue">Верхня межа діапазону коректних значень.</param>
    /// <returns>Список показів у порядку часу вимірювання.</returns>
    public static List<TelemetryReading> GenerateRawBatch(
        string deviceId, int count, double minValue, double maxValue)
    {
        var random = new Random();
        var readings = new List<TelemetryReading>();
        var baseTime = DateTime.UtcNow;

        for (int i = 0; i < count; i++)
        {
            // ~10% показів — апаратна помилка
            double value = random.NextDouble() < 0.1
                ? (random.Next(2) == 0 ? -999.0 : double.NaN)
                : Math.Round(minValue + random.NextDouble() * (maxValue - minValue), 2);

            readings.Add(new TelemetryReading(
                DeviceId: deviceId,
                Value: value,
                Timestamp: baseTime.AddSeconds(i * 5)));
        }

        return readings;
    }
}
