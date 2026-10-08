using AcDocument = Autodesk.AutoCAD.ApplicationServices.Document;
using CadBridge.AutoCAD.Reading;

namespace CadBridge.AutoCAD.Application;

public class ExportRoomsUseCase
{
    public RoomReadResult Execute(AcDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        return new DwgRoomReader().Read(document);
    }
}

