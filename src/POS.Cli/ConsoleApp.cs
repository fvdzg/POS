using System.Text.Json;
using Pos.Application;
using Pos.Application.Dtos;
using Pos.Application.Scoring;
using Pos.Domain.Common;

namespace Pos.Cli;

public sealed class ConsoleApp
{
    private const string DefaultExportPath = "results.csv";

    private readonly PosFacade _pos;
    private readonly ConsoleIo _io;
    private readonly List<(string Title, Action Run)> _commands;
    private ScoringMode _mode = ScoringMode.Strict;

    public ConsoleApp(PosFacade pos, ConsoleIo io)
    {
        _pos = pos;
        _io = io;
        _commands = new List<(string Title, Action Run)>
        {
            ("Список студентов", ShowStudents),
            ("Добавить студента", AddStudent),
            ("Удалить студента", RemoveStudent),
            ("Список критериев", ShowCriteria),
            ("Добавить критерий", AddCriterion),
            ("Удалить критерий", RemoveCriterion),
            ("Выставить или исправить оценку", RateStudent),
            ("Оценки студента", ShowEvaluations),
            ("Отменить оценку", CancelEvaluation),
            ("Итог студента", ShowFinal),
            ("Итоги всех студентов", ShowAllFinals),
            ("Экспорт итогов в файл (.csv или .json)", ExportResults),
            ("Сменить режим расчёта итога", SwitchMode)
        };
    }

    public void Run()
    {
        while (true)
        {
            PrintMenu();
            var choice = _io.ReadText("Выберите пункт: ");

            if (choice == "0")
            {
                return;
            }

            if (int.TryParse(choice, out var number) && number >= 1 && number <= _commands.Count)
            {
                Execute(_commands[number - 1].Run);
            }
            else
            {
                _io.Error("Нет такого пункта меню.");
            }
        }
    }

    private void PrintMenu()
    {
        _io.Header("ПОС: преподавательская оценка студентов");
        _io.WriteLine($"Режим расчёта итога: {ModeTitle(_mode)}");

        for (var i = 0; i < _commands.Count; i++)
        {
            _io.WriteLine($"{i + 1,2}. {_commands[i].Title}");
        }

        _io.WriteLine(" 0. Выход");
    }

    private void Execute(Action action)
    {
        try
        {
            action();
        }
        catch (DomainException ex)
        {
            _io.Error(ex.Message);
        }
        catch (UseCaseException ex)
        {
            _io.Error(ex.Message);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException
                                       or InvalidOperationException)
        {
            _io.Error($"Ошибка работы с файлами данных: {ex.Message}");
        }
    }

    private void ShowStudents()
    {
        var students = _pos.GetStudents();

        if (students.Count == 0)
        {
            _io.Info("Студентов пока нет.");
            return;
        }

        _io.Header("Студенты");
        for (var i = 0; i < students.Count; i++)
        {
            _io.WriteLine($"{i + 1}. {DescribeStudent(students[i])}");
        }
    }

    private void AddStudent()
    {
        var name = _io.ReadText("ФИО студента: ");
        var group = _io.ReadOptional("Группа (Enter — пропустить): ");

        var student = _pos.AddStudent(name, group);
        _io.Success($"Добавлен студент: {DescribeStudent(student)}");
    }

    private void RemoveStudent()
    {
        var student = PickStudent("Кого удалить?");

        if (student is null || !_io.Confirm($"Удалить «{student.FullName}» вместе с оценками?"))
        {
            return;
        }

        _pos.RemoveStudent(student.Id);
        _io.Success("Студент удалён.");
    }

    private void ShowCriteria()
    {
        var criteria = _pos.GetCriteria();

        if (criteria.Count == 0)
        {
            _io.Info("Критериев пока нет.");
            return;
        }

        _io.Header("Рубрикатор");
        for (var i = 0; i < criteria.Count; i++)
        {
            _io.WriteLine($"{i + 1}. {DescribeCriterion(criteria[i])}");
        }
    }

    private void AddCriterion()
    {
        var name = _io.ReadText("Название критерия: ");
        var maxScore = _io.ReadDecimal("Максимальный балл: ");
        var weight = _io.ReadDecimal("Вес критерия: ");

        var criterion = _pos.AddCriterion(name, maxScore, weight);
        _io.Success($"Добавлен критерий: {DescribeCriterion(criterion)}");
    }

    private void RemoveCriterion()
    {
        var criterion = PickCriterion("Какой критерий удалить?");

        if (criterion is null || !_io.Confirm($"Удалить критерий «{criterion.Name}»?"))
        {
            return;
        }

        _pos.RemoveCriterion(criterion.Id);
        _io.Success("Критерий удалён.");
    }

