namespace CadBridge.Core.Models;

public readonly struct Measure : IEquatable<Measure>
{
    public double Value { get; }

    public MeasureUnit Unit { get; }

    public Measure(double value, MeasureUnit unit)
    {
        if (!double.IsFinite(value) || value < 0)
            throw new ArgumentOutOfRangeException(nameof(value), "Measure must be a finite, non-negative number.");
        Value = value;
        Unit = unit;
    }

    public bool Equals(Measure other) => Value.Equals(other.Value) && Unit == other.Unit;
    public override bool Equals(object? obj) => obj is Measure other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Value, Unit);

    public static bool operator ==(Measure left, Measure right) => left.Equals(right);
    public static bool operator !=(Measure left, Measure right) => !left.Equals(right);

    public override string ToString() => $"{Value} {Unit.ToSymbol()}";
}

