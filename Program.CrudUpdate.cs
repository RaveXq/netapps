using IoTTelemetryLab.Data;

namespace IoTTelemetryLab;

public static class CrudUpdate
{
    public static void Run(AppDbContext context)
    {
        // Має збігатися з обмеженням у Fluent API (HasMaxLength(100) в AppDbContext)
        const int MaxNameLength = 100;

        while (true)
        {
            // === Перелік сенсорів ===
            var devices = context.Devices.OrderBy(d => d.Id).ToList();

            if (devices.Count == 0)
            {
                Console.WriteLine("У базі ще немає жодного сенсора. Спочатку наповніть базу (Крок 5).");
                return;
            }

            Console.WriteLine("\n=== Сенсори в базі даних ===");
            foreach (var d in devices)
            {
                Console.WriteLine($"  [{d.Id}] {d.Name}");
            }

            // === Вибір сенсора ===
            Console.Write("\nВведіть Id сенсора для перейменування (порожній рядок - вихід): ");
            var idInput = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(idInput))
            {
                break;
            }

            if (!int.TryParse(idInput, out var id))
            {
                Console.WriteLine("  Потрібно ввести число - Id зі списку вище.");
                continue;
            }

            // Сутність завантажується через контекст, тому Change Tracker її відстежує
            var device = context.Devices.FirstOrDefault(d => d.Id == id);
            if (device is null)
            {
                Console.WriteLine($"  Сенсора з Id={id} у базі немає.");
                continue;
            }

            // === Введення нової назви ===
            Console.WriteLine($"  Поточна назва: '{device.Name}'");
            Console.Write("  Нова назва (порожній рядок - скасувати): ");
            var newName = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(newName))
            {
                Console.WriteLine("  Перейменування скасовано.");
                continue;
            }

            if (newName.Length > MaxNameLength)
            {
                Console.WriteLine($"  Назва задовга: максимум {MaxNameLength} символів.");
                continue;
            }

            if (newName == device.Name)
            {
                Console.WriteLine("  Нова назва збігається з поточною, змін немає.");
                continue;
            }

            // Дублікат шукаємо серед УСІХ ІНШИХ сенсорів (d.Id != id),
            // інакше сам сенсор вважався б конфліктом із власною назвою
            var nameTaken = context.Devices.Any(d => d.Name == newName && d.Id != id);
            if (nameTaken)
            {
                Console.WriteLine($"  Сенсор з назвою '{newName}' уже існує.");
                continue;
            }

            // === UPDATE ===
            var oldName = device.Name;
            device.Name = newName;   // змінюємо властивість об'єкта в пам'яті
            context.SaveChanges();   // лише тепер зміна потрапляє в базу

            Console.WriteLine($"  Готово: '{oldName}' -> '{device.Name}'");
        }
    }
}
