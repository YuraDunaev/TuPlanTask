namespace CadBridge.Core.Models;

public sealed class RoomReport
{
    public string Id { get; init; } = string.Empty;

    public string Number { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public double RevitArea { get; init; }

    public double? CadArea { get; init; }
}

public sealed class RoomsReportDocument
{
    public string SchemaVersion { get; init; } = "1.0";

    public List<RoomReport> Rooms { get; init; } = new();
}
