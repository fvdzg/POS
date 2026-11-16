using Pos.Application;
using Pos.Application.Dtos;
using Pos.Application.Scoring;
using Pos.Domain.Common;

namespace Pos.Cli;

public sealed class DemoScenario
{
    private readonly PosFacade _pos;
    private readonly ConsoleIo _io;

    public DemoScenario(PosFacade pos, ConsoleIo io)
    {
        _pos = pos;
        _io = io;
    }

    public void Run(string exportDirectory)
    {
        _io.Header("Демо-сценарий: критерии, студенты, оценки, итог, экспорт");

        _io.WriteLine("Шаг 1. Заводим критерии рубрикатора");
        var homework = _pos.AddCriterion("Домашние работы", 10m, 3m);
        var test = _pos.AddCriterion("Контрольная работа", 20m, 3m);
        var exam = _pos.AddCriterion("Экзамен", 10m, 4m);
        foreach (var criterion in new[] { homework, test, exam })
        {
            _io.Success($"  {criterion.Name}: максимум {criterion.MaxScore:0.##}, вес {criterion.Weight:0.##}");
        }

        _io.WriteLine();
        _io.WriteLine("Шаг 2. Заводим студентов по группам");
        var ivan = _pos.AddStudent("Иванов Иван Петрович", "ПМИ-231");
        var anna = _pos.AddStudent("Петрова Анна Сергеевна", "ПМИ-231");
        var petr = _pos.AddStudent("Сидоров Пётр Алексеевич", "ПМИ-221");
        foreach (var group in new[] { ivan, anna, petr }.GroupBy(student => student.Group).OrderBy(g => g.Key))
        {
            _io.Success($"  Группа {group.Key}: {string.Join(", ", group.Select(student => student.FullName))}");
        }

        _io.WriteLine();
        _io.WriteLine("Шаг 3. Выставляем оценки");
        Rate(ivan, homework, 9m, "Отличная работа");
        Rate(ivan, test, 17m, null);
        Rate(ivan, exam, 8m, null);
        Rate(anna, homework, 10m, null);
        Rate(anna, test, 15m, "Две задачи с ошибками");
        Rate(petr, homework, 6m, null);

        _io.WriteLine();
        _io.WriteLine("Шаг 4. Модель не даёт нарушить правила");
        ExpectRejection("  Балл 12 при максимуме 10", () => _pos.Rate(petr.Id, homework.Id, 12m, null));
        ExpectRejection("  Отрицательный балл", () => _pos.Rate(petr.Id, homework.Id, -1m, null));
        ExpectRejection("  Критерий не из рубрикатора", () => _pos.Rate(petr.Id, Guid.NewGuid(), 5m, null));

        _io.WriteLine();
        _io.WriteLine("Шаг 5. Исправляем оценку");
        var correction = _pos.Rate(petr.Id, homework.Id, 8m, "Пересдача");
        _io.Success(
            $"  {petr.FullName}: {correction.Evaluation.CriterionName} теперь " +
            $"{correction.Evaluation.Score:0.##} из {correction.Evaluation.MaxScore:0.##}, " +
            $"исправление: {(correction.WasCorrection ? "да" : "нет")}");

        _io.WriteLine();
        _io.WriteLine("Шаг 6. Считаем итоги по группам в двух режимах");
        foreach (var mode in Enum.GetValues<ScoringMode>())
        {
            var results = _pos.CalculateAllFinals(mode);
            _io.Header($"Режим: {mode}");

            foreach (var group in results.GroupBy(result => result.Group).OrderBy(g => g.Key))
            {
                _io.WriteLine(group.Key is null ? "Без группы:" : $"Группа {group.Key}:");

                foreach (var result in group)
                {
                    _io.WriteResult(result);
                }
            }

            _io.WriteLine($"Правило: {results[0].Method}");
        }

        _io.WriteLine();
        _io.WriteLine("Шаг 7. Экспорт итога");
        var csvPath = Path.Combine(exportDirectory, "results.csv");
        var jsonPath = Path.Combine(exportDirectory, "results.json");
        _pos.ExportResults(csvPath, ScoringMode.Strict);
        _pos.ExportResults(jsonPath, ScoringMode.Strict);
        _io.Success($"  CSV:  {Path.GetFullPath(csvPath)}");
        _io.Success($"  JSON: {Path.GetFullPath(jsonPath)}");

        _io.Header("Демо завершено");
    }

    private void Rate(StudentDto student, CriterionDto criterion, decimal score, string? comment)
    {
        var result = _pos.Rate(student.Id, criterion.Id, score, comment);
        _io.Success(
            $"  {student.FullName}: {result.Evaluation.CriterionName} — " +
            $"{result.Evaluation.Score:0.##} из {result.Evaluation.MaxScore:0.##}");
    }

    private void ExpectRejection(string title, Action action)
    {
        try
        {
            action();
            _io.Error($"{title}: оценка принята, а должна быть отклонена");
        }
        catch (DomainException ex)
        {
            _io.Success($"{title} — отклонено: {ex.Message}");
        }
    }
}