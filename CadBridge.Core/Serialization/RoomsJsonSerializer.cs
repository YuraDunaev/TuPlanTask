using System.Text.Json;
using System.Text.Json.Serialization;
using CadBridge.Core.Models;

namespace CadBridge.Core.Serialization;

public static class RoomsJsonSerializer
{
    public static readonly JsonSerializerOptions Options = CreateOptions();

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        };
        return options;
    }

    public static string Serialize(RoomsDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        return JsonSerializer.Serialize(ToDto(document), Options);
    }

    public static string Serialize(RoomsDocumentDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        return JsonSerializer.Serialize(dto, Options);
    }

    public static RoomsDocument? Deserialize(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        var dto = JsonSerializer.Deserialize<RoomsDocumentDto>(json, Options);
        return dto is null ? null : ToModel(dto);
    }

    // ---- домен → DTO ----

    public static RoomsDocumentDto ToDto(RoomsDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var dto = new RoomsDocumentDto
        {
            SchemaVersion = document.SchemaVersion,
            Source = new SourceInfoDto
            {
                Application = document.Source.Application,
                File = document.Source.File,
                ExportedAt = document.Source.ExportedAt.UtcDateTime.ToString("o"),
                Units = document.Source.Units,
            },
        };

        foreach (var room in document.Rooms)
        {
            var roomDto = new RoomDto
            {
                Id = room.Id.Value,
                Number = room.Number,
                Name = room.Name,
                Area = room.Area,
                Perimeter = room.Perimeter,
                LabelPoint = new PointDto { X = room.LabelPoint.X, Y = room.LabelPoint.Y },
            };
            foreach (var vertex in room.Boundary.Vertices)
                roomDto.Boundary.Add(new BoundaryVertexDto { X = vertex.Point.X, Y = vertex.Point.Y, Bulge = vertex.Bulge });

            dto.Rooms.Add(roomDto);
        }

        foreach (var warning in document.Warnings)
        {
            dto.Warnings.Add(new WarningDto
            {
                Code = WarningCodeMapper.ToWire(warning.Code),
                EntityId = warning.EntityId,
                Message = warning.Message,
            });
        }

        return dto;
    }

    // ---- DTO → домен ----

    public static RoomsDocument ToModel(RoomsDocumentDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var rooms = new List<Room>(dto.Rooms.Count);
        foreach (var roomDto in dto.Rooms)
            rooms.Add(ToModel(roomDto));

        var warnings = new List<Warning>(dto.Warnings.Count);
        foreach (var warningDto in dto.Warnings)
        {
            warnings.Add(new Warning(
                WarningCodeMapper.FromWire(warningDto.Code),
                warningDto.EntityId,
                string.IsNullOrWhiteSpace(warningDto.Message) ? "(no message)" : warningDto.Message));
        }

        var source = new SourceInfo(
            dto.Source?.Application ?? string.Empty,
            dto.Source?.File ?? string.Empty,
            ParseExportedAt(dto.Source?.ExportedAt),
            dto.Source?.Units ?? string.Empty);

        return new RoomsDocument(source, rooms, warnings, dto.SchemaVersion);
    }

    private static Room ToModel(RoomDto dto)
    {
        var vertices = dto.Boundary.Select(v => new BoundaryVertex(v.X, v.Y, v.Bulge));
        var boundary = new Boundary(vertices);
        var label = new Point2D(dto.LabelPoint?.X ?? 0, dto.LabelPoint?.Y ?? 0);
        var id = RoomId.From(string.IsNullOrWhiteSpace(dto.Id) ? Guid.NewGuid().ToString("N") : dto.Id);

        return new Room(id, dto.Number, dto.Name, dto.Area, dto.Perimeter, label, boundary);
    }

    private static DateTimeOffset ParseExportedAt(string? value) =>
        DateTimeOffset.TryParse(value, out var parsed) ? parsed : DateTimeOffset.UnixEpoch;
}
