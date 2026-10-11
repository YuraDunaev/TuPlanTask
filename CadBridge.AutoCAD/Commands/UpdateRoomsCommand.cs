using CadApplication = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using CadBridge.AutoCAD.Application;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.ApplicationServices;
using Microsoft.Win32;

namespace CadBridge.AutoCAD.Commands;

public class UpdateRoomsCommand
{
    [CommandMethod("UPDATEROOMS")]
    public void Update()
    {
        var document = CadApplication.DocumentManager.MdiActiveDocument;
        if (document is null)
            return;

        var editor = document.Editor;
        try
        {
            var reportPath = ShowOpenDialog();
            if (string.IsNullOrWhiteSpace(reportPath))
            {
                editor.WriteMessage("\n[CadBridge] UPDATEROOMS cancelled: no file selected.");
                return;
            }

            var result = new UpdateRoomsUseCase().Execute(document, reportPath);

            editor.WriteMessage(string.Format(
                "\n[CadBridge] UPDATEROOMS: marked {0}, discrepancies >1%: {1}, not found in drawing: {2}.",
                result.Marked, result.Mismatched, result.Missing));
        }
        catch (System.Exception ex)
        {
            editor.WriteMessage(string.Format("\n[CadBridge] UPDATEROOMS failed: {0}", ex.Message));
        }
    }

    private static string ShowOpenDialog()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
            Title = "Select rooms_report.json",
            FileName = "rooms_report.json",
            DefaultExt = "json",
            CheckFileExists = true,
        };

        return dialog.ShowDialog() == true ? dialog.FileName : string.Empty;
    }
}