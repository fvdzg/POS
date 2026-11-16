using Pos.Domain.Rubrics;

namespace Pos.Infrastructure.Persistence.Json;

public sealed class JsonRubricRepository : IRubricRepository
{
    private readonly JsonFileStore _store;

    public JsonRubricRepository(string path)
    {
        _store = new JsonFileStore(path);
    }

    public Rubric? Find()
    {
        var row = _store.Read<RubricRecord>();
        if (row is null)
        {
            return null;
        }

        var rubric = new Rubric(new RubricId(row.Id), row.Title);
        foreach (var criterion in row.Criteria)
        {
            rubric.AddCriterion(
                new CriterionId(criterion.Id),
                criterion.Name,
                criterion.MaxScore,
                new Weight(criterion.Weight));
        }

        return rubric;
    }

    public void Save(Rubric rubric)
    {
        var criteria = rubric.Criteria
            .Select(c => new CriterionRecord(c.Id.Value, c.Name, c.MaxScore, c.Weight.Value))
            .ToList();

        _store.Write(new RubricRecord(rubric.Id.Value, rubric.Title, criteria));
    }
}