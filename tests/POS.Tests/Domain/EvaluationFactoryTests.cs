using Pos.Domain.Common;
using Pos.Domain.Evaluations;
using Pos.Domain.Rubrics;

namespace Pos.Tests.Domain;

public class EvaluationFactoryTests
{
    private readonly EvaluationFactory _factory = new();

    [Theory]
    [InlineData(0.0)]
    [InlineData(5.5)]
    [InlineData(10.0)]
    public void Create_ScoreWithinRange_ReturnsEvaluation(double score)
    {
        var rubric = TestData.CreateRubric(out var homework, out _);

        var evaluation = _factory.Create(rubric, homework.Id, (decimal)score, "Комментарий", TestData.Now);

        Assert.Equal(homework.Id, evaluation.CriterionId);
        Assert.Equal((decimal)score, evaluation.Score.Value);
        Assert.Equal("Комментарий", evaluation.Comment);
        Assert.Equal(TestData.Now, evaluation.EvaluatedAt);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(10.01)]
    [InlineData(100.0)]
    public void Create_ScoreOutOfRange_Throws(double score)
    {
        var rubric = TestData.CreateRubric(out var homework, out _);

        Assert.Throws<ScoreOutOfRangeException>(
            () => _factory.Create(rubric, homework.Id, (decimal)score, null, TestData.Now));
    }

    [Fact]
    public void Create_CriterionFromAnotherRubric_Throws()
    {
        var rubric = TestData.CreateRubric(out _, out _);
        TestData.CreateRubric(out var foreignCriterion, out _);

        Assert.Throws<CriterionNotFoundException>(
            () => _factory.Create(rubric, foreignCriterion.Id, 5m, null, TestData.Now));
    }

    [Fact]
    public void Create_UnknownCriterionId_Throws()
    {
        var rubric = TestData.CreateRubric(out _, out _);

        Assert.Throws<CriterionNotFoundException>(
            () => _factory.Create(rubric, CriterionId.New(), 5m, null, TestData.Now));
    }

    [Fact]
    public void Create_BlankComment_BecomesNull()
    {
        var rubric = TestData.CreateRubric(out var homework, out _);

        var evaluation = _factory.Create(rubric, homework.Id, 5m, "   ", TestData.Now);

        Assert.Null(evaluation.Comment);
    }

    [Fact]
    public void Create_TooLongComment_Throws()
    {
        var rubric = TestData.CreateRubric(out var homework, out _);
        var comment = new string('x', EvaluationFactory.MaxCommentLength + 1);

        Assert.Throws<DomainException>(
            () => _factory.Create(rubric, homework.Id, 5m, comment, TestData.Now));
    }
}