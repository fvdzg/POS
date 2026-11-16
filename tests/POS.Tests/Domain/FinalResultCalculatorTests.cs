using Pos.Domain.Common;
using Pos.Domain.Evaluations;
using Pos.Domain.Results;
using Pos.Domain.Rubrics;
using Pos.Domain.Students;

namespace Pos.Tests.Domain;

public class FinalResultCalculatorTests
{
    private readonly EvaluationFactory _factory = new();
    private readonly FinalResultCalculator _calculator = new();

    private EvaluationSheet SheetWith(Rubric rubric, params (Criterion Criterion, decimal Score)[] rates)
    {
        var sheet = new EvaluationSheet(StudentId.New());
        foreach (var (criterion, score) in rates)
        {
            sheet.Assign(_factory.Create(rubric, criterion.Id, score, null, TestData.Now));
        }

        return sheet;
    }

    [Fact]
    public void Strict_AllCriteriaRated_ReturnsWeightedPercent()
    {
        var rubric = TestData.CreateRubric(out var homework, out var exam);
        var sheet = SheetWith(rubric, (homework, 8m), (exam, 5m));

        var result = _calculator.Calculate(
            rubric, sheet, new RoundingFinalScoreDecorator(new StrictWeightedScoreStrategy(), 2));

        // (2 * 0.8 + 1 * 1.0) / 3 = 0.8666... -> 86.67 %
        Assert.Equal(86.67m, result.Percent);
        Assert.Equal(8.667m, result.TenPointMark);
        Assert.Equal(2, result.RatedCriteria);
        Assert.True(result.IsComplete);
    }

    [Fact]
    public void Strict_UnratedCriterionCountsAsZero()
    {
        var rubric = TestData.CreateRubric(out var homework, out _);
        var sheet = SheetWith(rubric, (homework, 8m));

        var result = _calculator.Calculate(
            rubric, sheet, new RoundingFinalScoreDecorator(new StrictWeightedScoreStrategy(), 2));

        // (2 * 0.8) / 3 = 0.5333... -> 53.33 %
        Assert.Equal(53.33m, result.Percent);
        Assert.Equal(1, result.RatedCriteria);
        Assert.Equal(2, result.TotalCriteria);
        Assert.False(result.IsComplete);
    }

    [Fact]
    public void RatedOnly_IgnoresUnratedCriteria()
    {
        var rubric = TestData.CreateRubric(out var homework, out _);
        var sheet = SheetWith(rubric, (homework, 8m));

        var result = _calculator.Calculate(rubric, sheet, new RatedOnlyWeightedScoreStrategy());

        Assert.Equal(80m, result.Percent);
    }

    [Fact]
    public void EmptySheet_GivesZeroPercent()
    {
        var rubric = TestData.CreateRubric(out _, out _);
        var sheet = SheetWith(rubric);

        Assert.Equal(0m, _calculator.Calculate(rubric, sheet, new StrictWeightedScoreStrategy()).Percent);
        Assert.Equal(0m, _calculator.Calculate(rubric, sheet, new RatedOnlyWeightedScoreStrategy()).Percent);
    }

    [Fact]
    public void FullMarks_GiveHundredPercent()
    {
        var rubric = TestData.CreateRubric(out var homework, out var exam);
        var sheet = SheetWith(rubric, (homework, 10m), (exam, 5m));

        var result = _calculator.Calculate(rubric, sheet, new StrictWeightedScoreStrategy());

        Assert.Equal(100m, result.Percent);
        Assert.Equal(10m, result.TenPointMark);
    }

    [Fact]
    public void Calculate_EvaluationForCriterionMissingInRubric_Throws()
    {
        var rubricA = TestData.CreateRubric(out var homeworkA, out _);
        var rubricB = TestData.CreateRubric(out _, out _);
        var sheet = SheetWith(rubricA, (homeworkA, 5m));

        Assert.Throws<CriterionNotFoundException>(
            () => _calculator.Calculate(rubricB, sheet, new StrictWeightedScoreStrategy()));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(7)]
    public void RoundingDecorator_InvalidDecimals_Throws(int decimals)
    {
        Assert.Throws<DomainException>(
            () => new RoundingFinalScoreDecorator(new StrictWeightedScoreStrategy(), decimals));
    }
}