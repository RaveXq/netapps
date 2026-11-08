using IoTTelemetryLab.Data;

namespace IoTTelemetryLab;

public static class IndividualTask
{
    public static void Run(AppDbContext context)
    {
        const double Threshold = 85;

        Console.Write("Джерело шуму (вентилятор, двигун, фон): ");
        var source = Console.ReadLine()?.Trim().ToLowerInvariant();
        if (source != "вентилятор" && source != "двигун" && source != "фон")
        {
            Console.WriteLine("  Введіть одне з джерел: вентилятор, двигун, фон.");
            return;
        }

        var query = context.Records.Where(r => r.Source == source);
        var total = query.Count();
        if (total == 0)
        {
            Console.WriteLine("  Немає даних для обраного джерела.");
            return;
        }

        var aboveThreshold = query.Count(r => r.Value > Threshold);
        var percentage = 100.0 * aboveThreshold / total;
        Console.WriteLine($"  Джерело: {source}");
        Console.WriteLine($"  Показів вище {Threshold} дБ: {aboveThreshold} з {total} ({percentage:F2}%).");
    }
}
