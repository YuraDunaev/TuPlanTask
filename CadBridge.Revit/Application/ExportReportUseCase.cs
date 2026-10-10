using System.IO;
using System.Linq;
using Autodesk.Revit.DB;
using CadBridge.Core.Models;
using CadBridge.Core.Serialization;
using CadBridge.Core.Services.Analysis;
using CadBridge.Revit.Adapters;

namespace CadBridge.Revit.Application;

public sealed class ExportReportUseCase
{
    private readonly Document _document;

    public ExportReportUseCase(Document document)
    {
        _document = document ?? throw new ArgumentNullException(nameof(document));
    }

    public RoomsReportDocument Execute(string? roomsJsonPath = null)
    {
        var report = new RoomsReportDocument();
        var store = new SourceIdStore();
        var ids = new List<string>();

        var rooms = new FilteredElementCollector(_document)
            .OfCategory(BuiltInCategory.OST_Rooms)
            .WhereElementIsNotElementType()
            .Cast<Autodesk.Revit.DB.Architecture.Room>()
            .ToList();

        foreach (var room in rooms)
        {
            var sourceId = store.GetSourceId(room);
            if (sourceId is null)
                continue;

            ids.Add(sourceId);
            var revitAreaM2 = AreaToM2(room.Area);

            report.Rooms.Add(new RoomReport
            {
                Id = sourceId,
                Number = room.Number,
                Name = room.Name,
                RevitArea = revitAreaM2,
                CadArea = store.GetCadArea(room),
                CadContourValid = true,
            });
        }

        if (!string.IsNullOrWhiteSpace(roomsJsonPath) && File.Exists(roomsJsonPath))
            AppendBrokenRooms(report, roomsJsonPath, ids);

        return report;
    }

    private static void AppendBrokenRooms(RoomsReportDocument report, string roomsJsonPath, List<string> ids)
    {
        RoomsDocument? source;
        try
        {
            source = RoomsJsonSerializer.Deserialize(File.ReadAllText(roomsJsonPath));
        }
        catch (IOException)
        {
            return;
        }
        catch (System.Text.Json.JsonException)
        {
            return;
        }

        if (source is null)
            return;

        foreach (var broken in BrokenRoomProjector.Project(source, ids))
            report.Rooms.Add(broken);
    }

    private static double AreaToM2(double areaSqFt) =>
        UnitUtils.ConvertFromInternalUnits(areaSqFt, UnitTypeId.SquareMeters);
}
