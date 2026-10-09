using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.ExtensibleStorage;

namespace CadBridge.Revit.Adapters;

public sealed class SourceIdStore
{
    public static readonly Guid SchemaId = new("5F7C4B3E-2D1A-4F8E-9A6C-7B5D3E1F2A4C");
    public const string FieldName = "SourceId";

    private static Schema? _schema;

    private static Schema GetSchema()
    {
        if (_schema is not null)
            return _schema;

        var existing = Schema.Lookup(SchemaId);
        if (existing is not null)
        {
            _schema = existing;
            return _schema;
        }

        var builder = new SchemaBuilder(SchemaId);
        builder.SetSchemaName("CadBridgeSourceId");
        builder.SetReadAccessLevel(AccessLevel.Public);
        builder.SetWriteAccessLevel(AccessLevel.Public);
        builder.AddSimpleField(FieldName, typeof(string));
        _schema = builder.Finish();
        return _schema;
    }

    public string? GetSourceId(Room room)
    {
        ArgumentNullException.ThrowIfNull(room);
        var schema = GetSchema();
        var entity = room.GetEntity(schema);
        if (!entity.IsValid())
            return null;

        var field = schema.GetField(FieldName);
        return entity.Get<string>(field);
    }

    public void SetSourceId(Room room, string sourceId)
    {
        ArgumentNullException.ThrowIfNull(room);
        var schema = GetSchema();
        var field = schema.GetField(FieldName);

        var entity = room.GetEntity(schema);
        if (!entity.IsValid())
            entity = new Entity(schema);

        entity.Set(field, sourceId);
        room.SetEntity(entity);
    }

    public static Guid ToGuid(string sourceId)
    {
        if (Guid.TryParse(sourceId, out var parsed))
            return parsed;

        using var md5 = System.Security.Cryptography.MD5.Create();
        var hash = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(sourceId));
        return new Guid(hash);
    }
}
