using CadApplication = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using CadBridge.AutoCAD.Application;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.EditorInput;

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
            var useCase = new ExportRoomsUseCase();
            var result = useCase.Export(document, out var jsonPath);

            editor.WriteMessage("\n[CadBridge] All set. File saved: rooms.json.");

            editor.WriteMessage(
                string.Format(
                    "\n[CadBridge] Exported {0} rooms. Warnings: {1} (see rooms.json → \"warnings\").",
                    result.Document.Rooms.Count,
                    result.ReadResult.Warnings.Count));

            foreach (var warning in result.ReadResult.Warnings)
                editor.WriteMessage(string.Format("\n[CadBridge]   {0}", warning));

            if (!string.IsNullOrEmpty(jsonPath))
                editor.WriteMessage(string.Format("\n[CadBridge]   rooms.json written to: {0}", jsonPath));
        }
        catch (System.Exception ex)
        {
            editor.WriteMessage(string.Format("\n[CadBridge] EXPORTROOMS failed: {0}", ex.Message));
        }
    }
}
