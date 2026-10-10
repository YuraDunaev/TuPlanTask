using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;

namespace CadBridge.AutoCAD.Drawing;

public sealed class RoomMarkerStore
{
    public const string RegAppName = "CADBRIDGE_CHECK";
    public const string LayerName = "A-ROOM-CHECK";

    public const string KindMarker = "marker";
    public const string KindText = "text";

    public void Mark(Entity entity, string sourceId, string kind)
    {
        ArgumentNullException.ThrowIfNull(entity);
        entity.XData = new ResultBuffer(
            new TypedValue((int)DxfCode.ExtendedDataRegAppName, RegAppName),
            new TypedValue((int)DxfCode.ExtendedDataAsciiString, sourceId),
            new TypedValue((int)DxfCode.ExtendedDataAsciiString, kind));
    }

    public (string SourceId, string Kind)? Read(Entity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var buffer = entity.GetXDataForApplication(RegAppName);
        if (buffer is null)
            return null;

        var values = buffer
            .OfType<TypedValue>()
            .Where(v => v.TypeCode == (short)DxfCode.ExtendedDataAsciiString)
            .Select(v => v.Value?.ToString())
            .ToList();

        if (values.Count < 2 || string.IsNullOrEmpty(values[0]) || string.IsNullOrEmpty(values[1]))
            return null;

        return (values[0]!, values[1]!);
    }
}