namespace Pos.Domain.Students;

public interface IStudentRepository
{
    IReadOnlyList<Student> GetAll();

    Student? Find(StudentId id);

    void Add(Student student);

    bool Remove(StudentId id);
}