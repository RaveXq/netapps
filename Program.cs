using System.Text;
using IoTTelemetryLab.Data;
using IoTTelemetryLab.Evaluation;
using IoTTelemetryLab.Models;
using IoTTelemetryLab.Monitoring;
using IoTTelemetryLab.Monitoring.Subscribers;
using IoTTelemetryLab.Processing;

Console.OutputEncoding = Encoding.UTF8;

const string deviceId = "sensor-07";
const SensorType sensorType = SensorType.RotationSpeed;

Console.WriteLine("Лабораторна робота 2. Варіант 5 - швидкість обертання, RPM");

var registry = EvaluatorSetup.CreateDefaultRegistry();
registry.Register(sensorType, new RotationSpeedEvaluator());
var monitor = new TelemetryMonitor(registry, sensorType);

var counter = new CriticalCounterSubscriber();
var hourlySubscriber = new HourlyCriticalSubscriber();

monitor.OnStatusEvaluated += ConsoleAlertSubscriber.OnStatusEvaluated;
monitor.OnStatusEvaluated += counter.OnStatusEvaluated;
monitor.OnStatusEvaluated += hourlySubscriber.OnStatusEvaluated;

var rawData = TelemetryGenerator.GenerateRawBatch(deviceId, count: 50,
    minValue: 0, maxValue: 3500);
var cleaned = TelemetryCleaner.RemoveInvalid(rawData).ToList();
Console.WriteLine($"Отримано показів: {rawData.Count}");
Console.WriteLine($"Після очищення: {cleaned.Count}");

var classStatuses = cleaned.Select(r => registry.Evaluate(sensorType, r.Value)).ToList();

registry.Register(sensorType, value => value switch
{
    < 500 or > 3000 => SafetyStatus.Critical,
    < 800 or > 2500 => SafetyStatus.Warning,
    _ => SafetyStatus.Normal
});

var funcStatuses = cleaned.Select(r => registry.Evaluate(sensorType, r.Value)).ToList();
Console.WriteLine($"Оцінювач-клас і Func дають однакові результати: {classStatuses.SequenceEqual(funcStatuses)}");

foreach (var reading in cleaned)
{
    monitor.Process(reading);
}

Console.WriteLine($"\nВсього критичних станів зафіксовано лічильником: {counter.CriticalCount}");
Console.WriteLine("Критичні покази за годинами доби (UTC):");

foreach (var group in hourlySubscriber.CriticalReadingsByHour.OrderBy(g => g.Key))
{
    Console.WriteLine($"{group.Key:00}:00 — {group.Value.Count}");
}
