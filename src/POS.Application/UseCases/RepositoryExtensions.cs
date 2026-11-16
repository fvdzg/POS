using Pos.Domain.Rubrics;
using Pos.Domain.Students;

namespace Pos.Application.UseCases;

internal static class RepositoryExtensions
{
    public static Rubric GetRequiredRubric(this IRubricRepository rubrics)
        => rubrics.Find()
           ?? throw new UseCaseException("Рубрикатор пока пуст: сначала добавьте хотя бы один критерий.");

    public static Student GetRequiredStudent(this IStudentRepository students, Guid studentId)
        => students.Find(new StudentId(studentId))
           ?? throw new UseCaseException("Студент не найден.");
}