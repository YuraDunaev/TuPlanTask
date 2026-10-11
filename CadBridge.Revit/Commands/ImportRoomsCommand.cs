using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CadBridge.Core.Serialization;
using CadBridge.Revit.Application;
using CadBridge.Revit.UI;

namespace CadBridge.Revit.Commands;

[Transaction(TransactionMode.Manual)]
public class ImportRoomsCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        try
        {
            var document = commandData.Application.ActiveUIDocument.Document;

            var dialog = new ImportRoomsDialog(document);
            if (dialog.ShowDialog() != true)
                return Result.Cancelled;

            var viewModel = dialog.ViewModel;

            var level = viewModel.GetSelectedLevel();
            if (level is null)
            {
                message = "Select a level for the import.";
                return Result.Failed;
            }

            if (string.IsNullOrWhiteSpace(viewModel.FilePath) || !File.Exists(viewModel.FilePath))
            {
                message = "Provide an existing rooms.json file (Browse… button).";
                return Result.Failed;
            }

            var json = File.ReadAllText(viewModel.FilePath);
            var sourceDocument = RoomsJsonSerializer.Deserialize(json);
            if (sourceDocument is null)
            {
                message = "Failed to parse rooms.json.";
                return Result.Failed;
            }

            var useCase = new ImportRoomsUseCase(document);
            var result = useCase.Execute(sourceDocument, level.Id);

            ShowSummary(result);
            return Result.Succeeded;
        }
        catch (Exception ex)
        {
            message = ex.Message;
            return Result.Failed;
        }
    }

    private static void ShowSummary(ImportResult result)
    {
        var lines = new List<string>
        {
            $"Created: {result.Created}",
            $"Updated: {result.Updated}",
            $"Errors: {result.Errors.Count}",
        };

        if (result.Errors.Count > 0)
        {
            lines.Add(string.Empty);
            lines.Add("Errors:");
            foreach (var error in result.Errors.Take(10))
                lines.Add($"  • {error.Room?.Number} — {error.Reason}");
            if (result.Errors.Count > 10)
                lines.Add($"  … and {result.Errors.Count - 10} more");
        }

        if (result.Missing.Count > 0)
        {
            lines.Add(string.Empty);
            lines.Add($"Missing from the file (not deleted): {result.Missing.Count}");
            foreach (var missing in result.Missing.Take(10))
                lines.Add($"  • {missing.Number} — {missing.Name}");
            if (result.Missing.Count > 10)
                lines.Add($"  … and {result.Missing.Count - 10} more");
        }

        if (result.Warnings.Count > 0)
        {
            lines.Add(string.Empty);
            lines.Add($"Warnings from rooms.json: {result.Warnings.Count} (see warnings in the file).");
        }

        TaskDialog.Show("CadBridge — Import Rooms", string.Join(Environment.NewLine, lines));
    }
}
