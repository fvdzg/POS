namespace Pos.Infrastructure.Persistence.Json;

internal sealed record StudentRecord(Guid Id, string FullName, string? Group);

internal sealed record CriterionRecord(Guid Id, string Name, decimal MaxScore, decimal Weight);

internal sealed record RubricRecord(Guid Id, string Title, List<CriterionRecord> Criteria);

internal sealed record EvaluationRecord(Guid CriterionId, decimal Score, string? Comment, DateTimeOffset EvaluatedAt);

internal sealed record SheetRecord(Guid StudentId, List<EvaluationRecord> Evaluations);