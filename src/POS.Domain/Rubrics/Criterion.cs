using Pos.Domain.Common;

namespace Pos.Domain.Rubrics;

public sealed class Criterion : Entity<CriterionId>
{
    public const int MaxNameLength = 100;

    internal Criterion(CriterionId id, string name, decimal maxScore, Weight weight) : base(id)
    {
        if (maxScore <= 0)
        {
            throw new DomainException("Максимальный балл критерия должен быть больше нуля.");
        }

        Name = Guard.RequireText(name, "Название критерия", MaxNameLength);
        MaxScore = maxScore;
        Weight = weight;
    }

    public string Name { get; }

    public decimal MaxScore { get; }

    public Weight Weight { get; }

    public bool Allows(decimal score) => score >= 0 && score <= MaxScore;

    public void EnsureAllows(decimal score)
    {
        if (!Allows(score))
        {
            throw new ScoreOutOfRangeException(Name, score, MaxScore);
        }
    }

    public decimal Ratio(Score score) => score.Value / MaxScore;
}