    private void RateStudent()
    {
        var student = PickStudent("Кого оценить?");
        if (student is null)
        {
            return;
        }

        var criterion = PickCriterion("По какому критерию?");
        if (criterion is null)
        {
            return;
        }

        var score = _io.ReadDecimal($"Балл (от 0 до {criterion.MaxScore:0.##}): ");
        var comment = _io.ReadOptional("Комментарий (Enter — без комментария): ");

        var result = _pos.Rate(student.Id, criterion.Id, score, comment);
        var action = result.WasCorrection ? "исправлена" : "выставлена";
        _io.Success($"Оценка {action}: {student.FullName}, {DescribeEvaluation(result.Evaluation)}");
    }

    private void ShowEvaluations()
    {
        var student = PickStudent("Чьи оценки показать?");
        if (student is null)
        {
            return;
        }

        var evaluations = _pos.GetEvaluations(student.Id);

        if (evaluations.Count == 0)
        {
            _io.Info("Оценок пока нет.");
            return;
        }

        _io.Header($"Оценки: {student.FullName}");
        foreach (var evaluation in evaluations)
        {
            _io.WriteLine($"{DescribeEvaluation(evaluation)} [{evaluation.EvaluatedAt.LocalDateTime:dd.MM.yyyy HH:mm}]");
        }
    }

    private void CancelEvaluation()
    {
        var student = PickStudent("У кого отменить оценку?");
        if (student is null)
        {
            return;
        }

        var evaluations = _pos.GetEvaluations(student.Id);

        if (evaluations.Count == 0)
        {
            _io.Info("У студента нет оценок.");
            return;
        }

        var evaluation = _io.Choose("Какую оценку отменить?", evaluations, DescribeEvaluation);
        if (evaluation is null)
        {
            return;
        }

        _pos.CancelEvaluation(student.Id, evaluation.CriterionId);
        _io.Success("Оценка отменена.");
    }

    private void ShowFinal()
    {
        var student = PickStudent("Итог какого студента показать?");
        if (student is null)
        {
            return;
        }

        var result = _pos.CalculateFinal(student.Id, _mode);

        _io.Header("Итог студента");
        _io.WriteResult(result);
        _io.WriteLine($"Правило: {result.Method}");
    }

    private void ShowAllFinals()
    {
        var results = _pos.CalculateAllFinals(_mode);

        if (results.Count == 0)
        {
            _io.Info("Студентов пока нет.");
            return;
        }

        _io.Header($"Итоги, режим: {ModeTitle(_mode)}");
        foreach (var result in results)
        {
            _io.WriteResult(result);
        }
    }

    private void ExportResults()
    {
        var path = _io.ReadOptional($"Файл выгрузки (Enter — {DefaultExportPath}): ") ?? DefaultExportPath;

        var count = _pos.ExportResults(path, _mode);
        _io.Success($"Выгружено студентов: {count}. Файл: {Path.GetFullPath(path)}");
    }

    private void SwitchMode()
    {
        _mode = _mode == ScoringMode.Strict ? ScoringMode.RatedOnly : ScoringMode.Strict;
        _io.Success($"Режим расчёта итога: {ModeTitle(_mode)}");
    }

    private StudentDto? PickStudent(string title)
    {
        var students = _pos.GetStudents();

        if (students.Count == 0)
        {
            _io.Info("Студентов пока нет.");
            return null;
        }

        return _io.Choose(title, students, DescribeStudent);
    }

    private CriterionDto? PickCriterion(string title)
    {
        var criteria = _pos.GetCriteria();

        if (criteria.Count == 0)
        {
            _io.Info("Критериев пока нет.");
            return null;
        }

        return _io.Choose(title, criteria, DescribeCriterion);
    }

    private static string DescribeStudent(StudentDto student)
        => student.Group is null ? student.FullName : $"{student.FullName} ({student.Group})";

    private static string DescribeCriterion(CriterionDto criterion)
        => $"{criterion.Name}: максимум {criterion.MaxScore:0.##}, вес {criterion.Weight:0.##}";

    private static string DescribeEvaluation(EvaluationDto evaluation)
    {
        var text = $"{evaluation.CriterionName}: {evaluation.Score:0.##} из {evaluation.MaxScore:0.##}";
        return evaluation.Comment is null ? text : $"{text} — {evaluation.Comment}";
    }

    private static string ModeTitle(ScoringMode mode) => mode switch
    {
        ScoringMode.Strict => "строгий (неоценённые критерии = 0)",
        ScoringMode.RatedOnly => "только по оценённым критериям",
        _ => mode.ToString()
    };
}