using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using CadBridge.Core.Models;

namespace CadBridge.Revit.Adapters;

public sealed class RevitRoomEntity
{
    public Autodesk.Revit.DB.Architecture.Room Element { get; init; } = null!;
    public Autodesk.Revit.DB.Level? Level { get; init; }
}

public sealed class RevitRoomReader
{
    private readonly Document _document;
    private readonly SourceIdStore _sourceIdStore;

    public RevitRoomReader(Document document)
    {
        _document = document;
        _sourceIdStore = new SourceIdStore();
    }

    public RevitRoomEntity? GetExisting(RoomId sourceId)
    {
        ArgumentNullException.ThrowIfNull(sourceId);

        var idString = sourceId.Value;

        var matching = new FilteredElementCollector(_document)
            .OfCategory(BuiltInCategory.OST_Rooms)
            .WhereElementIsNotElementType()
            .Cast<Autodesk.Revit.DB.Architecture.Room>()
            .Where(r =>
            {
                var stored = _sourceIdStore.GetSourceId(r);
                return stored != null && stored.Equals(idString, StringComparison.Ordinal);
            })
            .ToList();

        if (matching.Count > 0)
            return new RevitRoomEntity { Element = matching[0], Level = matching[0].Level };

        var byNumber = new FilteredElementCollector(_document)
            .OfCategory(BuiltInCategory.OST_Rooms)
            .WhereElementIsNotElementType()
            .Cast<Autodesk.Revit.DB.Architecture.Room>()
            .Where(r => r.Number == idString)
            .ToList();

        if (byNumber.Count > 0)
            return new RevitRoomEntity { Element = byNumber[0], Level = byNumber[0].Level };

        return null;
    }

    public IReadOnlyList<RevitRoomEntity> GetAll()
    {
        var result = new List<RevitRoomEntity>();
        var rooms = new FilteredElementCollector(_document)
            .OfCategory(BuiltInCategory.OST_Rooms)
            .WhereElementIsNotElementType()
            .Cast<Autodesk.Revit.DB.Architecture.Room>()
            .ToList();

        foreach (var room in rooms)
        {
            var level = new FilteredElementCollector(_document)
                .OfClass(typeof(Autodesk.Revit.DB.Level))
                .Cast<Autodesk.Revit.DB.Level>()
                .FirstOrDefault(l => l.Equals(room.Level));

            result.Add(new RevitRoomEntity { Element = room, Level = level });
        }

        return result;
    }
}
