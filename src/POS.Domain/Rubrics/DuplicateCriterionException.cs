using Pos.Domain.Common;

namespace Pos.Domain.Rubrics;

public sealed class DuplicateCriterionException : DomainException
{
    public DuplicateCriterionException(string criterionName)
        : base($"Критерий «{criterionName}» уже есть в рубрикаторе.")
    {
        CriterionName = criterionName;
    }

    public string CriterionName { get; }
}