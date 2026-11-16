using Microsoft.Extensions.DependencyInjection;
using Pos.Application;
using Pos.Application.Abstractions;
using Pos.Application.Scoring;
using Pos.Application.UseCases;
using Pos.Domain.Evaluations;
using Pos.Domain.Results;
using Pos.Domain.Rubrics;
using Pos.Domain.Students;
using Pos.Infrastructure.Export;
using Pos.Infrastructure.Persistence.Json;

namespace Pos.Cli;

public static class CompositionRoot
{
    public static ServiceProvider Build(string dataDirectory)
    {
        var services = new ServiceCollection();

        services.AddSingleton(TimeProvider.System);

        services.AddSingleton<IStudentRepository>(
            _ => new JsonStudentRepository(Path.Combine(dataDirectory, "students.json")));
        services.AddSingleton<IRubricRepository>(
            _ => new JsonRubricRepository(Path.Combine(dataDirectory, "rubric.json")));
        services.AddSingleton<IEvaluationSheetRepository>(
            provider => new JsonEvaluationSheetRepository(
                Path.Combine(dataDirectory, "evaluations.json"),
                provider.GetRequiredService<IRubricRepository>(),
                provider.GetRequiredService<EvaluationFactory>()));

        services.AddSingleton<EvaluationFactory>();
        services.AddSingleton<FinalResultCalculator>();
        services.AddSingleton(_ => new ScoringStrategyRegistry(
            new Dictionary<ScoringMode, IFinalScoreStrategy>
            {
                [ScoringMode.Strict] =
                    new RoundingFinalScoreDecorator(new StrictWeightedScoreStrategy(), 2),
                [ScoringMode.RatedOnly] =
                    new RoundingFinalScoreDecorator(new RatedOnlyWeightedScoreStrategy(), 2)
            }));

        services.AddSingleton<IResultExporter, CsvResultExporter>();
        services.AddSingleton<IResultExporter, JsonResultExporter>();

        services.AddSingleton<StudentUseCases>();
        services.AddSingleton<CriterionUseCases>();
        services.AddSingleton<EvaluationUseCases>();
        services.AddSingleton<ResultUseCases>();
        services.AddSingleton<PosFacade>();

        services.AddSingleton<ConsoleIo>();
        services.AddSingleton<ConsoleApp>();
        services.AddSingleton<DemoScenario>();

        return services.BuildServiceProvider();
    }
}