using Pos.Domain.Evaluations;
using Pos.Domain.Rubrics;
using Pos.Domain.Students;
using Pos.Infrastructure.Persistence.Json;

namespace Pos.Tests.Storage;

public class JsonRepositoriesTests : IDisposable
{
    private static readonly DateTimeOffset Now = new(2026, 11, 1, 10, 0, 0, TimeSpan.Zero);

    private readonly TempDirectory _directory = new();

    public void Dispose() => _directory.Dispose();

    [Fact]
    public void StudentRepository_NoFile_ReturnsEmpty()
    {
        var repository = new JsonStudentRepository(_directory.Combine("students.json"));

        Assert.Empty(repository.GetAll());
        Assert.Null(repository.Find(StudentId.New()));
    }

    [Fact]
    public void StudentRepository_KeepsDataBetweenInstances()
    {
        var path = _directory.Combine("students.json");
        var student = Student.Create("Иванов Иван", "ПМИ-231");
        new JsonStudentRepository(path).Add(student);

        var loaded = new JsonStudentRepository(path).Find(student.Id);

        Assert.NotNull(loaded);
        Assert.Equal("Иванов Иван", loaded.FullName);
        Assert.Equal("ПМИ-231", loaded.Group);
    }

    [Fact]
    public void StudentRepository_Remove_ReturnsExpectedFlags()
    {
        var repository = new JsonStudentRepository(_directory.Combine("students.json"));
        var student = Student.Create("Иванов Иван");
        repository.Add(student);

        Assert.True(repository.Remove(student.Id));
        Assert.False(repository.Remove(student.Id));
        Assert.Empty(repository.GetAll());
    }

    [Fact]
    public void RubricRepository_NoFile_ReturnsNull()
    {
        var repository = new JsonRubricRepository(_directory.Combine("rubric.json"));

        Assert.Null(repository.Find());
    }

    [Fact]
    public void RubricRepository_RoundTripsCriteria()
    {
        var path = _directory.Combine("rubric.json");
        var rubric = Rubric.Create("Курс");
        var criterion = rubric.AddCriterion("Экзамен", 10m, new Weight(2.5m));
        new JsonRubricRepository(path).Save(rubric);

        var loaded = new JsonRubricRepository(path).Find();

        Assert.NotNull(loaded);
        Assert.Equal(rubric.Id, loaded.Id);
        var restored = Assert.Single(loaded.Criteria);
        Assert.Equal(criterion.Id, restored.Id);
        Assert.Equal("Экзамен", restored.Name);
        Assert.Equal(10m, restored.MaxScore);
        Assert.Equal(2.5m, restored.Weight.Value);
    }

    [Fact]
    public void SheetRepository_NoFile_FindsNothingEvenWithoutRubric()
    {
        var rubrics = new JsonRubricRepository(_directory.Combine("rubric.json"));
        var repository = new JsonEvaluationSheetRepository(
            _directory.Combine("evaluations.json"), rubrics, new EvaluationFactory());

        Assert.Null(repository.Find(StudentId.New()));
        Assert.Empty(repository.GetAll());
    }

    [Fact]
    public void SheetRepository_RoundTripsEvaluations()
    {
        var (rubrics, rubric, criterion) = CreateRubricRepository();
        var factory = new EvaluationFactory();
        var path = _directory.Combine("evaluations.json");
        var studentId = StudentId.New();
        var sheet = new EvaluationSheet(studentId);
        sheet.Assign(factory.Create(rubric, criterion.Id, 7.5m, "Хорошо", Now));
        new JsonEvaluationSheetRepository(path, rubrics, factory).Save(sheet);

        var loaded = new JsonEvaluationSheetRepository(path, rubrics, factory).Find(studentId);

        Assert.NotNull(loaded);
        var evaluation = loaded.Find(criterion.Id);
        Assert.NotNull(evaluation);
        Assert.Equal(7.5m, evaluation.Score.Value);
        Assert.Equal("Хорошо", evaluation.Comment);
        Assert.Equal(Now, evaluation.EvaluatedAt);
    }

