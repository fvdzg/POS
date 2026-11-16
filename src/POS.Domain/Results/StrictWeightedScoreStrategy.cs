using Pos.Domain.Evaluations;
using Pos.Domain.Rubrics;

namespace Pos.Domain.Results;

public sealed class StrictWeightedScoreStrategy : IFinalScoreStrategy
{
    public string Name => "Строгий: неоценённые критерии = 0";

    public decimal CalculatePercent(Rubric rubric, EvaluationSheet sheet)
    {
        ArgumentNullException.ThrowIfNull(rubric);
        ArgumentNullException.ThrowIfNull(sheet);

        var totalWeight = rubric.TotalWeight;
        if (totalWeight == 0m)
        {
            return 0m;
        }

        var weighted = 0m;
        foreach (var criterion in rubric.Criteria)
        {
            var evaluation = sheet.Find(criterion.Id);
            if (evaluation is not null)
            {
                weighted += criterion.Weight.Value * criterion.Ratio(evaluation.Score);
            }
        }

        return Math.Clamp(weighted / totalWeight * 100m, 0m, 100m);
    }
}