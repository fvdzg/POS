using Pos.Application.Dtos;
using Pos.Domain.Evaluations;
using Pos.Domain.Rubrics;
using Pos.Domain.Students;

namespace Pos.Application.UseCases;

public sealed class EvaluationUseCases
{
    private readonly IStudentRepository _students;
    private readonly IRubricRepository _rubrics;
    private readonly IEvaluationSheetRepository _sheets;
    private readonly EvaluationFactory _factory;
    private readonly TimeProvider _clock;

    public EvaluationUseCases(
        IStudentRepository students,
        IRubricRepository rubrics,
        IEvaluationSheetRepository sheets,
        EvaluationFactory factory,
        TimeProvider clock)
    {
        _students = students;
        _rubrics = rubrics;
        _sheets = sheets;
        _factory = factory;
        _clock = clock;
    }

    public RateResultDto Rate(Guid studentId, Guid criterionId, decimal score, string? comment)
    {
        var student = _students.GetRequiredStudent(studentId);
        var rubric = _rubrics.GetRequiredRubric();
        var id = new CriterionId(criterionId);

        var evaluation = _factory.Create(rubric, id, score, comment, _clock.GetUtcNow());

        var sheet = _sheets.Find(student.Id) ?? new EvaluationSheet(student.Id);
        var wasCorrection = sheet.Assign(evaluation);
        _sheets.Save(sheet);

        var dto = EvaluationDto.From(evaluation, rubric.GetCriterion(id));
        return new RateResultDto(dto, wasCorrection);
    }

    public void Cancel(Guid studentId, Guid criterionId)
    {
        var student = _students.GetRequiredStudent(studentId);
        var sheet = _sheets.Find(student.Id)
                    ?? throw new UseCaseException("У студента нет выставленных оценок.");

        sheet.Cancel(new CriterionId(criterionId));
        _sheets.Save(sheet);
    }

    public IReadOnlyList<EvaluationDto> List(Guid studentId)
    {
        var student = _students.GetRequiredStudent(studentId);
        var rubric = _rubrics.Find();
        var sheet = _sheets.Find(student.Id);

        if (rubric is null || sheet is null)
        {
            return Array.Empty<EvaluationDto>();
        }

        return sheet.Evaluations
            .Select(evaluation => EvaluationDto.From(evaluation, rubric.GetCriterion(evaluation.CriterionId)))
            .ToList();
    }
}