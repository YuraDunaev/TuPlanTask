using CadBridge.Core.Models;
using CadBridge.Core.Services.Matching;

namespace CadBridge.Core.Tests.Services.Matching;

public sealed class RoomBlockMatcherTests
{
    private static Boundary Rectangle(double x, double y, double width, double height) => new(new[]
    {
        new BoundaryVertex(x, y, 0),
        new BoundaryVertex(x + width, y, 0),
        new BoundaryVertex(x + width, y + height, 0),
        new BoundaryVertex(x, y + height, 0),
    });

    private static RoomCandidate Room(string handle, Boundary boundary) => new(RoomId.From(handle), boundary);

    private static TagBlock Tag(double x, double y) =>
        new(null, new Point2D(x, y), "1", "105");

    [Fact]
    public void SingleTagInside_IsMatched()
    {
        var report = RoomBlockMatcher.Match(
            new[] { Room("A", Rectangle(0, 0, 500, 500)) },
            new[] { Tag(250, 250) });

        var match = Assert.Single(report.Rooms);
        Assert.Equal(MatchKind.Matched, match.Kind);
        Assert.NotNull(match.Tag);
        Assert.Equal(1, match.TagCount);
        Assert.Empty(report.OrphanTags);
    }

    [Fact]
    public void NoTagInside_IsNoTag()
    {
        var report = RoomBlockMatcher.Match(
            new[] { Room("A", Rectangle(0, 0, 500, 500)) },
            Array.Empty<TagBlock>());

        var match = Assert.Single(report.Rooms);
        Assert.Equal(MatchKind.NoTag, match.Kind);
        Assert.Null(match.Tag);
        Assert.Equal(0, match.TagCount);
    }

    [Fact]
    public void TwoTagsInside_IsMultiple_UsesFirst()
    {
        var report = RoomBlockMatcher.Match(
            new[] { Room("A", Rectangle(0, 0, 500, 500)) },
            new[] { Tag(100, 100), Tag(400, 400) });

        var match = Assert.Single(report.Rooms);
        Assert.Equal(MatchKind.MultipleTags, match.Kind);
        Assert.Equal(2, match.TagCount);
        Assert.NotNull(match.Tag);
    }

    [Fact]
    public void TagOutsideAllRooms_IsOrphan()
    {
        var report = RoomBlockMatcher.Match(
            new[] { Room("A", Rectangle(0, 0, 500, 500)) },
            new[] { Tag(5000, 5000) });

        Assert.Single(report.OrphanTags);
        var match = Assert.Single(report.Rooms);
        Assert.Equal(MatchKind.NoTag, match.Kind);
    }

    [Fact]
    public void TagsRoutedToCorrectRoom_AmongSeveral()
    {
        var report = RoomBlockMatcher.Match(
            new[]
            {
                Room("A", Rectangle(0, 0, 500, 500)),
                Room("B", Rectangle(600, 0, 500, 500)),
                Room("C", Rectangle(0, 600, 500, 500)),
            },
            new[] { Tag(250, 250), Tag(850, 250), Tag(250, 850) });

        Assert.All(report.Rooms, m => Assert.Equal(MatchKind.Matched, m.Kind));
        Assert.Empty(report.OrphanTags);
    }

    [Fact]
    public void TagOnSharedEdge_CountedOnce_FirstRoomWins()
    {
        var report = RoomBlockMatcher.Match(
            new[]
            {
                Room("A", Rectangle(0, 0, 500, 500)),
                Room("B", Rectangle(500, 0, 500, 500)),
            },
            new[] { Tag(500, 250) });

        Assert.Equal(MatchKind.Matched, report.Rooms[0].Kind);
        Assert.Equal(MatchKind.NoTag, report.Rooms[1].Kind);
        Assert.Empty(report.OrphanTags);
    }

    [Fact]
    public void NullInputs_Throw()
    {
        Assert.Throws<ArgumentNullException>(() => RoomBlockMatcher.Match(null!, Array.Empty<TagBlock>()));
        Assert.Throws<ArgumentNullException>(() => RoomBlockMatcher.Match(Array.Empty<RoomCandidate>(), null!));
    }
}
