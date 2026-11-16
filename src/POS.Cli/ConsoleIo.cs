using System.Globalization;
using Pos.Application.Dtos;

namespace Pos.Cli;

public sealed class ConsoleIo
{
    public void WriteLine(string text = "") => Console.WriteLine(text);

    public void Info(string text) => WriteColored(text, ConsoleColor.Yellow);

    public void Success(string text) => WriteColored(text, ConsoleColor.Green);

    public void Error(string text) => WriteColored(text, ConsoleColor.Red);

    public void Header(string text)
    {
        Console.WriteLine();
        WriteColored($"=== {text} ===", ConsoleColor.Cyan);
    }

    public void WriteResult(FinalResultDto result)
    {
        var completeness = result.IsComplete ? string.Empty : " (оценены не все критерии)";

        WriteLine(
            $"{result.StudentName}: {result.Percent:0.##}% | {result.TenPointMark:0.##} из 10 | " +
            $"критериев {result.RatedCriteria}/{result.TotalCriteria}{completeness}");
    }

    public string ReadText(string prompt)
    {
        Console.Write(prompt);
        var line = Console.ReadLine();

        if (line is null)
        {
            throw new OperationCanceledException();
        }

        return line.Trim();
    }

    public string? ReadOptional(string prompt)
    {
        var text = ReadText(prompt);
        return text.Length == 0 ? null : text;
    }

    public decimal ReadDecimal(string prompt)
    {
        while (true)
        {
            var text = ReadText(prompt).Replace(',', '.');

            if (decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
            {
                return value;
            }

            Error("Введите число, например 7 или 7.5.");
        }
    }

    public bool Confirm(string prompt)
    {
        while (true)
        {
            var answer = ReadText($"{prompt} (д/н): ").ToLowerInvariant();

            if (answer is "д" or "да" or "y" or "yes")
            {
                return true;
            }

            if (answer is "" or "н" or "нет" or "n" or "no")
            {
                return false;
            }

            Error("Ответьте «д» или «н».");
        }
    }

    public T? Choose<T>(string title, IReadOnlyList<T> items, Func<T, string> describe)
        where T : class
    {
        WriteLine(title);

        for (var i = 0; i < items.Count; i++)
        {
            WriteLine($"  {i + 1}. {describe(items[i])}");
        }

        WriteLine("  0. Отмена");

        while (true)
        {
            var text = ReadText("Номер: ");

            if (text.Length == 0 || text == "0")
            {
                return null;
            }

            if (int.TryParse(text, out var number) && number >= 1 && number <= items.Count)
            {
                return items[number - 1];
            }

            Error("Введите номер из списка.");
        }
    }

    private static void WriteColored(string text, ConsoleColor color)
    {
        var previous = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ForegroundColor = previous;
    }
}