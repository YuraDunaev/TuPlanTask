using CadBridge.Core.Models;

namespace CadBridge.Core.Tests.Models;

public sealed class RoomTests
{
    private static Boundary Boundary() => new(new[]
    {
        new BoundaryVertex(0, 0, 0),
        new BoundaryVertex(500, 0, 0),
        new BoundaryVertex(500, 369, 0),
        new BoundaryVertex(0, 369, 0),
    });

    [Fact]
    public void Constructor_KeepsAllFields()
    {
        var room = new Room(
            RoomId.From("2A5"),
            "105",
            "Living room",
            18.45,
            17.38,
            new Point2D(250, 184.5),
            Boundary());

        Assert.Equal("2A5", room.Id.Value);
        Assert.Equal("105", room.Number);
        Assert.Equal("Living room", room.Name);
        Assert.Equal(18.45, room.Area);
        Assert.Equal(17.38, room.Perimeter);
        Assert.Equal(new Point2D(250, 184.5), room.LabelPoint);
        Assert.Equal(4, room.Boundary.Count);
    }

    [Fact]
    public void Constructor_RequiresStableId()
    {
        Assert.Throws<ArgumentException>(() => new Room(
            default,
            "105",
            null,
            18.45,
            17.38,
            new Point2D(0, 0),
            Boundary()));
    }

    [Fact]
    public void Constructor_RejectsNullBoundary()
    {
        Assert.Throws<ArgumentNullException>(() => new Room(
            RoomId.From("2A5"),
            "105",
            null,
            18.45,
            17.38,
            new Point2D(0, 0),
            null!));
    }

    [Fact]
    public void Constructor_RejectsNegativeOrNonFiniteArea()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Room(
            RoomId.From("2A5"), "105", null, -1, 17.38, new Point2D(0, 0), Boundary()));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Room(
            RoomId.From("2A5"), "105", null, double.NaN, 17.38, new Point2D(0, 0), Boundary()));
    }

    [Fact]
    public void Constructor_RejectsNegativeOrNonFinitePerimeter()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Room(
            RoomId.From("2A5"), "105", null, 18.45, -1, new Point2D(0, 0), Boundary()));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Room(
            RoomId.From("2A5"), "105", null, 18.45, double.PositiveInfinity, new Point2D(0, 0), Boundary()));
    }

    [Fact]
    public void Constructor_NormalisesMissingNumberAndName()
    {
        var room = new Room(
            RoomId.From("2A5"), null, null, 18.45, 17.38, new Point2D(0, 0), Boundary());

        Assert.Equal(string.Empty, room.Number);
        Assert.Equal(string.Empty, room.Name);
    }

    [Fact]
    public void ToString_IncludesNumberWhenPresent()
    {
        var room = new Room(
            RoomId.From("2A5"), "105", "Living room", 18.45, 17.38, new Point2D(0, 0), Boundary());

        Assert.Equal("Room 105 (2A5)", room.ToString());
    }

    [Fact]
    public void ToString_FallsBackToIdWithoutNumber()
    {
        var room = new Room(
            RoomId.From("2A5"), null, null, 18.45, 17.38, new Point2D(0, 0), Boundary());

        Assert.Equal("Room 2A5", room.ToString());
    }
}

public sealed class SourceInfoTests
{
    [Fact]
    public void Record_ExposesFieldsAndValueEquality()
    {
        var a = new SourceInfo("AutoCAD 2025", "rooms_sample.dwg", DateTimeOffset.UnixEpoch, "cm");
        var b = new SourceInfo("AutoCAD 2025", "rooms_sample.dwg", DateTimeOffset.UnixEpoch, "cm");

        Assert.Equal("AutoCAD 2025", a.Application);
        Assert.Equal("rooms_sample.dwg", a.File);
        Assert.Equal(DateTimeOffset.UnixEpoch, a.ExportedAt);
        Assert.Equal("cm", a.Units);
        Assert.Equal(a, b);
    }
}
