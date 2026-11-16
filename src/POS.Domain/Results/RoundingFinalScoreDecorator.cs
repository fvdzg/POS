using Pos.Domain.Common;
using Pos.Domain.Evaluations;
using Pos.Domain.Rubrics;

namespace Pos.Domain.Results;

public sealed class RoundingFinalScoreDecorator : IFinalScoreStrategy
{
    private readonly IFinalScoreStrategy _inner;
    private readonly int _decimals;

    public RoundingFinalScoreDecorator(IFinalScoreStrategy inner, int decimals)
    {
        ArgumentNullException.ThrowIfNull(inner);

        if (decimals < 0 || decimals > 6)
        {
            throw new DomainException("Число знаков округления должно быть от 0 до 6.");
        }

        _inner = inner;
        _decimals = decimals;
    }

    public string Name => $"{_inner.Name}; округление до {_decimals} зн.";

    public decimal CalculatePercent(Rubric rubric, EvaluationSheet sheet)
        => Math.Round(_inner.CalculatePercent(rubric, sheet), _decimals, MidpointRounding.AwayFromZero);
}