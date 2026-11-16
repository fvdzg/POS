using Pos.Application.Dtos;
using Pos.Domain.Evaluations;
using Pos.Domain.Rubrics;

namespace Pos.Application.UseCases;

public sealed class CriterionUseCases
{
    public const string DefaultRubricTitle = "Рубрикатор курса";

    private readonly IRubricRepository _rubrics;
    private readonly IEvaluationSheetRepository _sheets;

    public CriterionUseCases(IRubricRepository rubrics, IEvaluationSheetRepository sheets)
    {
        _rubrics = rubrics;
        _sheets = sheets;
    }

    public CriterionDto Add(string name, decimal maxScore, decimal weight)
    {
        var rubric = _rubrics.Find() ?? Rubric.Create(DefaultRubricTitle);
        var criterion = rubric.AddCriterion(name, maxScore, new Weight(weight));
        _rubrics.Save(rubric);
        return CriterionDto.From(criterion);
    }

    public IReadOnlyList<CriterionDto> List()
    {
        var rubric = _rubrics.Find();
        if (rubric is null)
        {
            return Array.Empty<CriterionDto>();
        }

        return rubric.Criteria.Select(CriterionDto.From).ToList();
    }

    public void Remove(Guid criterionId)
    {
        var rubric = _rubrics.GetRequiredRubric();
        var id = new CriterionId(criterionId);

        if (rubric.FindCriterion(id) is null)
        {
            throw new UseCaseException("Критерий не найден.");
        }

        if (_sheets.GetAll().Any(sheet => sheet.HasEvaluationFor(id)))
        {
            throw new UseCaseException(
                "Нельзя удалить критерий: по нему уже выставлены оценки. Сначала отмените их.");
        }

        rubric.RemoveCriterion(id);
        _rubrics.Save(rubric);
    }
}