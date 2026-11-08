using System.Text;
using IoTTelemetryLab.Data;
using IoTTelemetryLab.Models;
using IoTTelemetryLab.Processing;

Console.OutputEncoding = Encoding.UTF8;

const string deviceId = "sensor-07";
const SensorType sensorType = SensorType.SoundLevel;

Console.WriteLine("Лабораторна робота 1. Варіант 5 - рівень шуму, дБ");

// Генеруємо "сирі" дані з апаратними помилками
var rawData = TelemetryGenerator.GenerateRawBatch(deviceId, count: 50,
    minValue: 60, maxValue: 130);
Console.WriteLine($"Отримано показів: {rawData.Count}");

// Базова частина: очищення
var cleaned = TelemetryCleaner.RemoveInvalid(rawData).ToList();
Console.WriteLine($"Після очищення: {cleaned.Count}");

DateTime? firstCritical = TelemetryAnalytics.FirstCriticalTimestamp(cleaned, sensorType);

if (firstCritical.HasValue)
{
    Console.WriteLine($"Час першого критичного показу (UTC): {firstCritical.Value:dd.MM.yyyy HH:mm:ss}");
}
else
{
    Console.WriteLine("Критичних показів не виявлено.");
}
