using Pos.Domain.Evaluations;
using Pos.Domain.Rubrics;

namespace Pos.Domain.Results;

public sealed class FinalResultCalculator
{
    public FinalResult Calculate(Rubric rubric, EvaluationSheet sheet, IFinalScoreStrategy strategy)
    {
        ArgumentNullException.ThrowIfNull(rubric);
        ArgumentNullException.ThrowIfNull(sheet);
        ArgumentNullException.ThrowIfNull(strategy);

        foreach (var evaluation in sheet.Evaluations)
        {
            if (rubric.FindCriterion(evaluation.CriterionId) is null)
            {
                throw new CriterionNotFoundException(evaluation.CriterionId);
            }
        }

        var percent = strategy.CalculatePercent(rubric, sheet);
        var rated = rubric.Criteria.Count(c => sheet.HasEvaluationFor(c.Id));

        return new FinalResult(sheet.StudentId, percent, rated, rubric.Criteria.Count, strategy.Name);
    }
}