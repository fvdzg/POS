using Pos.Domain.Common;
using Pos.Domain.Rubrics;

namespace Pos.Domain.Evaluations;
public sealed record Evaluation
{
    internal Evaluation(CriterionId criterionId, Score score, string? comment, DateTimeOffset evaluatedAt)
    {
        CriterionId = criterionId;
        Score = score;
        Comment = comment;
        EvaluatedAt = evaluatedAt;
    }

    public CriterionId CriterionId { get; }

    public Score Score { get; }

    public string? Comment { get; }

    public DateTimeOffset EvaluatedAt { get; }
}