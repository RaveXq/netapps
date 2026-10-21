using IoTTelemetryLab.Models;

namespace IoTTelemetryLab.Processing;

/// <summary>
/// Очищує покази датчика від апаратних помилок.
/// </summary>
public static class TelemetryCleaner
{
    private const double ErrorMarker = -999.0;

    /// <summary>
    /// Відбирає покази без NaN та маркера помилки -999 за допомогою Where.
    /// </summary>
    public static IEnumerable<TelemetryReading> RemoveInvalid(
        IEnumerable<TelemetryReading> raw)
    {
        return raw.Where(r =>
            !double.IsNaN(r.Value) &&
            r.Value != ErrorMarker);
    }
}
