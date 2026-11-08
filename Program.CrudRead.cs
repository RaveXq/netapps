using IoTTelemetryLab.Data;

namespace IoTTelemetryLab;

public static class CrudRead
{
    public static void Run(AppDbContext context)
    {
        // === ЧАСТИНА 1: перелік усіх сенсорів у базі ===
        var devices = context.Devices.OrderBy(d => d.Id).ToList();

        if (devices.Count == 0)
        {
            Console.WriteLine("У базі ще немає жодного сенсора. Спочатку наповніть базу (Крок 5).");
            return;
        }

        Console.WriteLine("=== Сенсори в базі даних ===");
        foreach (var device in devices)
        {
            // Кількість показів рахуємо окремим запитом: device.Records без .Include() порожня
            var count = context.Records.Count(r => r.DeviceId == device.Id);
            Console.WriteLine($"  [{device.Id}] {device.Name} (показів: {count})");
        }

        // === ЧАСТИНА 2: вибір сенсора та перегляд його показів ===
        while (true)
        {
            Console.Write("\nВведіть Id сенсора (порожній рядок - вихід): ");
            var input = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(input))
            {
                break;
            }

            if (!int.TryParse(input, out var id))
            {
                Console.WriteLine("  Потрібно ввести число - Id зі списку вище.");
                continue;
            }

            var selected = context.Devices.FirstOrDefault(d => d.Id == id);
            if (selected is null)
            {
                Console.WriteLine($"  Сенсора з Id={id} у базі немає.");
                continue;
            }

            // Запит до Records напряму: IQueryable транслюється в SQL
            // (WHERE DeviceId = ... ORDER BY Timestamp), а не фільтрується в пам'яті
            var readings = context.Records
                .Where(r => r.DeviceId == selected.Id)
                .OrderBy(r => r.Timestamp)
                .ToList();

            Console.WriteLine($"\n--- Покази сенсора '{selected.Name}' ---");

            if (readings.Count == 0)
            {
                Console.WriteLine("  Показів немає.");
            }
            else
            {
                for (var i = 0; i < readings.Count; i++)
                {
                    var r = readings[i];
                    Console.WriteLine($"  {i + 1,3}. {r.Timestamp:yyyy-MM-dd HH:mm:ss}  {r.Value,8:F2}");
                }
            }

            Console.WriteLine($"Усього показів: {readings.Count}");
        }
    }
}
