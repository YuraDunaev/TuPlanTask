using CadBridge.Core.Models;

namespace CadBridge.Core.Services.Analysis;

public static class BrokenRoomProjector
{
    public static IReadOnlyList<RoomReport> Project(RoomsDocument source, IEnumerable<string> existingIds)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(existingIds);

        var seen = new HashSet<string>(existingIds, StringComparer.Ordinal);
        var projected = new List<RoomReport>();

        foreach (var warning in source.Warnings)
        {
            if (warning.Code != WarningCode.OpenPolyline && warning.Code != WarningCode.CalculationFailed)
                continue;

            var id = warning.EntityId;
            if (string.IsNullOrWhiteSpace(id) || !seen.Add(id))
                continue;

            projected.Add(new RoomReport
            {
                Id = id,
                Number = string.Empty,
                Name = "(битый контур CAD)",
                RevitArea = 0,
                CadArea = null,
                CadContourValid = false,
            });
        }

        return projected;
    }
}
