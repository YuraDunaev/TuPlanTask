namespace CadBridge.Core.Models;

public sealed class RoomsDocument
{
    public const string CurrentSchemaVersion = "1.0";

    public string SchemaVersion { get; }

    public SourceInfo Source { get; }

    public IReadOnlyList<Room> Rooms { get; }

    public IReadOnlyList<Warning> Warnings { get; }

    public RoomsDocument(
        SourceInfo source,
        IEnumerable<Room> rooms,
        IEnumerable<Warning>? warnings = null,
        string? schemaVersion = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(rooms);

        SchemaVersion = string.IsNullOrWhiteSpace(schemaVersion)
            ? CurrentSchemaVersion
            : schemaVersion;
        Source = source;
        Rooms = rooms.ToArray();
        Warnings = warnings?.ToArray() ?? Array.Empty<Warning>();
    }
}

