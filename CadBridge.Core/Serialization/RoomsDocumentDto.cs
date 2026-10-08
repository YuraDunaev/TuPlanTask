using System.Text.Json.Serialization;

namespace CadBridge.Core.Serialization;

public sealed class PointDto
{
    public double X { get; set; }

    public double Y { get; set; }
}

/// <summary>Вершина границы в «плоском» виде <c>{ "x", "y", "bulge" }</c> (см).</summary>
public sealed class BoundaryVertexDto
{
    public double X { get; set; }

    public double Y { get; set; }

    public double Bulge { get; set; }
}

public sealed class RoomDto
{
    public string Id { get; set; } = string.Empty;

    public string Number { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public double Area { get; set; }

    public double Perimeter { get; set; }

    public PointDto LabelPoint { get; set; } = new();

    public List<BoundaryVertexDto> Boundary { get; set; } = new();
}

public sealed class WarningDto
{
    public string Code { get; set; } = string.Empty;

    public string EntityId { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;
}

public sealed class SourceInfoDto
{
    public string Application { get; set; } = string.Empty;

    public string File { get; set; } = string.Empty;

    public string ExportedAt { get; set; } = string.Empty;

    public string Units { get; set; } = string.Empty;
}

public sealed class RoomsDocumentDto
{
    public string SchemaVersion { get; set; } = string.Empty;

    public SourceInfoDto Source { get; set; } = new();

    public List<RoomDto> Rooms { get; set; } = new();

    [JsonPropertyName("warnings")]
    public List<WarningDto> Warnings { get; set; } = new();
}
