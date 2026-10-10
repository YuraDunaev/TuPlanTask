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
                message = "Выберите уровень для импорта.";
                return Result.Failed;
            }

            if (string.IsNullOrWhiteSpace(viewModel.FilePath) || !File.Exists(viewModel.FilePath))
            {
                message = "Укажите существующий файл rooms.json (кнопка Browse…).";
                return Result.Failed;
            }

            var json = File.ReadAllText(viewModel.FilePath);
            var sourceDocument = RoomsJsonSerializer.Deserialize(json);
            if (sourceDocument is null)
            {
                message = "Не удалось разобрать rooms.json.";
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
            $"Создано: {result.Created}",
            $"Обновлено: {result.Updated}",
            $"Ошибок: {result.Errors.Count}",
        };

        if (result.Errors.Count > 0)
        {
            lines.Add(string.Empty);
            lines.Add("Ошибки:");
            foreach (var error in result.Errors.Take(10))
                lines.Add($"  • {error.Room?.Number} — {error.Reason}");
            if (result.Errors.Count > 10)
                lines.Add($"  … и ещё {result.Errors.Count - 10}");
        }

        if (result.Missing.Count > 0)
        {
            lines.Add(string.Empty);
            lines.Add($"Отсутствуют в файле (не удалены): {result.Missing.Count}");
            foreach (var missing in result.Missing.Take(10))
                lines.Add($"  • {missing.Number} — {missing.Name}");
            if (result.Missing.Count > 10)
                lines.Add($"  … и ещё {result.Missing.Count - 10}");
        }

        if (result.Warnings.Count > 0)
        {
            lines.Add(string.Empty);
            lines.Add($"Предупреждений из rooms.json: {result.Warnings.Count} (см. warnings в файле).");
        }

        TaskDialog.Show("CadBridge — Import Rooms", string.Join(Environment.NewLine, lines));
    }
}
