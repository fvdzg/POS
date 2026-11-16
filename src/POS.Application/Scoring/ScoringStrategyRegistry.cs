using Pos.Domain.Results;

namespace Pos.Application.Scoring;

public enum ScoringMode
{
    Strict,
    RatedOnly
}


public sealed class ScoringStrategyRegistry
{
    private readonly IReadOnlyDictionary<ScoringMode, IFinalScoreStrategy> _strategies;

    public ScoringStrategyRegistry(IReadOnlyDictionary<ScoringMode, IFinalScoreStrategy> strategies)
    {
        ArgumentNullException.ThrowIfNull(strategies);

        foreach (var mode in Enum.GetValues<ScoringMode>())
        {
            if (!strategies.ContainsKey(mode))
            {
                throw new ArgumentException($"Не задана стратегия для режима {mode}.", nameof(strategies));
            }
        }

        _strategies = strategies;
    }

    public IFinalScoreStrategy Get(ScoringMode mode) => _strategies[mode];
}