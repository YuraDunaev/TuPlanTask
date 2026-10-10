using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using CadBridge.Core.Models;
using CadBridge.Revit.Adapters;

namespace CadBridge.Revit.Application;

public sealed class ImportRoomsUseCase
{
    private readonly Document _document;

    public ImportRoomsUseCase(Document document)
    {
        _document = document ?? throw new ArgumentNullException(nameof(document));
    }

    public ImportResult Execute(RoomsDocument sourceDocument, ElementId levelId)
    {
        ArgumentNullException.ThrowIfNull(sourceDocument);

        var level = _document.GetElement(levelId) as Level
                    ?? throw new ArgumentException($"Уровень с id {levelId} не найден.", nameof(levelId));

        var result = new ImportResult();
        foreach (var warning in sourceDocument.Warnings)
            result.Warnings.Add(warning);

        var reader = new RevitRoomReader(_document);

        using var group = new TransactionGroup(_document, "CadBridge.ImportRooms");
        group.Start();
        try
        {
            var writer = new RevitRoomWriter(_document, level);

            foreach (var room in sourceDocument.Rooms)
            {
                try
                {
                    var existing = reader.GetExisting(room.Id);
                    if (existing is null)
                    {
                        writer.Create(room);
                        result.Created++;
                    }
                    else
                    {
                        writer.Update(existing.Element, room);
                        result.Updated++;
                    }
                }
                catch (Exception ex)
                {
                    result.Errors.Add(new ImportError { Room = room, Reason = ex.Message });
                }
            }

            DetectMissing(sourceDocument, result);

            group.Assimilate();
        }
        catch (Exception ex)
        {
            group.RollBack();
            throw new InvalidOperationException($"Импорт отменён: {ex.Message}", ex);
        }

        return result;
    }

    private void DetectMissing(RoomsDocument sourceDocument, ImportResult result)
    {
        var fileIds = new HashSet<string>(sourceDocument.Rooms.Select(r => r.Id.Value));

        var all = new FilteredElementCollector(_document)
            .OfCategory(BuiltInCategory.OST_Rooms)
            .WhereElementIsNotElementType()
            .Cast<Autodesk.Revit.DB.Architecture.Room>()
            .ToList();

        var store = new SourceIdStore();
        foreach (var room in all)
        {
            var stored = store.GetSourceId(room);
            if (stored is null || fileIds.Contains(stored))
                continue;

            result.Missing.Add(new MissingRoom
            {
                Id = stored,
                Number = room.Number,
                Name = room.Name,
            });
        }
    }
}

public sealed class ImportError
{
    public Room? Room { get; init; }

    public string Reason { get; init; } = string.Empty;
}

public sealed class MissingRoom
{
    public string Id { get; init; } = string.Empty;

    public string Number { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;
}

public sealed class ImportResult
{
    public int Created { get; set; }

    public int Updated { get; set; }

    public List<ImportError> Errors { get; } = new();

    public List<Warning> Warnings { get; } = new();

    public List<MissingRoom> Missing { get; } = new();
}

