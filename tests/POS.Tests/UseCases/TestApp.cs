using Pos.Application;
using Pos.Application.Abstractions;
using Pos.Application.Dtos;
using Pos.Application.Scoring;
using Pos.Application.UseCases;
using Pos.Domain.Evaluations;
using Pos.Domain.Results;
using Pos.Domain.Rubrics;
using Pos.Domain.Students;
using Pos.Infrastructure.Export;
using Pos.Infrastructure.Persistence.Json;

namespace Pos.Tests.UseCases;

internal sealed class TestApp : IDisposable
{
    public static readonly DateTimeOffset Now = new(2026, 11, 1, 10, 0, 0, TimeSpan.Zero);

    private readonly TempDirectory _workDirectory = new();

    public TestApp()
    {
        Students = new JsonStudentRepository(_workDirectory.Combine("students.json"));
        Rubrics = new JsonRubricRepository(_workDirectory.Combine("rubric.json"));

        var factory = new EvaluationFactory();
        Sheets = new JsonEvaluationSheetRepository(_workDirectory.Combine("evaluations.json"), Rubrics, factory);

        var strategies = new ScoringStrategyRegistry(new Dictionary<ScoringMode, IFinalScoreStrategy>
        {
            [ScoringMode.Strict] = new RoundingFinalScoreDecorator(new StrictWeightedScoreStrategy(), 2),
            [ScoringMode.RatedOnly] = new RoundingFinalScoreDecorator(new RatedOnlyWeightedScoreStrategy(), 2)
        });

        var exporters = new IResultExporter[] { new CsvResultExporter(), new JsonResultExporter() };

        Facade = new PosFacade(
            new StudentUseCases(Students, Sheets),
            new CriterionUseCases(Rubrics, Sheets),
            new EvaluationUseCases(Students, Rubrics, Sheets, factory, new FixedTimeProvider(Now)),
            new ResultUseCases(Students, Rubrics, Sheets, new FinalResultCalculator(), strategies, exporters));
    }

    public IStudentRepository Students { get; }

    public IRubricRepository Rubrics { get; }

    public IEvaluationSheetRepository Sheets { get; }

    public PosFacade Facade { get; }

    public string PathFor(string relativePath) => _workDirectory.Combine(relativePath);

    public (CriterionDto Homework, CriterionDto Exam) SeedRubric()
    {
        var homework = Facade.AddCriterion("Домашние работы", 10m, 2m);
        var exam = Facade.AddCriterion("Экзамен", 5m, 1m);
        return (homework, exam);
    }

    public void Dispose() => _workDirectory.Dispose();

    private sealed class FixedTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _now;

        public FixedTimeProvider(DateTimeOffset now)
        {
            _now = now;
        }

        public override DateTimeOffset GetUtcNow() => _now;
    }
}