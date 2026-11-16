using Pos.Domain.Students;

namespace Pos.Domain.Evaluations;

public interface IEvaluationSheetRepository
{
    EvaluationSheet? Find(StudentId studentId);

    IReadOnlyList<EvaluationSheet> GetAll();

    void Save(EvaluationSheet sheet);

    bool Remove(StudentId studentId);
}