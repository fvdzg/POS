using Pos.Application;
using Pos.Domain.Common;
using Pos.Domain.Rubrics;

namespace Pos.Tests.UseCases;

public class CriterionUseCasesTests : IDisposable
{
    private readonly TestApp _app = new();

    public void Dispose() => _app.Dispose();

    [Fact]
    public void List_WithoutRubric_ReturnsEmpty()
    {
        Assert.Empty(_app.Facade.GetCriteria());
    }

    [Fact]
    public void Add_FirstCriterion_CreatesRubric()
    {
        Assert.Null(_app.Rubrics.Find());

        _app.Facade.AddCriterion("Экзамен", 10m, 1m);

        var rubric = _app.Rubrics.Find();
        Assert.NotNull(rubric);
        Assert.Single(rubric.Criteria);
    }

    [Fact]
    public void Add_DuplicateName_Throws()
    {
        _app.Facade.AddCriterion("Экзамен", 10m, 1m);

        Assert.Throws<DuplicateCriterionException>(() => _app.Facade.AddCriterion("экзамен", 5m, 2m));
        Assert.Single(_app.Facade.GetCriteria());
    }

    [Fact]
    public void Add_NonPositiveWeight_Throws()
    {
        Assert.Throws<DomainException>(() => _app.Facade.AddCriterion("Экзамен", 10m, 0m));
    }

    [Fact]
    public void Add_NonPositiveMaxScore_Throws()
    {
        Assert.Throws<DomainException>(() => _app.Facade.AddCriterion("Экзамен", 0m, 1m));
    }

    [Fact]
    public void Remove_CriterionWithEvaluations_Throws()
    {
        var (homework, _) = _app.SeedRubric();
        var student = _app.Facade.AddStudent("Иванов Иван", null);
        _app.Facade.Rate(student.Id, homework.Id, 7m, null);

        Assert.Throws<UseCaseException>(() => _app.Facade.RemoveCriterion(homework.Id));
        Assert.Equal(2, _app.Facade.GetCriteria().Count);
    }

    [Fact]
    public void Remove_UnusedCriterion_RemovesIt()
    {
        var (_, exam) = _app.SeedRubric();

        _app.Facade.RemoveCriterion(exam.Id);

        var remaining = Assert.Single(_app.Facade.GetCriteria());
        Assert.Equal("Домашние работы", remaining.Name);
    }

    [Fact]
    public void Remove_UnknownCriterion_Throws()
    {
        _app.SeedRubric();

        Assert.Throws<UseCaseException>(() => _app.Facade.RemoveCriterion(Guid.NewGuid()));
    }

    [Fact]
    public void Remove_WithoutRubric_Throws()
    {
        Assert.Throws<UseCaseException>(() => _app.Facade.RemoveCriterion(Guid.NewGuid()));
    }
}