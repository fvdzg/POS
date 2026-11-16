using Pos.Domain.Evaluations;
using Pos.Domain.Rubrics;
using Pos.Domain.Students;

namespace Pos.Infrastructure.Persistence.Json;

public sealed class JsonEvaluationSheetRepository : IEvaluationSheetRepository
{
    private readonly JsonFileStore _store;
    private readonly IRubricRepository _rubrics;
    private readonly EvaluationFactory _factory;

    public JsonEvaluationSheetRepository(string path, IRubricRepository rubrics, EvaluationFactory factory)
    {
        _store = new JsonFileStore(path);
        _rubrics = rubrics;
        _factory = factory;
    }

    public EvaluationSheet? Find(StudentId studentId)
    {
        var row = Load().FirstOrDefault(item => item.StudentId == studentId.Value);
        return row is null ? null : Restore(row, RequireRubric());
    }

    public IReadOnlyList<EvaluationSheet> GetAll()
    {
        var rows = Load();
        if (rows.Count == 0)
        {
            return Array.Empty<EvaluationSheet>();
        }

        var rubric = RequireRubric();
        return rows.Select(row => Restore(row, rubric)).ToList();
    }

    public void Save(EvaluationSheet sheet)
    {
        var rows = Load();
        rows.RemoveAll(item => item.StudentId == sheet.StudentId.Value);
        rows.Add(ToRecord(sheet));
        _store.Write(rows);
    }

    public bool Remove(StudentId studentId)
    {
        var rows = Load();
        var removed = rows.RemoveAll(item => item.StudentId == studentId.Value) > 0;

        if (removed)
        {
            _store.Write(rows);
        }

        return removed;
    }

    private List<SheetRecord> Load()
        => _store.Read<List<SheetRecord>>() ?? new List<SheetRecord>();

    private Rubric RequireRubric()
        => _rubrics.Find()
           ?? throw new InvalidOperationException(
               "В хранилище есть оценки, но нет рубрикатора: данные повреждены.");

    private EvaluationSheet Restore(SheetRecord row, Rubric rubric)
    {
        var sheet = new EvaluationSheet(new StudentId(row.StudentId));

        foreach (var item in row.Evaluations)
        {
            var evaluation = _factory.Create(
                rubric,
                new CriterionId(item.CriterionId),
                item.Score,
                item.Comment,
                item.EvaluatedAt);

            sheet.Assign(evaluation);
        }

        return sheet;
    }

    private static SheetRecord ToRecord(EvaluationSheet sheet)
    {
        var evaluations = sheet.Evaluations
            .Select(e => new EvaluationRecord(e.CriterionId.Value, e.Score.Value, e.Comment, e.EvaluatedAt))
            .ToList();

        return new SheetRecord(sheet.StudentId.Value, evaluations);
    }
}