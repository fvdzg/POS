using Pos.Domain.Rubrics;
using Pos.Domain.Students;
using Pos.Domain.Common;

namespace Pos.Domain.Evaluations;

public sealed class EvaluationSheet : Entity<StudentId>
{
    private readonly Dictionary<CriterionId, Evaluation> _evaluations = new();

    public EvaluationSheet(StudentId studentId) : base(studentId)
    {
    }

    public StudentId StudentId => Id;

    public int Count => _evaluations.Count;

    public IReadOnlyList<Evaluation> Evaluations
        => _evaluations.Values.OrderBy(e => e.EvaluatedAt).ToList();

    public bool Assign(Evaluation evaluation)
    {
        ArgumentNullException.ThrowIfNull(evaluation);

        var isCorrection = _evaluations.ContainsKey(evaluation.CriterionId);
        _evaluations[evaluation.CriterionId] = evaluation;
        return isCorrection;
    }
    public void Cancel(CriterionId criterionId)
    {
        if (!_evaluations.Remove(criterionId))
        {
            throw new EvaluationNotFoundException(criterionId);
        }
    }

    public Evaluation? Find(CriterionId criterionId)
        => _evaluations.TryGetValue(criterionId, out var evaluation) ? evaluation : null;

    public bool HasEvaluationFor(CriterionId criterionId)
        => _evaluations.ContainsKey(criterionId);
}