using CadBridge.Core.Models;

namespace CadBridge.Core.Tests.Models;

public sealed class WarningTests
{
    [Fact]
    public void Constructor_RequiresExplanatoryMessage()
    {
        Assert.Throws<ArgumentException>(
            () => new Warning(WarningCode.NoTag, "1A", "   "));
        Assert.Throws<ArgumentException>(
            () => new Warning(WarningCode.NoTag, "1A", null));
    }

    [Fact]
    public void Constructor_NormalisesMissingEntityId()
    {
        var warning = new Warning(WarningCode.OpenPolyline, null, "Not closed.");

        Assert.Equal(string.Empty, warning.EntityId);
        Assert.Equal(WarningCode.OpenPolyline, warning.Code);
        Assert.Equal("Not closed.", warning.Message);
    }

    [Fact]
    public void ToString_ContainsCodeMessageAndEntity()
    {
        var warning = new Warning(WarningCode.OrphanTag, "9F", "Tag outside any room.");

        Assert.Contains("OrphanTag", warning.ToString());
        Assert.Contains("Tag outside any room.", warning.ToString());
        Assert.Contains("9F", warning.ToString());
    }
}

public sealed class RoomsDocumentTests
{
    private static SourceInfo Source() =>
        new("AutoCAD 2025", "rooms_sample.dwg", DateTimeOffset.UnixEpoch, "cm");

    [Fact]
    public void Defaults_ToSchemaVersionOne()
    {
        var document = new RoomsDocument(Source(), Array.Empty<Room>());

        Assert.Equal("1.0", document.SchemaVersion);
        Assert.Empty(document.Warnings);
        Assert.Empty(document.Rooms);
    }

    [Fact]
    public void Constructor_RejectsNullArguments()
    {
        Assert.Throws<ArgumentNullException>(() => new RoomsDocument(null!, Array.Empty<Room>()));
        Assert.Throws<ArgumentNullException>(() => new RoomsDocument(Source(), null!));
    }

    [Fact]
    public void BlankSchemaVersion_FallsBackToCurrent()
    {
        var document = new RoomsDocument(Source(), Array.Empty<Room>(), null, "   ");

        Assert.Equal("1.0", document.SchemaVersion);
    }

    [Fact]
    public void ExplicitSchemaVersion_IsPreserved()
    {
        var document = new RoomsDocument(Source(), Array.Empty<Room>(), null, "2.1");

        Assert.Equal("2.1", document.SchemaVersion);
    }

    [Fact]
    public void CurrentSchemaVersion_ConstantIsOne()
    {
        Assert.Equal("1.0", RoomsDocument.CurrentSchemaVersion);
    }

    [Fact]
    public void Keeps_RoomsAndWarnings()
    {
        var room = MakeRoom("2A5", "105");
        var warning = new Warning(WarningCode.NoTag, "2A5", "No tag inside.");

        var document = new RoomsDocument(Source(), new[] { room }, new[] { warning }, "1.0");

        Assert.Single(document.Rooms);
        Assert.Single(document.Warnings);
        Assert.Equal("105", document.Rooms[0].Number);
    }

    private static Room MakeRoom(string handle, string number) => new(
        RoomId.From(handle),
        number,
        "Living room",
        18.45,
        17.38,
        new Point2D(250, 184.5),
        new Boundary(new[]
        {
            new BoundaryVertex(0, 0, 0),
            new BoundaryVertex(500, 0, 0),
            new BoundaryVertex(500, 369, 0),
            new BoundaryVertex(0, 369, 0),
        }));
}
