using Pos.Domain.Common;
using Pos.Domain.Rubrics;

namespace Pos.Domain.Evaluations;

public sealed class EvaluationFactory
{
    public const int MaxCommentLength = 500;

    public Evaluation Create(
        Rubric rubric,
        CriterionId criterionId,
        decimal score,
        string? comment,
        DateTimeOffset evaluatedAt)
    {
        ArgumentNullException.ThrowIfNull(rubric);

        var criterion = rubric.GetCriterion(criterionId);
        criterion.EnsureAllows(score);

        return new Evaluation(
            criterionId,
            new Score(score),
            Guard.OptionalText(comment, "Комментарий", MaxCommentLength),
            evaluatedAt);
    }
}