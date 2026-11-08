using System.Text;
using IoTTelemetryLab;
using IoTTelemetryLab.Data;

// Щоб кирилиця коректно вводилась і виводилась у консолі Windows
Console.InputEncoding = Encoding.Unicode;
Console.OutputEncoding = Encoding.UTF8;

using var context = new AppDbContext();

Console.WriteLine("Лабораторна робота 3");
FillDb.Seed(context);

while (true)
{
    Console.WriteLine("\n1 - додати сенсор");
    Console.WriteLine("2 - переглянути покази сенсора");
    Console.WriteLine("3 - перейменувати сенсор");
    Console.WriteLine("4 - видалити сенсор та його покази");
    Console.WriteLine("5 - видалити покази за умовою");
    Console.WriteLine("6 - індивідуальне завдання: відсоток показів вище 85 дБ за джерелом");
    Console.WriteLine("0 - вихід");
    Console.Write("Оберіть дію: ");
    var choice = Console.ReadLine()?.Trim();

    switch (choice)
    {
        case "1":
            FillDb.AddDevice(context);
            break;
        case "2":
            CrudRead.Run(context);
            break;
        case "3":
            CrudUpdate.Run(context);
            break;
        case "4":
            CrudDelete.Run(context);
            break;
        case "5":
            CrudConditionalDelete.Run(context);
            break;
        case "6":
            IndividualTask.Run(context);
            break;
        case "0":
        case "":
        case null:
            return;
        default:
            Console.WriteLine("Невідома дія.");
            break;
    }
}
