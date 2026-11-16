using Pos.Domain.Common;

namespace Pos.Domain.Rubrics;

public sealed class ScoreOutOfRangeException : DomainException
{
    public ScoreOutOfRangeException(string criterionName, decimal score, decimal maxScore)
        : base($"Балл {score} вне допустимого диапазона [0; {maxScore}] для критерия «{criterionName}».")
    {
        CriterionName = criterionName;
        Score = score;
        MaxScore = maxScore;
    }

    public string CriterionName { get; }

    public decimal Score { get; }

    public decimal MaxScore { get; }
}