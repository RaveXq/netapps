using IoTTelemetryLab.Models;

namespace IoTTelemetryLab.Evaluation;

public class RotationSpeedEvaluator : ISensorEvaluator
{
    public SafetyStatus Evaluate(double value) => value switch
    {
        < 500 or > 3000 => SafetyStatus.Critical,
        < 800 or > 2500 => SafetyStatus.Warning,
        _ => SafetyStatus.Normal
    };
}
