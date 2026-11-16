using Pos.Domain.Rubrics;

namespace Pos.Tests.Domain;

internal static class TestData
{
    public static readonly DateTimeOffset Now = new(2026, 11, 1, 10, 0, 0, TimeSpan.Zero);

    public static Rubric CreateRubric(out Criterion homework, out Criterion exam)
    {
        var rubric = Rubric.Create("Конструирование ПО");
        homework = rubric.AddCriterion("Домашние работы", 10m, new Weight(2m));
        exam = rubric.AddCriterion("Экзамен", 5m, new Weight(1m));
        return rubric;
    }
}