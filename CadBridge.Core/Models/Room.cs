namespace CadBridge.Core.Models;

public sealed class Room
{
    public RoomId Id { get; }

    public string Number { get; }

    public string Name { get; }

    public double Area { get; }

    public double Perimeter { get; }

    public Point2D LabelPoint { get; }

    public Boundary Boundary { get; }

    public Room(
        RoomId id,
        string? number,
        string? name,
        double area,
        double perimeter,
        Point2D labelPoint,
        Boundary boundary)
    {
        if (id.IsEmpty)
            throw new ArgumentException("Room must carry a stable id.", nameof(id));
        ArgumentNullException.ThrowIfNull(boundary);
        if (!double.IsFinite(area) || area < 0)
            throw new ArgumentOutOfRangeException(nameof(area), "Area must be a finite, non-negative number (m²).");
        if (!double.IsFinite(perimeter) || perimeter < 0)
            throw new ArgumentOutOfRangeException(nameof(perimeter), "Perimeter must be a finite, non-negative number (m).");

        Id = id;
        Number = number ?? string.Empty;
        Name = name ?? string.Empty;
        Area = area;
        Perimeter = perimeter;
        LabelPoint = labelPoint;
        Boundary = boundary;
    }

    public override string ToString() =>
        string.IsNullOrEmpty(Number) ? $"Room {Id}" : $"Room {Number} ({Id})";
}

