using IoTTelemetryLab.Models;
using IoTTelemetryLab.Evaluation;

namespace IoTTelemetryLab.Processing;

/// <summary>
/// Виконує аналітичні LINQ-запити над очищеними показами датчика.
/// </summary>
public static class TelemetryAnalytics
{
    /// <summary>
    /// Підраховує покази із заданим рівнем небезпеки.
    /// </summary>
    public static int CountByStatus(
        IEnumerable<TelemetryReading> readings,
        SensorType type,
        SafetyStatus target,
        SensorEvaluatorRegistry registry)
    {
        return readings.Count(r => registry.Evaluate(type, r.Value) == target);
    }

    // Ковзне середнє за вікном N показів — типова аналітична задача для телеметрії
    /// <summary>
    /// Обчислює ковзне середнє за вікном із windowSize послідовних показів.
    /// </summary>
    public static IEnumerable<double> MovingAverage(
        IEnumerable<TelemetryReading> readings, int windowSize)
    {
        var values = readings.Select(r => r.Value).ToList();

        for (int i = 0; i <= values.Count - windowSize; i++)
        {
            yield return values.Skip(i).Take(windowSize).Average();
        }
    }

    /// <summary>
    /// Знаходить час першого критичного показу у впорядкованій за часом послідовності.
    /// Where відбирає Critical, Select отримує час, FirstOrDefault повертає перший результат.
    /// </summary>
    /// <returns>Час першого критичного показу або null, якщо таких показів немає.</returns>
    public static DateTime? FirstCriticalTimestamp(
        IEnumerable<TelemetryReading> readings, SensorType type,
        SensorEvaluatorRegistry registry)
    {
        return readings
            .Where(r => registry.Evaluate(type, r.Value) == SafetyStatus.Critical)
            .Select(r => (DateTime?)r.Timestamp)
            .FirstOrDefault();
    }
}
