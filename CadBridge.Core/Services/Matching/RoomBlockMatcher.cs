using CadBridge.Core.Geometry;
using CadBridge.Core.Models;

namespace CadBridge.Core.Services.Matching;

public enum MatchKind
{
    Matched,

    NoTag,

    MultipleTags,
}

public sealed record TagBlock(RoomId? Id, Point2D Position, string Number, string Name);

public sealed record RoomCandidate(RoomId Id, Boundary Boundary);

public sealed record RoomMatch(
    RoomId Id,
    Boundary Boundary,
    MatchKind Kind,
    TagBlock? Tag,
    int TagCount);

public sealed record MatchingReport(
    IReadOnlyList<RoomMatch> Rooms,
    IReadOnlyList<TagBlock> OrphanTags);


public static class RoomBlockMatcher
{
    public static MatchingReport Match(
        IEnumerable<RoomCandidate> rooms,
        IEnumerable<TagBlock> tags)
    {
        ArgumentNullException.ThrowIfNull(rooms);
        ArgumentNullException.ThrowIfNull(tags);

        var roomList = rooms.ToArray();
        var tagList = tags.ToArray();

        var assigned = new List<TagBlock>[roomList.Length];
        for (var i = 0; i < assigned.Length; i++)
            assigned[i] = new List<TagBlock>();

        var orphans = new List<TagBlock>();

        foreach (var tag in tagList)
        {
            var roomIndex = IndexOfContainingRoom(roomList, tag.Position);
            if (roomIndex < 0)
            {
                orphans.Add(tag);
                continue;
            }

            assigned[roomIndex].Add(tag);
        }

        var matches = new List<RoomMatch>(roomList.Length);
        for (var i = 0; i < roomList.Length; i++)
        {
            var room = roomList[i];
            var roomTags = assigned[i];
            var kind = roomTags.Count switch
            {
                0 => MatchKind.NoTag,
                1 => MatchKind.Matched,
                _ => MatchKind.MultipleTags,
            };
            matches.Add(new RoomMatch(room.Id, room.Boundary, kind, roomTags.Count > 0 ? roomTags[0] : null, roomTags.Count));
        }

        return new MatchingReport(matches, orphans);
    }

    private static int IndexOfContainingRoom(RoomCandidate[] roomList, Point2D position)
    {
        for (var i = 0; i < roomList.Length; i++)
        {
            if (PointInPolygon.IsInside(position, roomList[i].Boundary))
                return i;
        }

        return -1;
    }
}
