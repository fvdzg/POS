using Pos.Domain.Common;

namespace Pos.Domain.Rubrics;
public sealed class CriterionNotFoundException : DomainException
{
    public CriterionNotFoundException(CriterionId criterionId)
        : base($"Критерий {criterionId} не входит в рубрикатор.")
    {
        CriterionId = criterionId;
    }

    public CriterionId CriterionId { get; }
}