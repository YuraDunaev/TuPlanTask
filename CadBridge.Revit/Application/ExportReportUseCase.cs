using System.Linq;
using Autodesk.Revit.DB;
using CadBridge.Core.Models;
using CadBridge.Revit.Adapters;

namespace CadBridge.Revit.Application;

public sealed class ExportReportUseCase
{
    private readonly Document _document;

    public ExportReportUseCase(Document document)
    {
        _document = document ?? throw new ArgumentNullException(nameof(document));
    }

    public RoomsReportDocument Execute()
    {
        var report = new RoomsReportDocument();
        var store = new SourceIdStore();

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

            var revitAreaM2 = AreaToM2(room.Area);

            report.Rooms.Add(new RoomReport
            {
                Id = sourceId,
                Number = room.Number,
                Name = room.Name,
                RevitArea = revitAreaM2,
                CadArea = store.GetCadArea(room),
            });
        }

        return report;
    }

    private static double AreaToM2(double areaSqFt) =>
        UnitUtils.ConvertFromInternalUnits(areaSqFt, UnitTypeId.SquareMeters);
}
