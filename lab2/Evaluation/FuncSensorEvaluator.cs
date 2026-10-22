using IoTTelemetryLab.Models;

namespace IoTTelemetryLab.Evaluation;

public class FuncSensorEvaluator : ISensorEvaluator
{
    private readonly Func<double, SafetyStatus> _evaluatorFunc;

    public FuncSensorEvaluator(Func<double, SafetyStatus> evaluatorFunc)
    {
        _evaluatorFunc = evaluatorFunc;
    }

    public SafetyStatus Evaluate(double value) => _evaluatorFunc(value);
}
