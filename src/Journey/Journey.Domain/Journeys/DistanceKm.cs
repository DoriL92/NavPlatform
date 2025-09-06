using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Domain.Journeys;
public sealed class DistanceKm
{
    public decimal Value { get; }

    private DistanceKm(decimal value)
    {
        if (value < 0 || value > 999.99m) 
            throw new ArgumentOutOfRangeException(nameof(value));
        Value = Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }

    public static DistanceKm From(decimal value) => new(value);

    public override string ToString() => Value.ToString("0.00");
    public static implicit operator decimal(DistanceKm d) => d.Value;
}