using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;

namespace CadBridge.Revit.Adapters;

public sealed class SeparationLineStore
{
    public static readonly Guid SchemaId = new("1B4E2A6C-8D3F-4A5B-9C7E-2F1A3B5D7E9C");
    public const string FieldName = "OwnerSourceId";

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
        builder.SetSchemaName("CadBridgeSeparationLineOwner");
        builder.SetReadAccessLevel(AccessLevel.Public);
        builder.SetWriteAccessLevel(AccessLevel.Public);
        builder.AddSimpleField(FieldName, typeof(string));
        _schema = builder.Finish();
        return _schema;
    }

    public string? GetOwner(Element element)
    {
        ArgumentNullException.ThrowIfNull(element);
        var schema = GetSchema();
        var entity = element.GetEntity(schema);
        if (!entity.IsValid())
            return null;

        var field = schema.GetField(FieldName);
        return entity.Get<string>(field);
    }

    public void Mark(Element element, string ownerSourceId)
    {
        ArgumentNullException.ThrowIfNull(element);
        var schema = GetSchema();
        var field = schema.GetField(FieldName);

        var entity = element.GetEntity(schema);
        if (!entity.IsValid())
            entity = new Entity(schema);

        entity.Set(field, ownerSourceId);
        element.SetEntity(entity);
    }
}
