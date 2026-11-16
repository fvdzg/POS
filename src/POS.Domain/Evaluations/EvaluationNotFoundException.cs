using Pos.Domain.Common;
using Pos.Domain.Rubrics;

namespace Pos.Domain.Evaluations;

public sealed class EvaluationNotFoundException : DomainException
{
    public EvaluationNotFoundException(CriterionId criterionId)
        : base($"Оценка по критерию {criterionId} не выставлена.")
    {
        CriterionId = criterionId;
    }

    public CriterionId CriterionId { get; }
}