using Pos.Application.Dtos;
using Pos.Domain.Evaluations;
using Pos.Domain.Students;

namespace Pos.Application.UseCases;

public sealed class StudentUseCases
{
    private readonly IStudentRepository _students;
    private readonly IEvaluationSheetRepository _sheets;

    public StudentUseCases(IStudentRepository students, IEvaluationSheetRepository sheets)
    {
        _students = students;
        _sheets = sheets;
    }

    public StudentDto Add(string fullName, string? group)
    {
        var student = Student.Create(fullName, group);
        _students.Add(student);
        return StudentDto.From(student);
    }

    public IReadOnlyList<StudentDto> List()
        => _students.GetAll().Select(StudentDto.From).ToList();

    public void Remove(Guid studentId)
    {
        var id = new StudentId(studentId);

        if (!_students.Remove(id))
        {
            throw new UseCaseException("Студент не найден.");
        }

        _sheets.Remove(id);
    }
}