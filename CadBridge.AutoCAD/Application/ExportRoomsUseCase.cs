using AcDocument = Autodesk.AutoCAD.ApplicationServices.Document;
using CadBridge.AutoCAD.Reading;
using CadBridge.Core.Geometry;
using CadBridge.Core.Models;
using CadBridge.Core.Services.Geometry;
using CadBridge.Core.Serialization;
using Autodesk.AutoCAD.EditorInput;
using Microsoft.Win32;
using System.IO;
using System.Text;

namespace CadBridge.AutoCAD.Application;

public class ExportRoomsUseCase
{
    public RoomReadResult Execute(AcDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        return new DwgRoomReader().Read(document);
    }

    public ExportResult Export(AcDocument document, out string jsonPath)
    {
        ArgumentNullException.ThrowIfNull(document);

        var readResult = Execute(document);

        var rooms = new List<Room>(readResult.Rooms.Count);
        foreach (var draft in readResult.Rooms)
        {
            var boundary = draft.Boundary.Boundary;

            var area = AreaCalculator.Calculate(boundary);
            var perimeter = AreaCalculator.Perimeter(boundary);
            var labelPoint = draft.Tag is { } tag
                ? tag.Position
                : LabelPointResolver.TryResolve(boundary, out var resolved)
                    ? resolved
                    : new Point2D(0, 0);

            var id = RoomId.From(draft.Boundary.Handle);
            rooms.Add(new Room(id, draft.Tag?.Number, draft.Tag?.Name, area, perimeter, labelPoint, boundary));
        }

        var source = new SourceInfo(
            Application: "CadBridge.AutoCAD",
            File: Path.GetFileName(document.Name),
            ExportedAt: DateTimeOffset.UtcNow,
            Units: "cm");

        var roomsDocument = new RoomsDocument(source, rooms, readResult.Warnings);
        var json = RoomsJsonSerializer.Serialize(roomsDocument);

        jsonPath = ShowSaveDialog(document.Editor);
        if (!string.IsNullOrEmpty(jsonPath))
        {
            File.WriteAllText(jsonPath, json, Encoding.UTF8);
        }

        return new ExportResult(roomsDocument, readResult, jsonPath);
    }

    private static string ShowSaveDialog(Editor editor)
    {
       
        var dialog = new SaveFileDialog
        {
            Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
            Title = "Save rooms.json",
            FileName = "rooms.json",
            DefaultExt = "json",
            AddExtension = true,
        };

        
        return dialog.ShowDialog() == true
            ? dialog.FileName
            : string.Empty;
    }
}

public readonly record struct ExportResult(
    RoomsDocument Document,
    RoomReadResult ReadResult,
    string JsonPath);
