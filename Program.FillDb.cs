using IoTTelemetryLab.Data;

namespace IoTTelemetryLab;

public static class FillDb
{
    // Параметри генерації показів (можна змінити під свій варіант)
    private const int ReadingCount = 50;
    private const double MinValue = 30;
    private const double MaxValue = 120;

    public static void Seed(AppDbContext context)
    {
        const int SensorCount = 5;

        // === Наповнення: 5 сенсорів по ~50 показів ===
        Console.WriteLine("=== Наповнення бази ===");

        for (var i = 1; i <= SensorCount; i++)
        {
            var name = $"sensor-{i:D2}";

            // Не створюємо дублікат, якщо програму запускають повторно
            if (context.Devices.Any(d => d.Name == name))
            {
                Console.WriteLine($"  '{name}' уже є в базі, пропускаємо.");
                continue;
            }

            DeviceSeeder.SeedDevice(context, name, ReadingCount, MinValue, MaxValue);

            // Фактичну кількість беремо з бази: генератор сам відсіює частину показів
            var saved = context.Records.Count(r => r.Device.Name == name);
            Console.WriteLine($"  '{name}': згенеровано {ReadingCount}, збережено {saved}");
        }
    }

    public static void AddDevice(AppDbContext context)
    {
        Console.WriteLine("=== Наповнення бази телеметрією ===");
        Console.WriteLine("Введіть назву сенсора (порожній рядок - завершити введення).\n");

        while (true)
        {
            Console.Write("Назва сенсора: ");
            var name = Console.ReadLine()?.Trim();

            // Порожній ввід - вихід з циклу
            if (string.IsNullOrEmpty(name))
            {
                break;
            }

            // Перевірка: не створюємо дублікат пристрою з такою самою назвою
            var exists = context.Devices.Any(d => d.Name == name);
            if (exists)
            {
                Console.WriteLine($"  Сенсор '{name}' уже є в базі, пропускаємо.\n");
                continue;
            }

            DeviceSeeder.SeedDevice(context, name, ReadingCount, MinValue, MaxValue);
            var savedCount = context.Records.Count(r => r.Device.Name == name);
            Console.WriteLine($"  Сенсор '{name}' створено, додано показів до БД: {savedCount}.\n");
        }

        // === Підсумок: що зараз є в базі ===
        var devices = context.Devices.ToList();
        Console.WriteLine($"\nУсього сенсорів у базі: {devices.Count}");

        foreach (var device in devices)
        {
            // Рахуємо записи окремим запитом до Records:
            // device.Records без .Include() порожня (це тема Л7)
            var recordsCount = context.Records.Count(r => r.DeviceId == device.Id);
            Console.WriteLine($"  Id={device.Id}, Name={device.Name}, показів: {recordsCount}");
        }
    }
}
