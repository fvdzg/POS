using Pos.Domain.Students;

namespace Pos.Infrastructure.Persistence.Json;

public sealed class JsonStudentRepository : IStudentRepository
{
    private readonly JsonFileStore _store;

    public JsonStudentRepository(string path)
    {
        _store = new JsonFileStore(path);
    }

    public IReadOnlyList<Student> GetAll() => Load().Select(ToDomain).ToList();

    public Student? Find(StudentId id) => GetAll().FirstOrDefault(student => student.Id == id);

    public void Add(Student student)
    {
        var rows = Load();
        rows.Add(new StudentRecord(student.Id.Value, student.FullName, student.Group));
        _store.Write(rows);
    }

    public bool Remove(StudentId id)
    {
        var rows = Load();
        var removed = rows.RemoveAll(row => row.Id == id.Value) > 0;

        if (removed)
        {
            _store.Write(rows);
        }

        return removed;
    }

    private List<StudentRecord> Load()
        => _store.Read<List<StudentRecord>>() ?? new List<StudentRecord>();

    private static Student ToDomain(StudentRecord row)
        => new(new StudentId(row.Id), row.FullName, row.Group);
}