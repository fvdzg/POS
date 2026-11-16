using Pos.Domain.Common;
using Pos.Domain.Students;

namespace Pos.Domain.Results;

public sealed record FinalResult
{
    internal FinalResult(StudentId studentId, decimal percent, int ratedCriteria, int totalCriteria, string method)
    {
        if (percent < 0 || percent > 100)
        {
            throw new DomainException("Итог в процентах должен быть в диапазоне от 0 до 100.");
        }

        if (ratedCriteria < 0 || ratedCriteria > totalCriteria)
        {
            throw new DomainException("Некорректное число оценённых критериев.");
        }

        StudentId = studentId;
        Percent = percent;
        RatedCriteria = ratedCriteria;
        TotalCriteria = totalCriteria;
        Method = method;
    }

    public StudentId StudentId { get; }

    public decimal Percent { get; }

    public decimal TenPointMark => Percent / 10m;

    public int RatedCriteria { get; }

    public int TotalCriteria { get; }

    public string Method { get; }

    public bool IsComplete => TotalCriteria > 0 && RatedCriteria == TotalCriteria;
}