using CadBridge.Core.Models;
using CadBridge.Core.Serialization;

namespace CadBridge.Core.Tests.Serialization;

public sealed class RoomsJsonSerializerTests
{
    private static RoomsDocument Sample() => new(
        new SourceInfo("AutoCAD 2025", "rooms_sample.dwg", new DateTimeOffset(2026, 10, 6, 12, 0, 0, TimeSpan.Zero), "cm"),
        new[]
        {
            new Room(
                RoomId.From("2A7F"),
                "1",
                "105",
                18.45,
                17.38,
                new Point2D(250, 184.5),
                new Boundary(new[]
                {
                    new BoundaryVertex(0, 0, 0),
                    new BoundaryVertex(500, 0, 0),
                    new BoundaryVertex(500, 369, 0),
                    new BoundaryVertex(0, 369, 0),
                })),
        },
        new[]
        {
            new Warning(WarningCode.OpenPolyline, "2B10", "Polyline on A-ROOM is not closed."),
            new Warning(WarningCode.NoTag, "2B15", "Room has no tag."),
        });

    [Fact]
    public void Serialize_MatchesSection6Shape()
    {
        var json = RoomsJsonSerializer.Serialize(Sample());

        Assert.Contains("\"schemaVersion\": \"1.0\"", json);
        Assert.Contains("\"source\"", json);
        Assert.Contains("\"rooms\"", json);
        Assert.Contains("\"warnings\"", json);
        Assert.Contains("\"labelPoint\"", json);
        Assert.Contains("\"area\": 18.45", json);
        Assert.Contains("\"perimeter\": 17.38", json);

        Assert.Contains("\"boundary\"", json);
        Assert.Contains("\"bulge\"", json);
        Assert.DoesNotContain("\"vertices\"", json);

        Assert.DoesNotContain("\"SchemaVersion\"", json);
        Assert.DoesNotContain("\"ExportedAt\"", json);

        Assert.Contains("\n", json);
    }

    [Fact]
    public void Serialize_WarningCodesAreScreamingSnake()
    {
        var json = RoomsJsonSerializer.Serialize(Sample());
        Assert.Contains("OPEN_POLYLINE", json);
        Assert.Contains("NO_TAG", json);
        Assert.DoesNotContain("\"OpenPolyline\"", json);
    }

    [Fact]
    public void RoundTrip_PreservesValues()
    {
        var original = Sample();
        var restored = RoomsJsonSerializer.Deserialize(RoomsJsonSerializer.Serialize(original));

        Assert.NotNull(restored);
        Assert.Equal(original.SchemaVersion, restored!.SchemaVersion);
        Assert.Equal(original.Source.Application, restored.Source.Application);
        Assert.Equal(original.Source.File, restored.Source.File);
        Assert.Equal(original.Source.Units, restored.Source.Units);
        Assert.Equal(original.Source.ExportedAt, restored.Source.ExportedAt);

        Assert.Equal(original.Rooms.Count, restored.Rooms.Count);
        var before = original.Rooms[0];
        var after = restored.Rooms[0];
        Assert.Equal(before.Id, after.Id);
        Assert.Equal(before.Number, after.Number);
        Assert.Equal(before.Name, after.Name);
        Assert.Equal(before.Area, after.Area);
        Assert.Equal(before.Perimeter, after.Perimeter);
        Assert.Equal(before.LabelPoint, after.LabelPoint);
        Assert.Equal(before.Boundary, after.Boundary);

        Assert.Equal(original.Warnings.Count, restored.Warnings.Count);
        Assert.Equal(WarningCode.OpenPolyline, restored.Warnings[0].Code);
        Assert.Equal("2B10", restored.Warnings[0].EntityId);
        Assert.Equal(WarningCode.NoTag, restored.Warnings[1].Code);
    }

