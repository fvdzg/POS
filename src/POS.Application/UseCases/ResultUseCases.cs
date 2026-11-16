using Pos.Application.Abstractions;
using Pos.Application.Dtos;
using Pos.Application.Scoring;
using Pos.Domain.Evaluations;
using Pos.Domain.Results;
using Pos.Domain.Rubrics;
using Pos.Domain.Students;

namespace Pos.Application.UseCases;

public sealed class ResultUseCases
{
    private readonly IStudentRepository _students;
    private readonly IRubricRepository _rubrics;
    private readonly IEvaluationSheetRepository _sheets;
    private readonly FinalResultCalculator _calculator;
    private readonly ScoringStrategyRegistry _strategies;
    private readonly IReadOnlyList<IResultExporter> _exporters;

    public ResultUseCases(
        IStudentRepository students,
        IRubricRepository rubrics,
        IEvaluationSheetRepository sheets,
        FinalResultCalculator calculator,
        ScoringStrategyRegistry strategies,
        IEnumerable<IResultExporter> exporters)
    {
        _students = students;
        _rubrics = rubrics;
        _sheets = sheets;
        _calculator = calculator;
        _strategies = strategies;
        _exporters = exporters.ToList();
    }

    public FinalResultDto Calculate(Guid studentId, ScoringMode mode)
    {
        var student = _students.GetRequiredStudent(studentId);
        var rubric = _rubrics.GetRequiredRubric();
        return CalculateFor(student, rubric, mode);
    }

    public IReadOnlyList<FinalResultDto> CalculateAll(ScoringMode mode)
    {
        var students = _students.GetAll();
        if (students.Count == 0)
        {
            return Array.Empty<FinalResultDto>();
        }

        var rubric = _rubrics.GetRequiredRubric();
        return students.Select(student => CalculateFor(student, rubric, mode)).ToList();
    }


    public int Export(string path, ScoringMode mode)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new UseCaseException("Укажите путь к файлу выгрузки.");
        }

        var extension = Path.GetExtension(path);
        var exporter = _exporters.FirstOrDefault(
            e => string.Equals(e.FileExtension, extension, StringComparison.OrdinalIgnoreCase));

        if (exporter is null)
        {
            var supported = string.Join(", ", _exporters.Select(e => e.FileExtension));
            throw new UseCaseException(
                $"Формат файла «{extension}» не поддерживается. Доступные форматы: {supported}.");
        }

        var results = CalculateAll(mode);
        exporter.Export(results, path);
        return results.Count;
    }

    private FinalResultDto CalculateFor(Student student, Rubric rubric, ScoringMode mode)
    {
        var sheet = _sheets.Find(student.Id) ?? new EvaluationSheet(student.Id);
        var result = _calculator.Calculate(rubric, sheet, _strategies.Get(mode));
        return FinalResultDto.From(result, student);
    }
}