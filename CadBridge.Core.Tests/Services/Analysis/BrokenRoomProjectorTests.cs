using System;
using CadBridge.Core.Models;
using CadBridge.Core.Services.Analysis;
using Xunit;

namespace CadBridge.Core.Tests.Services.Analysis;

public class BrokenRoomProjectorTests
{
    private static RoomsDocument Document(params Warning[] warnings) =>
        new(new SourceInfo("AutoCAD", "rooms_sample.dwg", DateTimeOffset.UnixEpoch, "cm"),
            Array.Empty<Room>(), warnings);

    [Fact]
    public void Projects_open_polyline_as_invalid_room()
    {
        var doc = Document(new Warning(WarningCode.OpenPolyline, "41A", "not closed"));

        var result = BrokenRoomProjector.Project(doc, Array.Empty<string>());

        var room = Assert.Single(result);
        Assert.Equal("41A", room.Id);
        Assert.False(room.CadContourValid);
        Assert.Null(room.CadArea);
    }

    [Fact]
    public void Ignores_non_contour_warnings()
    {
        var doc = Document(new Warning(WarningCode.NoTag, "396", "no tag"));

        Assert.Empty(BrokenRoomProjector.Project(doc, Array.Empty<string>()));
    }

    [Fact]
    public void Skips_ids_already_present_in_report()
    {
        var doc = Document(new Warning(WarningCode.OpenPolyline, "41A", "not closed"));

        Assert.Empty(BrokenRoomProjector.Project(doc, new[] { "41A" }));
    }

    [Fact]
    public void Deduplicates_repeated_handles()
    {
        var doc = Document(
            new Warning(WarningCode.OpenPolyline, "41A", "a"),
            new Warning(WarningCode.CalculationFailed, "41A", "b"));

        Assert.Single(BrokenRoomProjector.Project(doc, Array.Empty<string>()));
    }
}
