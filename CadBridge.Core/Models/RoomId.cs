namespace CadBridge.Core.Models;

public readonly struct RoomId : IEquatable<RoomId>
{
    private readonly string? _value;

    private RoomId(string value) => _value = value;

    public string Value => _value ?? string.Empty;

    public bool IsEmpty => _value is null;

    public static RoomId From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Room id must be a non-empty handle.", nameof(value));
        return new RoomId(value);
    }

    public static bool TryParse(string? value, out RoomId id)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            id = default;
            return false;
        }

        id = new RoomId(value);
        return true;
    }

    public bool Equals(RoomId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
    public override bool Equals(object? obj) => obj is RoomId other && Equals(other);
    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

    public static bool operator ==(RoomId left, RoomId right) => left.Equals(right);
    public static bool operator !=(RoomId left, RoomId right) => !left.Equals(right);

    public override string ToString() => Value;
}

