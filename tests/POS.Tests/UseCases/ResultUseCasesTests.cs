using System.Text.Json;
using Pos.Application;
using Pos.Application.Scoring;
using Pos.Domain.Results;

namespace Pos.Tests.UseCases;

public class ResultUseCasesTests : IDisposable
{
    private readonly TestApp _app = new();

    public void Dispose() => _app.Dispose();

    [Fact]
    public void Calculate_AllCriteriaRated_ReturnsRoundedWeightedPercent()
    {
        var (homework, exam) = _app.SeedRubric();
        var student = _app.Facade.AddStudent("Иванов Иван", "ПМИ-231");
        _app.Facade.Rate(student.Id, homework.Id, 8m, null);
        _app.Facade.Rate(student.Id, exam.Id, 5m, null);

        var result = _app.Facade.CalculateFinal(student.Id, ScoringMode.Strict);

        Assert.Equal(86.67m, result.Percent);
        Assert.Equal(8.667m, result.TenPointMark);
        Assert.Equal(2, result.RatedCriteria);
        Assert.Equal(2, result.TotalCriteria);
        Assert.True(result.IsComplete);
        Assert.Equal("ПМИ-231", result.Group);
        Assert.Contains("Строгий", result.Method);
    }

    [Fact]
    public void Calculate_UnratedCriterion_ModesGiveDifferentResults()
    {
        var (homework, _) = _app.SeedRubric();
        var student = _app.Facade.AddStudent("Иванов Иван", null);
        _app.Facade.Rate(student.Id, homework.Id, 8m, null);

        var strict = _app.Facade.CalculateFinal(student.Id, ScoringMode.Strict);
        var ratedOnly = _app.Facade.CalculateFinal(student.Id, ScoringMode.RatedOnly);

        Assert.Equal(53.33m, strict.Percent);
        Assert.Equal(80m, ratedOnly.Percent);
        Assert.Equal(1, strict.RatedCriteria);
        Assert.False(strict.IsComplete);
    }

    [Fact]
    public void Calculate_AfterCorrection_UsesCurrentEvaluations()
    {
        var (homework, _) = _app.SeedRubric();
        var student = _app.Facade.AddStudent("Иванов Иван", null);
        _app.Facade.Rate(student.Id, homework.Id, 5m, null);
        _app.Facade.Rate(student.Id, homework.Id, 10m, null);

        var result = _app.Facade.CalculateFinal(student.Id, ScoringMode.RatedOnly);

        Assert.Equal(100m, result.Percent);
    }

    [Fact]
    public void CalculateAll_ReturnsResultForEveryStudent()
    {
        var (homework, _) = _app.SeedRubric();
        var first = _app.Facade.AddStudent("Иванов Иван", null);
        _app.Facade.AddStudent("Петрова Анна", null);
        _app.Facade.Rate(first.Id, homework.Id, 10m, null);

        var results = _app.Facade.CalculateAllFinals(ScoringMode.RatedOnly);

        Assert.Equal(2, results.Count);
        Assert.Contains(results, r => r.StudentName == "Петрова Анна" && r.Percent == 0m);
    }

    [Fact]
    public void CalculateAll_NoStudents_ReturnsEmpty()
    {
        Assert.Empty(_app.Facade.CalculateAllFinals(ScoringMode.Strict));
    }

    [Fact]
    public void Calculate_WithoutRubric_Throws()
    {
        var student = _app.Facade.AddStudent("Иванов Иван", null);

        Assert.Throws<UseCaseException>(() => _app.Facade.CalculateFinal(student.Id, ScoringMode.Strict));
    }

    [Fact]
    public void Calculate_UnknownStudent_Throws()
    {
        _app.SeedRubric();

        Assert.Throws<UseCaseException>(() => _app.Facade.CalculateFinal(Guid.NewGuid(), ScoringMode.Strict));
    }

    [Fact]
    public void Export_Csv_CreatesFileWithStudents()
    {
        var (homework, _) = _app.SeedRubric();
        var student = _app.Facade.AddStudent("Иванов Иван", null);
        _app.Facade.Rate(student.Id, homework.Id, 8m, null);
        var path = _app.PathFor(Path.Combine("out", "results.csv"));

        var count = _app.Facade.ExportResults(path, ScoringMode.Strict);

        Assert.Equal(1, count);
        var lines = File.ReadAllLines(path);
        Assert.Equal(2, lines.Length);
        Assert.Contains("Иванов Иван", lines[1]);
    }

    [Fact]
    public void Export_Json_CreatesParsableFile()
    {
        var (homework, _) = _app.SeedRubric();
        var student = _app.Facade.AddStudent("Иванов Иван", null);
        _app.Facade.Rate(student.Id, homework.Id, 8m, null);
        var path = _app.PathFor("results.json");

        _app.Facade.ExportResults(path, ScoringMode.RatedOnly);

        using var document = JsonDocument.Parse(File.ReadAllText(path));
        Assert.Equal(JsonValueKind.Array, document.RootElement.ValueKind);
        Assert.Equal(1, document.RootElement.GetArrayLength());
        Assert.Equal("Иванов Иван", document.RootElement[0].GetProperty("StudentName").GetString());
    }

    [Fact]
    public void Export_UnsupportedExtension_ThrowsAndCreatesNothing()
    {
        _app.SeedRubric();
        var path = _app.PathFor("results.xlsx");

        Assert.Throws<UseCaseException>(() => _app.Facade.ExportResults(path, ScoringMode.Strict));
        Assert.False(File.Exists(path));
    }

    [Fact]
    public void Export_EmptyPath_Throws()
    {
        Assert.Throws<UseCaseException>(() => _app.Facade.ExportResults("  ", ScoringMode.Strict));
    }

    [Fact]
    public void Registry_WithoutStrategyForMode_Throws()
    {
        var strategies = new Dictionary<ScoringMode, IFinalScoreStrategy>
        {
            [ScoringMode.Strict] = new StrictWeightedScoreStrategy()
        };

        Assert.Throws<ArgumentException>(() => new ScoringStrategyRegistry(strategies));
    }

    [Fact]
    public void Registry_ReturnsStrategyConfiguredForMode()
    {
        var strict = new StrictWeightedScoreStrategy();
        var ratedOnly = new RatedOnlyWeightedScoreStrategy();
        var registry = new ScoringStrategyRegistry(new Dictionary<ScoringMode, IFinalScoreStrategy>
        {
            [ScoringMode.Strict] = strict,
            [ScoringMode.RatedOnly] = ratedOnly
        });

        Assert.Same(strict, registry.Get(ScoringMode.Strict));
        Assert.Same(ratedOnly, registry.Get(ScoringMode.RatedOnly));
    }
}