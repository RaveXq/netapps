using IoTTelemetryLab.Models;

namespace IoTTelemetryLab.Evaluation;

public class SoundLevelEvaluator : ISensorEvaluator
{
    public SafetyStatus Evaluate(double value) => value switch
    {
        > 100 => SafetyStatus.Critical,
        > 85 and <= 100 => SafetyStatus.Warning,
        _ => SafetyStatus.Normal
    };
}
