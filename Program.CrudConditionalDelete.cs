using System.Globalization;
using IoTTelemetryLab.Data;
using IoTTelemetryLab.Models;

namespace IoTTelemetryLab;

public static class CrudConditionalDelete
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

            // === Крок 1: область видалення ===
            Console.Write("\nId сенсора (0 - усі сенсори, порожній рядок - вихід): ");
            var scopeInput = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(scopeInput))
            {
                break;
            }

            if (!int.TryParse(scopeInput, out var scopeId))
            {
                Console.WriteLine("  Потрібно ввести число.");
                continue;
            }

            // IQueryable - це лише ОПИС запиту, до бази він ще не звертався
            // (відкладене виконання, Л4). Умови додаються поступово.
            IQueryable<TelemetryRecord> query = context.Records;
            string scopeTitle = "усіх сенсорів";

            if (scopeId != 0)
            {
                var selected = devices.FirstOrDefault(d => d.Id == scopeId);
                if (selected is null)
                {
                    Console.WriteLine($"  Сенсора з Id={scopeId} у базі немає.");
                    continue;
                }

                query = query.Where(r => r.DeviceId == scopeId);
                scopeTitle = $"сенсора '{selected.Name}'";
            }

            // === Крок 2: умова видалення ===
            Console.WriteLine("\nУмова видалення показів:");
            Console.WriteLine("  1 - значення нижче порога");
            Console.WriteLine("  2 - значення вище порога");
            Console.WriteLine("  3 - значення в діапазоні [від; до]");
            Console.WriteLine("  4 - покази, зафіксовані раніше за вказану дату");
            Console.Write("Оберіть умову (1-4): ");
            var choice = Console.ReadLine()?.Trim();

            string conditionTitle;

            switch (choice)
            {
                case "1":
                    if (!TryReadDouble("  Поріг (видалити покази < поріг): ", out var below)) continue;
                    query = query.Where(r => r.Value < below);
                    conditionTitle = $"значення < {below.ToString(CultureInfo.InvariantCulture)}";
                    break;

                case "2":
                    if (!TryReadDouble("  Поріг (видалити покази > поріг): ", out var above)) continue;
                    query = query.Where(r => r.Value > above);
                    conditionTitle = $"значення > {above.ToString(CultureInfo.InvariantCulture)}";
                    break;

                case "3":
                    if (!TryReadDouble("  Нижня межа діапазону: ", out var min)) continue;
                    if (!TryReadDouble("  Верхня межа діапазону: ", out var max)) continue;
                    if (min > max)
                    {
                        Console.WriteLine("  Нижня межа не може бути більшою за верхню.");
                        continue;
                    }
                    query = query.Where(r => r.Value >= min && r.Value <= max);
                    conditionTitle = $"значення від {min.ToString(CultureInfo.InvariantCulture)} до {max.ToString(CultureInfo.InvariantCulture)}";
                    break;

                case "4":
                    if (!TryReadDate("  Дата (yyyy-MM-dd або yyyy-MM-dd HH:mm, UTC): ", out var before)) continue;
                    query = query.Where(r => r.Timestamp < before);
                    conditionTitle = $"зафіксовані раніше {before:yyyy-MM-dd HH:mm}";
                    break;

                default:
                    Console.WriteLine("  Невідома умова.");
                    continue;
            }

            // === Крок 3: що саме буде видалено ===
            // Count() - момент, коли запит РЕАЛЬНО виконується (SQL COUNT)
            var matchCount = query.Count();

            if (matchCount == 0)
            {
                Console.WriteLine("\n  Покази за цією умовою не знайдено.");
                continue;
            }

            Console.WriteLine($"\n  Знайдено показів: {matchCount} ({scopeTitle}; {conditionTitle})");

            var preview = query.OrderBy(r => r.Timestamp).Take(5).ToList();
            foreach (var r in preview)
            {
                Console.WriteLine($"    Id={r.Id}, сенсор={r.DeviceId}, {r.Timestamp:yyyy-MM-dd HH:mm:ss}, значення={r.Value:F2}");
            }

            if (matchCount > preview.Count)
            {
                Console.WriteLine($"    ... та ще {matchCount - preview.Count}");
            }

            Console.Write($"\n  Видалити {matchCount} показів? (y/n): ");
            var confirm = Console.ReadLine()?.Trim().ToLowerInvariant();

            if (confirm != "y")
            {
                Console.WriteLine("  Видалення скасовано.");
                continue;
            }

            // === Крок 4: DELETE ===
            var totalBefore = context.Records.Count();

            var toDelete = query.ToList();          // завантажуємо записи, що підпадають під умову
            context.Records.RemoveRange(toDelete);  // позначаємо їх на видалення
            context.SaveChanges();                  // один виклик - одна транзакція

            // === Перевірка результату ===
            var totalAfter = context.Records.Count();
            var stillMatching = query.Count();      // той самий query виконується знову, вже по новому стану бази

            Console.WriteLine($"\n  Видалено показів: {totalBefore - totalAfter}.");
            Console.WriteLine($"  Усього показів у базі: було {totalBefore}, стало {totalAfter}.");
            Console.WriteLine($"  Показів, що ще підпадають під умову: {stillMatching}.");
        }

        // === Допоміжні методи введення ===

        static bool TryReadDouble(string prompt, out double value)
        {
            Console.Write(prompt);
            // Приймаємо і крапку, і кому як десятковий роздільник
            var text = Console.ReadLine()?.Trim().Replace(',', '.');

            if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
            {
                Console.WriteLine("  Потрібно ввести число (наприклад, 25.5).");
                return false;
            }

            return true;
        }

        static bool TryReadDate(string prompt, out DateTime value)
        {
            Console.Write(prompt);
            var text = Console.ReadLine()?.Trim();
            string[] formats = { "yyyy-MM-dd HH:mm", "yyyy-MM-dd" };

            if (!DateTime.TryParseExact(text, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out value))
            {
                Console.WriteLine("  Невірний формат дати. Приклад: 2026-10-19 14:30");
                return false;
            }

            return true;
        }
    }
}
