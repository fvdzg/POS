using Pos.Application;
using Pos.Domain.Common;
using Pos.Domain.Students;

namespace Pos.Tests.UseCases;

public class StudentUseCasesTests : IDisposable
{
    private readonly TestApp _app = new();

    public void Dispose() => _app.Dispose();

    [Fact]
    public void Add_ThenList_ReturnsStudentWithGroup()
    {
        var added = _app.Facade.AddStudent("  Иванов Иван ", "ПМИ-231");

        var student = Assert.Single(_app.Facade.GetStudents());

        Assert.Equal(added.Id, student.Id);
        Assert.Equal("Иванов Иван", student.FullName);
        Assert.Equal("ПМИ-231", student.Group);
    }

    [Fact]
    public void Add_EmptyName_Throws()
    {
        Assert.Throws<DomainException>(() => _app.Facade.AddStudent("   ", null));
    }

    [Fact]
    public void Remove_StudentWithEvaluations_RemovesSheetToo()
    {
        var (homework, _) = _app.SeedRubric();
        var student = _app.Facade.AddStudent("Иванов Иван", null);
        _app.Facade.Rate(student.Id, homework.Id, 7m, null);

        _app.Facade.RemoveStudent(student.Id);

        Assert.Empty(_app.Facade.GetStudents());
        Assert.Null(_app.Sheets.Find(new StudentId(student.Id)));
    }

    [Fact]
    public void Remove_UnknownStudent_Throws()
    {
        Assert.Throws<UseCaseException>(() => _app.Facade.RemoveStudent(Guid.NewGuid()));
    }
}