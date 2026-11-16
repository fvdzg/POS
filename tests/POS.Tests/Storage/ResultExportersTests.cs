using System.Text.Json;
using Pos.Application.Dtos;
using Pos.Infrastructure.Export;

namespace Pos.Tests.Storage;

public class ResultExportersTests : IDisposable
{
    private readonly TempDirectory _directory = new();

    public void Dispose() => _directory.Dispose();

    [Fact]
    public void Csv_WritesHeaderAndRowAndCreatesFolders()
    {
        var path = _directory.Combine(Path.Combine("nested", "results.csv"));

        new CsvResultExporter().Export(new[] { Sample("Иванов Иван") }, path);

        var lines = File.ReadAllLines(path);
        Assert.Equal(2, lines.Length);
        Assert.StartsWith("Студент;Группа;", lines[0]);
        Assert.StartsWith("Иванов Иван;ПМИ-231;2;2;", lines[1]);
    }

    [Fact]
    public void Csv_QuotesValuesWithDelimiters()
    {
        var path = _directory.Combine("results.csv");

        new CsvResultExporter().Export(new[] { Sample("Иванов Иван") }, path);

        var lines = File.ReadAllLines(path);
        Assert.Contains("\"Строгий; округление до 2 зн.\"", lines[1]);
    }

    [Fact]
    public void Csv_EscapesQuotesInsideValues()
    {
        var path = _directory.Combine("results.csv");

        new CsvResultExporter().Export(new[] { Sample("Иванов \"Иван\"") }, path);

        var lines = File.ReadAllLines(path);
        Assert.StartsWith("\"Иванов \"\"Иван\"\"\";", lines[1]);
    }

    [Fact]
    public void Csv_EmptyList_WritesOnlyHeader()
    {
        var path = _directory.Combine("results.csv");

        new CsvResultExporter().Export(Array.Empty<FinalResultDto>(), path);

        Assert.Single(File.ReadAllLines(path));
    }

    [Fact]
    public void Json_WritesParsableArray()
    {
        var path = _directory.Combine(Path.Combine("nested", "results.json"));

        new JsonResultExporter().Export(new[] { Sample("Иванов Иван") }, path);

        using var document = JsonDocument.Parse(File.ReadAllText(path));
        var item = document.RootElement[0];
        Assert.Equal(JsonValueKind.Array, document.RootElement.ValueKind);
        Assert.Equal("Иванов Иван", item.GetProperty("StudentName").GetString());
        Assert.Equal(86.67m, item.GetProperty("Percent").GetDecimal());
    }

    [Fact]
    public void Exporters_ReportTheirExtensions()
    {
        Assert.Equal(".csv", new CsvResultExporter().FileExtension);
        Assert.Equal(".json", new JsonResultExporter().FileExtension);
    }

    private static FinalResultDto Sample(string name)
        => new(Guid.NewGuid(), name, "ПМИ-231", 86.67m, 8.667m, 2, 2, true, "Строгий; округление до 2 зн.");
}