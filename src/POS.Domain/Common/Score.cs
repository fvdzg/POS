using System.Globalization;

namespace Pos.Domain.Common;

public readonly record struct Score
{
    public Score(decimal value)
    {
        if (value < 0)
        {
            throw new DomainException("Балл не может быть отрицательным.");
        }

        Value = value;
    }

    public decimal Value { get; }

    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}