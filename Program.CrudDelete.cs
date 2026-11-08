using IoTTelemetryLab.Data;

namespace IoTTelemetryLab;

public static class CrudDelete
{
    public static void Run(AppDbContext context)
    {
        while (true)
        {
            // === Перелік сенсорів з кількістю показів ===
            var devices = context.Devices.OrderBy(d => d.Id).ToList();

            if (devices.Count == 0)
            {
                Console.WriteLine("У базі немає жодного сенсора.");
                return;
            }

            Console.WriteLine("\n=== Сенсори в базі даних ===");
            foreach (var d in devices)
            {
                var count = context.Records.Count(r => r.DeviceId == d.Id);
                Console.WriteLine($"  [{d.Id}] {d.Name} (показів: {count})");
            }

            // === Вибір сенсора ===
            Console.Write("\nВведіть Id сенсора для видалення (порожній рядок - вихід): ");
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

            var device = context.Devices.FirstOrDefault(d => d.Id == id);
            if (device is null)
            {
                Console.WriteLine($"  Сенсора з Id={id} у базі немає.");
                continue;
            }

            // === Підтвердження: показуємо наслідки ДО видалення ===
            var recordsCount = context.Records.Count(r => r.DeviceId == id);
            Console.WriteLine($"  Буде видалено сенсор '{device.Name}' та {recordsCount} пов'язаних показів.");
            Console.Write("  Підтвердити видалення? (y/n): ");

            var confirm = Console.ReadLine()?.Trim().ToLowerInvariant();
            if (confirm != "y")
            {
                Console.WriteLine("  Видалення скасовано.");
                continue;
            }

            var totalBefore = context.Records.Count();
            var deviceName = device.Name;

            // === DELETE ===
            // Видаляємо лише Device. Записи телеметрії видалить сама база
            // завдяки каскаду (ON DELETE CASCADE) - в межах тієї ж транзакції
            context.Devices.Remove(device);
            context.SaveChanges();

            // === Перевірка цілісності ===
            var totalAfter = context.Records.Count();
            var ownRecordsLeft = context.Records.Count(r => r.DeviceId == id);
            var orphans = context.Records.Count(r => !context.Devices.Any(d => d.Id == r.DeviceId));

            Console.WriteLine($"\n  Сенсор '{deviceName}' видалено.");
            Console.WriteLine($"  Усього показів у базі: було {totalBefore}, стало {totalAfter} (видалено {totalBefore - totalAfter}).");
            Console.WriteLine($"  Показів видаленого сенсора лишилось: {ownRecordsLeft}.");
            Console.WriteLine($"  Записів без батьківського сенсора (сиріт): {orphans}.");

            if (ownRecordsLeft == 0 && orphans == 0)
            {
                Console.WriteLine("  Цілісність бази збережено.");
            }
            else
            {
                Console.WriteLine("  УВАГА: виявлено порушення цілісності!");
            }
        }
    }
}
