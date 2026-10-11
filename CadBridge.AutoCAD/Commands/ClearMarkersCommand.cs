using CadApplication = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using CadBridge.AutoCAD.Application;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.EditorInput;

namespace CadBridge.AutoCAD.Commands;

public class ClearMarkersCommand
{
    [CommandMethod("CLEARMARKS")]
    public void Clear()
    {
        var document = CadApplication.DocumentManager.MdiActiveDocument;
        if (document is null)
            return;

        var editor = document.Editor;
        try
        {
            var removed = new ClearMarkersUseCase().Execute(document);

            editor.WriteMessage(string.Format(
                "\n[CadBridge] CLEARMARKS: markers removed: {0}.", removed));
        }
        catch (System.Exception ex)
        {
            editor.WriteMessage(string.Format("\n[CadBridge] CLEARMARKS failed: {0}", ex.Message));
        }
    }
}
