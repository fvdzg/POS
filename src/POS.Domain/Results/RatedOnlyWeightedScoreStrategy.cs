using Pos.Domain.Evaluations;
using Pos.Domain.Rubrics;

namespace Pos.Domain.Results;

public sealed class RatedOnlyWeightedScoreStrategy : IFinalScoreStrategy
{
    public string Name => "Только по оценённым критериям";

    public decimal CalculatePercent(Rubric rubric, EvaluationSheet sheet)
    {
        ArgumentNullException.ThrowIfNull(rubric);
        ArgumentNullException.ThrowIfNull(sheet);

        var ratedWeight = 0m;
        var weighted = 0m;

        foreach (var criterion in rubric.Criteria)
        {
            var evaluation = sheet.Find(criterion.Id);
            if (evaluation is null)
            {
                continue;
            }

            ratedWeight += criterion.Weight.Value;
            weighted += criterion.Weight.Value * criterion.Ratio(evaluation.Score);
        }

        if (ratedWeight == 0m)
        {
            return 0m;
        }

        return Math.Clamp(weighted / ratedWeight * 100m, 0m, 100m);
    }
}