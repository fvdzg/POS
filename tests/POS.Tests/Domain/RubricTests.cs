using Pos.Domain.Common;
using Pos.Domain.Rubrics;

namespace Pos.Tests.Domain;

public class RubricTests
{
    [Fact]
    public void AddCriterion_ValidData_AddsCriterion()
    {
        var rubric = Rubric.Create("Курс");

        var criterion = rubric.AddCriterion("  Экзамен ", 10m, new Weight(1.5m));

        Assert.Single(rubric.Criteria);
        Assert.Equal("Экзамен", criterion.Name);
        Assert.Equal(10m, criterion.MaxScore);
        Assert.Equal(1.5m, criterion.Weight.Value);
    }

    [Fact]
    public void AddCriterion_DuplicateName_IgnoringCase_Throws()
    {
        var rubric = Rubric.Create("Курс");
        rubric.AddCriterion("Экзамен", 10m, new Weight(1m));

        Assert.Throws<DuplicateCriterionException>(
            () => rubric.AddCriterion("  экзамен ", 5m, new Weight(2m)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void AddCriterion_NonPositiveMaxScore_Throws(int maxScore)
    {
        var rubric = Rubric.Create("Курс");

        Assert.Throws<DomainException>(
            () => rubric.AddCriterion("Экзамен", maxScore, new Weight(1m)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Weight_NonPositive_Throws(int value)
    {
        Assert.Throws<DomainException>(() => new Weight(value));
    }

    [Fact]
    public void GetCriterion_Unknown_Throws()
    {
        var rubric = Rubric.Create("Курс");

        Assert.Throws<CriterionNotFoundException>(() => rubric.GetCriterion(CriterionId.New()));
    }

    [Fact]
    public void RemoveCriterion_ExistingAndMissing_ReturnsExpectedFlags()
    {
        var rubric = Rubric.Create("Курс");
        var criterion = rubric.AddCriterion("Экзамен", 10m, new Weight(1m));

        Assert.True(rubric.RemoveCriterion(criterion.Id));
        Assert.False(rubric.RemoveCriterion(criterion.Id));
        Assert.Empty(rubric.Criteria);
    }

    [Fact]
    public void FindCriterionByName_IgnoresCaseAndSpaces()
    {
        var rubric = Rubric.Create("Курс");
        var criterion = rubric.AddCriterion("Экзамен", 10m, new Weight(1m));

        Assert.Equal(criterion, rubric.FindCriterionByName("  ЭКЗАМЕН "));
        Assert.Null(rubric.FindCriterionByName("Нет такого"));
    }

    [Fact]
    public void TotalWeight_SumsWeightsOfAllCriteria()
    {
        var rubric = TestData.CreateRubric(out _, out _);

        Assert.Equal(3m, rubric.TotalWeight);
    }
}