    [Fact]
    public void SheetRepository_SaveTwice_ReplacesSheetOfSameStudent()
    {
        var (rubrics, rubric, criterion) = CreateRubricRepository();
        var factory = new EvaluationFactory();
        var repository = new JsonEvaluationSheetRepository(
            _directory.Combine("evaluations.json"), rubrics, factory);
        var studentId = StudentId.New();

        var first = new EvaluationSheet(studentId);
        first.Assign(factory.Create(rubric, criterion.Id, 3m, null, Now));
        repository.Save(first);

        var second = new EvaluationSheet(studentId);
        second.Assign(factory.Create(rubric, criterion.Id, 9m, null, Now));
        repository.Save(second);

        var all = repository.GetAll();
        var sheet = Assert.Single(all);
        Assert.Equal(9m, sheet.Find(criterion.Id)!.Score.Value);
    }

    [Fact]
    public void SheetRepository_Remove_ReturnsExpectedFlags()
    {
        var (rubrics, rubric, criterion) = CreateRubricRepository();
        var factory = new EvaluationFactory();
        var repository = new JsonEvaluationSheetRepository(
            _directory.Combine("evaluations.json"), rubrics, factory);
        var studentId = StudentId.New();
        var sheet = new EvaluationSheet(studentId);
        sheet.Assign(factory.Create(rubric, criterion.Id, 3m, null, Now));
        repository.Save(sheet);

        Assert.True(repository.Remove(studentId));
        Assert.False(repository.Remove(studentId));
        Assert.Empty(repository.GetAll());
    }

    [Fact]
    public void SheetRepository_ScoreAboveMaxInFile_IsRejectedOnLoad()
    {
        var (rubrics, _, criterion) = CreateRubricRepository();
        var studentId = StudentId.New();
        var path = _directory.Combine("evaluations.json");
        var json = "[{\"StudentId\":\"" + studentId.Value + "\",\"Evaluations\":[{\"CriterionId\":\""
                   + criterion.Id.Value
                   + "\",\"Score\":99,\"Comment\":null,\"EvaluatedAt\":\"2026-11-01T10:00:00+00:00\"}]}]";
        File.WriteAllText(path, json);
        var repository = new JsonEvaluationSheetRepository(path, rubrics, new EvaluationFactory());

        Assert.Throws<ScoreOutOfRangeException>(() => repository.Find(studentId));
    }

    [Fact]
    public void SheetRepository_EvaluationForCriterionMissingInRubric_IsRejectedOnLoad()
    {
        var (rubrics, _, _) = CreateRubricRepository();
        var studentId = StudentId.New();
        var path = _directory.Combine("evaluations.json");
        var json = "[{\"StudentId\":\"" + studentId.Value + "\",\"Evaluations\":[{\"CriterionId\":\""
                   + Guid.NewGuid()
                   + "\",\"Score\":5,\"Comment\":null,\"EvaluatedAt\":\"2026-11-01T10:00:00+00:00\"}]}]";
        File.WriteAllText(path, json);
        var repository = new JsonEvaluationSheetRepository(path, rubrics, new EvaluationFactory());

        Assert.Throws<CriterionNotFoundException>(() => repository.Find(studentId));
    }

    [Fact]
    public void SheetRepository_EvaluationsWithoutRubric_ThrowsOnLoad()
    {
        var rubrics = new JsonRubricRepository(_directory.Combine("rubric.json"));
        var studentId = StudentId.New();
        var path = _directory.Combine("evaluations.json");
        var json = "[{\"StudentId\":\"" + studentId.Value + "\",\"Evaluations\":[]}]";
        File.WriteAllText(path, json);
        var repository = new JsonEvaluationSheetRepository(path, rubrics, new EvaluationFactory());

        Assert.Throws<InvalidOperationException>(() => repository.Find(studentId));
    }

    private (JsonRubricRepository Repository, Rubric Rubric, Criterion Criterion) CreateRubricRepository()
    {
        var repository = new JsonRubricRepository(_directory.Combine("rubric.json"));
        var rubric = Rubric.Create("Курс");
        var criterion = rubric.AddCriterion("Экзамен", 10m, new Weight(1m));
        repository.Save(rubric);
        return (repository, rubric, criterion);
    }
}