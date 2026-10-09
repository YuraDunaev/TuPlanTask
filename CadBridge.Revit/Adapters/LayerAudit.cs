using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

namespace CadBridge.Revit.Adapters;

public sealed class LayerAudit
{
    private readonly Document _document;

    public LayerAudit(Document document)
    {
        _document = document;
    }

    public string CurrentLayer => "A-ROOM";

    public string CheckLayer => "A-ROOM-CHECK";

    public Dictionary<string, int> GetReport()
    {
        var report = new Dictionary<string, int>();

        var currentRooms = new FilteredElementCollector(_document)
            .OfCategory(BuiltInCategory.OST_Rooms)
            .WhereElementIsNotElementType()
            .Cast<Autodesk.Revit.DB.Architecture.Room>()
            .ToList();

        report[CurrentLayer] = currentRooms.Count;

        var seen = new HashSet<string>();
        var duplicates = currentRooms
            .Where(r => !seen.Add(r.Number ?? ""))
            .Count();

        report["Duplicates"] = duplicates;

        return report;
    }

    public bool Validate()
    {
        var rooms = new FilteredElementCollector(_document)
            .OfCategory(BuiltInCategory.OST_Rooms)
            .WhereElementIsNotElementType()
            .Cast<Autodesk.Revit.DB.Architecture.Room>()
            .ToList();

        return rooms.Count > 0;
    }
}
