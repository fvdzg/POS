using System.Globalization;
using System.Text;
using Pos.Application.Abstractions;
using Pos.Application.Dtos;

namespace Pos.Infrastructure.Export;

public sealed class CsvResultExporter : IResultExporter
{
    private const string Delimiter = ";";

    public string FileExtension => ".csv";

    public void Export(IReadOnlyList<FinalResultDto> results, string path)
    {
        var directory = Path.GetDirectoryName(Path.GetFullPath(path));
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var lines = new List<string>
        {
            string.Join(
                Delimiter,
                "Студент",
                "Группа",
                "Оценено критериев",
                "Всего критериев",
                "Итог, %",
                "Итог по 10-балльной шкале",
                "Правило расчёта")
        };

        foreach (var result in results)
        {
            lines.Add(string.Join(
                Delimiter,
                Escape(result.StudentName),
                Escape(result.Group ?? string.Empty),
                result.RatedCriteria,
                result.TotalCriteria,
                result.Percent.ToString(CultureInfo.CurrentCulture),
                result.TenPointMark.ToString(CultureInfo.CurrentCulture),
                Escape(result.Method)));
        }

        File.WriteAllLines(path, lines, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
    }

    private static string Escape(string value)
    {
        if (value.IndexOfAny(new[] { ';', '"', '\n', '\r' }) < 0)
        {
            return value;
        }

        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }
}