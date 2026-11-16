using Pos.Application;
using Pos.Domain.Evaluations;
using Pos.Domain.Rubrics;

namespace Pos.Tests.UseCases;

public class EvaluationUseCasesTests : IDisposable
{
    private readonly TestApp _app = new();

    public void Dispose() => _app.Dispose();

    [Fact]
    public void Rate_ValidScore_SavesEvaluation()
    {
        var (homework, _) = _app.SeedRubric();
        var student = _app.Facade.AddStudent("Иванов Иван", null);

        var result = _app.Facade.Rate(student.Id, homework.Id, 7.5m, "Хорошо");

        Assert.False(result.WasCorrection);
        Assert.Equal("Домашние работы", result.Evaluation.CriterionName);
        Assert.Equal(7.5m, result.Evaluation.Score);
        Assert.Equal(10m, result.Evaluation.MaxScore);
        Assert.Equal(TestApp.Now, result.Evaluation.EvaluatedAt);

        var saved = Assert.Single(_app.Facade.GetEvaluations(student.Id));
        Assert.Equal("Хорошо", saved.Comment);
    }

    [Fact]
    public void Rate_SameCriterionTwice_IsCorrection()
    {
        var (homework, _) = _app.SeedRubric();
        var student = _app.Facade.AddStudent("Иванов Иван", null);
        _app.Facade.Rate(student.Id, homework.Id, 6m, null);

        var result = _app.Facade.Rate(student.Id, homework.Id, 9m, "Пересдача");

        Assert.True(result.WasCorrection);
        var saved = Assert.Single(_app.Facade.GetEvaluations(student.Id));
        Assert.Equal(9m, saved.Score);
    }

    [Fact]
    public void Rate_ScoreAboveMax_ThrowsAndSavesNothing()
    {
        var (homework, _) = _app.SeedRubric();
        var student = _app.Facade.AddStudent("Иванов Иван", null);

        Assert.Throws<ScoreOutOfRangeException>(
            () => _app.Facade.Rate(student.Id, homework.Id, 10.5m, null));

        Assert.Empty(_app.Facade.GetEvaluations(student.Id));
    }

    [Fact]
    public void Rate_NegativeScore_Throws()
    {
        var (homework, _) = _app.SeedRubric();
        var student = _app.Facade.AddStudent("Иванов Иван", null);

        Assert.Throws<ScoreOutOfRangeException>(
            () => _app.Facade.Rate(student.Id, homework.Id, -1m, null));
    }

    [Fact]
    public void Rate_CriterionNotInRubric_Throws()
    {
        _app.SeedRubric();
        var student = _app.Facade.AddStudent("Иванов Иван", null);

        Assert.Throws<CriterionNotFoundException>(
            () => _app.Facade.Rate(student.Id, Guid.NewGuid(), 5m, null));
    }

    [Fact]
    public void Rate_UnknownStudent_Throws()
    {
        var (homework, _) = _app.SeedRubric();

        Assert.Throws<UseCaseException>(
            () => _app.Facade.Rate(Guid.NewGuid(), homework.Id, 5m, null));
    }

    [Fact]
    public void Rate_WithoutRubric_Throws()
    {
        var student = _app.Facade.AddStudent("Иванов Иван", null);

        Assert.Throws<UseCaseException>(
            () => _app.Facade.Rate(student.Id, Guid.NewGuid(), 5m, null));
    }

    [Fact]
    public void Cancel_ExistingEvaluation_RemovesIt()
    {
        var (homework, _) = _app.SeedRubric();
        var student = _app.Facade.AddStudent("Иванов Иван", null);
        _app.Facade.Rate(student.Id, homework.Id, 7m, null);

        _app.Facade.CancelEvaluation(student.Id, homework.Id);

        Assert.Empty(_app.Facade.GetEvaluations(student.Id));
    }

    [Fact]
    public void Cancel_StudentWithoutEvaluations_Throws()
    {
        var (homework, _) = _app.SeedRubric();
        var student = _app.Facade.AddStudent("Иванов Иван", null);

        Assert.Throws<UseCaseException>(
            () => _app.Facade.CancelEvaluation(student.Id, homework.Id));
    }

    [Fact]
    public void Cancel_EvaluationThatWasNeverSet_Throws()
    {
        var (homework, exam) = _app.SeedRubric();
        var student = _app.Facade.AddStudent("Иванов Иван", null);
        _app.Facade.Rate(student.Id, homework.Id, 7m, null);

        Assert.Throws<EvaluationNotFoundException>(
            () => _app.Facade.CancelEvaluation(student.Id, exam.Id));
    }

    [Fact]
    public void GetEvaluations_UnknownStudent_Throws()
    {
        Assert.Throws<UseCaseException>(() => _app.Facade.GetEvaluations(Guid.NewGuid()));
    }
}