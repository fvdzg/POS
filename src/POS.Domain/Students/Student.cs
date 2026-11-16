using Pos.Domain.Common;

namespace Pos.Domain.Students;

public sealed class Student : Entity<StudentId>
{
    public const int MaxNameLength = 200;
    public const int MaxGroupLength = 50;

    public Student(StudentId id, string fullName, string? group = null) : base(id)
    {
        FullName = Guard.RequireText(fullName, "ФИО студента", MaxNameLength);
        Group = Guard.OptionalText(group, "Группа", MaxGroupLength);
    }

    public string FullName { get; }

    public string? Group { get; }

    public static Student Create(string fullName, string? group = null)
        => new(StudentId.New(), fullName, group);
}