using Pos.Domain.Evaluations;
using Pos.Domain.Rubrics;

namespace Pos.Domain.Results;

public interface IFinalScoreStrategy
{
    string Name { get; }

    decimal CalculatePercent(Rubric rubric, EvaluationSheet sheet);
}