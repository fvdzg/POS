using Pos.Domain.Common;
using Pos.Domain.Students;

namespace Pos.Tests.Domain;

public class StudentTests
{
    [Fact]
    public void Create_TrimsNameAndGroup()
    {
        var student = Student.Create("  Иванов Иван  ", " БПИ-241 ");

        Assert.Equal("Иванов Иван", student.FullName);
        Assert.Equal("БПИ-241", student.Group);
    }

    [Fact]
    public void Create_BlankGroup_BecomesNull()
    {
        var student = Student.Create("Иванов Иван", "   ");

        Assert.Null(student.Group);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptyName_Throws(string name)
    {
        Assert.Throws<DomainException>(() => Student.Create(name));
    }

    [Fact]
    public void Students_WithSameId_AreEqual()
    {
        var id = StudentId.New();

        Assert.Equal(new Student(id, "Иванов Иван"), new Student(id, "Другое имя"));
    }
}