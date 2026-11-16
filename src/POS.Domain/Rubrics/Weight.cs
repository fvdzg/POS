using System.Globalization;
using Pos.Domain.Common;

namespace Pos.Domain.Rubrics;

public readonly record struct Weight
{
    public Weight(decimal value)
    {
        if (value <= 0)
        {
            throw new DomainException("Вес критерия должен быть больше нуля.");
        }

        Value = value;
    }

    public decimal Value { get; }

    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}