    [Fact]
    public void RoundTrip_PreservesArcBoundary()
    {
        var document = new RoomsDocument(
            new SourceInfo("AutoCAD 2025", "a.dwg", DateTimeOffset.UnixEpoch, "cm"),
            new[]
            {
                new Room(
                    RoomId.From("ARC1"),
                    "2",
                    "201",
                    22.816850,
                    17.967,
                    new Point2D(250, 184),
                    new Boundary(new[]
                    {
                        new BoundaryVertex(0, 0, 0),
                        new BoundaryVertex(500, 0, 0.5),
                        new BoundaryVertex(500, 369, 0),
                        new BoundaryVertex(0, 369, 0),
                    })),
            });

        var restored = RoomsJsonSerializer.Deserialize(RoomsJsonSerializer.Serialize(document));

        Assert.NotNull(restored);
        Assert.Equal(4, restored!.Rooms[0].Boundary.Count);
        Assert.Equal(0.5, restored.Rooms[0].Boundary[1].Bulge);
        Assert.True(restored.Rooms[0].Boundary[1].IsArc);
    }

    [Fact]
    public void Deserialize_UnknownWarningCode_BecomesUnknown_DoesNotThrow()
    {
        const string json = """
        {
          "schemaVersion": "1.0",
          "source": { "application": "x", "file": "y", "exportedAt": "2026-01-01T00:00:00Z", "units": "cm" },
          "rooms": [],
          "warnings": [ { "code": "SOME_FUTURE_CODE", "entityId": "z", "message": "future" } ]
        }
        """;

        var document = RoomsJsonSerializer.Deserialize(json);

        Assert.NotNull(document);
        Assert.Equal(WarningCode.UnknownCode, document!.Warnings[0].Code);
    }

    [Fact]
    public void Serialize_NullDocument_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => RoomsJsonSerializer.Serialize((RoomsDocument)null!));
    }

    [Fact]
    public void Deserialize_EmptyWarningMessage_BecomesPlaceholder()
    {
        const string json = """
        {
          "schemaVersion": "1.0",
          "source": { "application": "x", "file": "y", "exportedAt": "2026-01-01T00:00:00Z", "units": "cm" },
          "rooms": [],
          "warnings": [ { "code": "NO_TAG", "entityId": "9A", "message": "" } ]
        }
        """;

        var document = RoomsJsonSerializer.Deserialize(json);

        Assert.NotNull(document);
        Assert.Single(document!.Warnings);
        Assert.Equal("(no message)", document.Warnings[0].Message);
    }

    [Fact]
    public void Deserialize_RoomWithoutId_GeneratesNonEmptyHandle()
    {
        const string json = """
        {
          "schemaVersion": "1.0",
          "source": { "application": "x", "file": "y", "exportedAt": "2026-01-01T00:00:00Z", "units": "cm" },
          "rooms": [ { "number": "7", "area": 12.3, "perimeter": 8.1, "labelPoint": { "x": 1, "y": 2 }, "boundary": [ { "x": 0, "y": 0, "bulge": 0 }, { "x": 100, "y": 0, "bulge": 0 }, { "x": 100, "y": 100, "bulge": 0 }, { "x": 0, "y": 100, "bulge": 0 } ] } ],
          "warnings": []
        }
        """;

        var document = RoomsJsonSerializer.Deserialize(json);

        Assert.NotNull(document);
        Assert.Single(document!.Rooms);
        Assert.False(string.IsNullOrWhiteSpace(document.Rooms[0].Id.Value));
        Assert.NotEqual("00000000-0000-0000-0000-0000-0000-0000", document.Rooms[0].Id.Value);
        Assert.Equal("7", document.Rooms[0].Number);
    }

    [Fact]
    public void Deserialize_NullSource_DefaultsToEmptyStrings()
    {
        const string json = """
        {
          "schemaVersion": "1.0",
          "source": null,
          "rooms": [],
          "warnings": []
        }
        """;

        var document = RoomsJsonSerializer.Deserialize(json);

        Assert.NotNull(document);
        Assert.Equal(string.Empty, document!.Source.Application);
        Assert.Equal(string.Empty, document.Source.File);
        Assert.Equal(string.Empty, document.Source.Units);
        Assert.Equal(DateTimeOffset.UnixEpoch, document.Source.ExportedAt);
    }
}
