using Pos.Domain.Evaluations;
using Pos.Domain.Results;
using Pos.Domain.Rubrics;
using Pos.Domain.Students;

namespace Pos.Application.Dtos;

public sealed record StudentDto(Guid Id, string FullName, string? Group)
{
    public static StudentDto From(Student student)
        => new(student.Id.Value, student.FullName, student.Group);
}

public sealed record CriterionDto(Guid Id, string Name, decimal MaxScore, decimal Weight)
{
    public static CriterionDto From(Criterion criterion)
        => new(criterion.Id.Value, criterion.Name, criterion.MaxScore, criterion.Weight.Value);
}

public sealed record EvaluationDto(
    Guid CriterionId,
    string CriterionName,
    decimal Score,
    decimal MaxScore,
    string? Comment,
    DateTimeOffset EvaluatedAt)
{
    public static EvaluationDto From(Evaluation evaluation, Criterion criterion)
        => new(
            evaluation.CriterionId.Value,
            criterion.Name,
            evaluation.Score.Value,
            criterion.MaxScore,
            evaluation.Comment,
            evaluation.EvaluatedAt);
}

public sealed record RateResultDto(EvaluationDto Evaluation, bool WasCorrection);

public sealed record FinalResultDto(
    Guid StudentId,
    string StudentName,
    string? Group,
    decimal Percent,
    decimal TenPointMark,
    int RatedCriteria,
    int TotalCriteria,
    bool IsComplete,
    string Method)
{
    public static FinalResultDto From(FinalResult result, Student student)
        => new(
            student.Id.Value,
            student.FullName,
            student.Group,
            result.Percent,
            result.TenPointMark,
            result.RatedCriteria,
            result.TotalCriteria,
            result.IsComplete,
            result.Method);
}