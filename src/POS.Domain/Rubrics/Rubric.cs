using Pos.Domain.Common;

namespace Pos.Domain.Rubrics;

public sealed class Rubric : Entity<RubricId>
{
    public const int MaxTitleLength = 200;

    private readonly List<Criterion> _criteria = new();

    public Rubric(RubricId id, string title) : base(id)
    {
        Title = Guard.RequireText(title, "Название рубрикатора", MaxTitleLength);
    }

    public string Title { get; }

    public IReadOnlyList<Criterion> Criteria => _criteria.AsReadOnly();

    public decimal TotalWeight => _criteria.Sum(c => c.Weight.Value);

    public static Rubric Create(string title) => new(RubricId.New(), title);

    public Criterion AddCriterion(string name, decimal maxScore, Weight weight)
        => AddCriterion(CriterionId.New(), name, maxScore, weight);

    public Criterion AddCriterion(CriterionId id, string name, decimal maxScore, Weight weight)
    {
        var criterion = new Criterion(id, name, maxScore, weight);

        if (_criteria.Any(c => c.Id == id))
        {
            throw new DomainException("Критерий с таким идентификатором уже есть в рубрикаторе.");
        }

        if (_criteria.Any(c => string.Equals(c.Name, criterion.Name, StringComparison.OrdinalIgnoreCase)))
        {
            throw new DuplicateCriterionException(criterion.Name);
        }

        _criteria.Add(criterion);
        return criterion;
    }
    public bool RemoveCriterion(CriterionId id)
    {
        var criterion = FindCriterion(id);
        return criterion is not null && _criteria.Remove(criterion);
    }

    public Criterion? FindCriterion(CriterionId id)
        => _criteria.FirstOrDefault(c => c.Id == id);

    public Criterion? FindCriterionByName(string? name)
    {
        var normalized = name?.Trim();
        return _criteria.FirstOrDefault(
            c => string.Equals(c.Name, normalized, StringComparison.OrdinalIgnoreCase));
    }

    public Criterion GetCriterion(CriterionId id)
        => FindCriterion(id) ?? throw new CriterionNotFoundException(id);
}