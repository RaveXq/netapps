namespace IoTTelemetryLab.Models;

// readonly record struct: незмінний, легкий (value type) — ідеально для великої кількості короткоживучих показів з датчиків
/// <summary>
/// Зберігає ідентифікатор датчика, значення показу та час вимірювання в UTC.
/// </summary>
public readonly record struct TelemetryReading(
    string DeviceId,
    double Value,
    DateTime Timestamp);
