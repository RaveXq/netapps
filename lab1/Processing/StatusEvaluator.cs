using IoTTelemetryLab.Models;

namespace IoTTelemetryLab.Processing;

/// <summary>
/// Визначає рівень небезпеки показу датчика.
/// </summary>
public enum SafetyStatus { Normal, Warning, Critical }

/// <summary>
/// Оцінює стан датчика за типом фізичної величини та її значенням.
/// </summary>
public static class StatusEvaluator
{
    /// <summary>
    /// Повертає рівень небезпеки очищеного показу через вкладені switch-вирази.
    /// </summary>
    public static SafetyStatus Evaluate(SensorType type, double value) => type switch
    {
        SensorType.Temperature => value switch
        {
            < 0 or > 80 => SafetyStatus.Critical,
            >= 60 and <= 80 => SafetyStatus.Warning,
            _ => SafetyStatus.Normal
        },
        SensorType.Vibration => value switch
        {
            > 5.0 => SafetyStatus.Critical,
            > 2.5 => SafetyStatus.Warning,
            _ => SafetyStatus.Normal
        },
        SensorType.Voltage => value switch
        {
            < 200 or > 240 => SafetyStatus.Critical,
            < 210 or > 230 => SafetyStatus.Warning,
            _ => SafetyStatus.Normal
        },
        // За умовою варіанта: понад 85 дБ — підвищений шум, понад 100 дБ — критичний.
        SensorType.SoundLevel => value switch
        {
            > 100 => SafetyStatus.Critical,
            > 85 and <= 100 => SafetyStatus.Warning,
            _ => SafetyStatus.Normal
        },
        _ => throw new ArgumentOutOfRangeException(nameof(type))
    };
}
