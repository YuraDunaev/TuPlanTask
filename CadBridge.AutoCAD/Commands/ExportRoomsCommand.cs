using CadApplication = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using CadBridge.AutoCAD.Application;
using Autodesk.AutoCAD.Runtime;

namespace CadBridge.AutoCAD.Commands;

public class ExportRoomsCommand
{
    [CommandMethod("EXPORTROOMS")]
    public void Export()
    {
        var editor = CadApplication.DocumentManager.MdiActiveDocument.Editor;
        try
        {
            var command = new ExportRoomsUseCase().Execute();
            editor.WriteMessage($"\n{command}");
        }
        catch (System.Exception ex)
        {
            editor.WriteMessage($"\n[CadBridge] EXPORTROOMS failed: {ex.Message}");
        }
    }
}
