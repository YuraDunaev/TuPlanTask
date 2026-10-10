using AcDocument = Autodesk.AutoCAD.ApplicationServices.Document;
using CadBridge.AutoCAD.Drawing;
using Autodesk.AutoCAD.DatabaseServices;

namespace CadBridge.AutoCAD.Application;

public sealed class ClearMarkersUseCase
{
    private readonly RoomMarkerStore _markers = new();

    public int Execute(AcDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var database = document.Database;
        var removed = 0;

        using var tr = database.TransactionManager.StartTransaction();
        try
        {
            var space = (BlockTableRecord)tr.GetObject(
                SymbolUtilityServices.GetBlockModelSpaceId(database), OpenMode.ForWrite);

            var toErase = new List<ObjectId>();
            foreach (ObjectId id in space)
            {
                if (id.IsErased)
                    continue;
                if (tr.GetObject(id, OpenMode.ForRead) is not Entity entity)
                    continue;
                if (_markers.Read(entity) is not null)
                    toErase.Add(id);
            }

            foreach (ObjectId id in toErase)
            {
                if (tr.GetObject(id, OpenMode.ForWrite) is Entity entity)
                {
                    entity.Erase();
                    removed++;
                }
            }

            tr.Commit();
        }
        catch (System.Exception)
        {
            tr.Abort();
            throw;
        }

        return removed;
    }
}
