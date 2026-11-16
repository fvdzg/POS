namespace Pos.Domain.Rubrics;

public interface IRubricRepository
{
    Rubric? Find();

    void Save(Rubric rubric);
}