using Pos.Domain.Evaluations;
using Pos.Domain.Students;

namespace Pos.Tests.Domain;

public class EvaluationSheetTests
{
    private readonly EvaluationFactory _factory = new();

    [Fact]
    public void Assign_NewEvaluation_IsNotCorrection()
    {
        var rubric = TestData.CreateRubric(out var homework, out _);
        var sheet = new EvaluationSheet(StudentId.New());

        var isCorrection = sheet.Assign(_factory.Create(rubric, homework.Id, 7m, null, TestData.Now));

        Assert.False(isCorrection);
        Assert.Equal(1, sheet.Count);
    }

    [Fact]
    public void Assign_SameCriterionTwice_ReplacesEvaluation()
    {
        var rubric = TestData.CreateRubric(out var homework, out _);
        var sheet = new EvaluationSheet(StudentId.New());
        sheet.Assign(_factory.Create(rubric, homework.Id, 7m, null, TestData.Now));

        var isCorrection = sheet.Assign(_factory.Create(rubric, homework.Id, 9m, "Пересдача", TestData.Now));

        Assert.True(isCorrection);
        Assert.Equal(1, sheet.Count);
        Assert.Equal(9m, sheet.Find(homework.Id)!.Score.Value);
    }

    [Fact]
    public void Cancel_ExistingEvaluation_RemovesIt()
    {
        var rubric = TestData.CreateRubric(out var homework, out _);
        var sheet = new EvaluationSheet(StudentId.New());
        sheet.Assign(_factory.Create(rubric, homework.Id, 7m, null, TestData.Now));

        sheet.Cancel(homework.Id);

        Assert.Equal(0, sheet.Count);
        Assert.False(sheet.HasEvaluationFor(homework.Id));
    }

    [Fact]
    public void Cancel_MissingEvaluation_Throws()
    {
        var rubric = TestData.CreateRubric(out var homework, out _);
        var sheet = new EvaluationSheet(StudentId.New());

        Assert.Throws<EvaluationNotFoundException>(() => sheet.Cancel(homework.Id));
    }

    [Fact]
    public void Evaluations_AreOrderedByTimeOfAssignment()
    {
        var rubric = TestData.CreateRubric(out var homework, out var exam);
        var sheet = new EvaluationSheet(StudentId.New());
        sheet.Assign(_factory.Create(rubric, exam.Id, 4m, null, TestData.Now.AddMinutes(5)));
        sheet.Assign(_factory.Create(rubric, homework.Id, 8m, null, TestData.Now));

        var ordered = sheet.Evaluations;

        Assert.Equal(homework.Id, ordered[0].CriterionId);
        Assert.Equal(exam.Id, ordered[1].CriterionId);
    }
}