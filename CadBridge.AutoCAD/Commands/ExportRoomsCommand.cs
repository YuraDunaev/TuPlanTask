using CadApplication = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using CadBridge.AutoCAD.Application;
using Autodesk.AutoCAD.Runtime;

namespace CadBridge.AutoCAD.Commands;

public class ExportRoomsCommand
{
    [CommandMethod("EXPORTROOMS")]
    public void Export()
    {
        var document = CadApplication.DocumentManager.MdiActiveDocument;
        if (document is null)
            return;

        var editor = document.Editor;
        try
        {
            var result = new ExportRoomsUseCase().Execute(document);

            var tagged = result.Rooms.Count(room => room.Tag is not null);
            editor.WriteMessage(
                $"\n[CadBridge] Read {result.Rooms.Count} rooms ({tagged} tagged). " +
                $"Warnings: {result.Warnings.Count}.");

            foreach (var warning in result.Warnings)
                editor.WriteMessage($"\n[CadBridge]   {warning}");
        }
        catch (System.Exception ex)
        {
            editor.WriteMessage($"\n[CadBridge] EXPORTROOMS failed: {ex.Message}");
        }
    }
}

