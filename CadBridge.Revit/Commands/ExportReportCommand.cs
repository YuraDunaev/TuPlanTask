using System.IO;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CadBridge.Core.Serialization;
using CadBridge.Revit.Application;

namespace CadBridge.Revit.Commands;

[Transaction(TransactionMode.ReadOnly)]
public class ExportReportCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        try
        {
            var document = commandData.Application.ActiveUIDocument.Document;

            var roomsJsonPath = AskRoomsJsonPath();

            var useCase = new ExportReportUseCase(document);
            var report = useCase.Execute(roomsJsonPath);

            if (report.Rooms.Count == 0)
            {
                TaskDialog.Show("CadBridge — Export Report", "No imported rooms found. Run Import Rooms first.");
                return Result.Cancelled;
            }

            var path = AskSavePath();
            if (string.IsNullOrWhiteSpace(path))
                return Result.Cancelled;

            File.WriteAllText(path, RoomsReportJsonSerializer.Serialize(report));

            TaskDialog.Show("CadBridge — Export Report",
                $"Report rooms_report.json written ({report.Rooms.Count} rooms).\n{path}");
            return Result.Succeeded;
        }
        catch (IOException ex)
        {
            message = $"Failed to write file: {ex.Message}";
            return Result.Failed;
        }
        catch (Exception ex)
        {
            message = ex.Message;
            return Result.Failed;
        }
    }

    private static string? AskRoomsJsonPath()
    {
        var answer = TaskDialog.Show("CadBridge — Export Report",
            "Add rooms with broken contours from rooms.json to the report?",
            TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No);
        if (answer != TaskDialogResult.Yes)
            return null;

        using var dialog = new FileOpenDialog("rooms.json|*.json");
        dialog.Title = "Select rooms.json (can be cancelled)";
        if (dialog.Show() != ItemSelectionDialogResult.Confirmed)
            return null;

        return ModelPathUtils.ConvertModelPathToUserVisiblePath(dialog.GetSelectedModelPath());
    }

    private static string? AskSavePath()
    {
        using var dialog = new FileSaveDialog("rooms_report.json|*.json");
        dialog.Title = "Save rooms_report.json";
        dialog.InitialFileName = "rooms_report.json";
        if (dialog.Show() != ItemSelectionDialogResult.Confirmed)
            return null;

        var modelPath = dialog.GetSelectedModelPath();
        return ModelPathUtils.ConvertModelPathToUserVisiblePath(modelPath);
    }
}
