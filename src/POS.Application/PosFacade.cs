using Pos.Application.Dtos;
using Pos.Application.Scoring;
using Pos.Application.UseCases;

namespace Pos.Application;

public sealed class PosFacade
{
    private readonly StudentUseCases _students;
    private readonly CriterionUseCases _criteria;
    private readonly EvaluationUseCases _evaluations;
    private readonly ResultUseCases _results;

    public PosFacade(
        StudentUseCases students,
        CriterionUseCases criteria,
        EvaluationUseCases evaluations,
        ResultUseCases results)
    {
        _students = students;
        _criteria = criteria;
        _evaluations = evaluations;
        _results = results;
    }


    public StudentDto AddStudent(string fullName, string? group) => _students.Add(fullName, group);

    public IReadOnlyList<StudentDto> GetStudents() => _students.List();

    public void RemoveStudent(Guid studentId) => _students.Remove(studentId);


    public CriterionDto AddCriterion(string name, decimal maxScore, decimal weight)
        => _criteria.Add(name, maxScore, weight);

    public IReadOnlyList<CriterionDto> GetCriteria() => _criteria.List();

    public void RemoveCriterion(Guid criterionId) => _criteria.Remove(criterionId);


    public RateResultDto Rate(Guid studentId, Guid criterionId, decimal score, string? comment)
        => _evaluations.Rate(studentId, criterionId, score, comment);

    public void CancelEvaluation(Guid studentId, Guid criterionId)
        => _evaluations.Cancel(studentId, criterionId);

    public IReadOnlyList<EvaluationDto> GetEvaluations(Guid studentId) => _evaluations.List(studentId);


    public FinalResultDto CalculateFinal(Guid studentId, ScoringMode mode)
        => _results.Calculate(studentId, mode);

    public IReadOnlyList<FinalResultDto> CalculateAllFinals(ScoringMode mode)
        => _results.CalculateAll(mode);

    public int ExportResults(string path, ScoringMode mode) => _results.Export(path, mode);
}