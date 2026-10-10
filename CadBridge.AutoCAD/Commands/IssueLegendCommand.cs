using CadApplication = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using CadBridge.AutoCAD.Drawing;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;

namespace CadBridge.AutoCAD.Commands;

public class IssueLegendCommand
{
    [CommandMethod("CADBRIDGELEGEND")]
    public void Legend()
    {
        var document = CadApplication.DocumentManager.MdiActiveDocument;
        if (document is null)
            return;

        var editor = document.Editor;
        try
        {
            IReadOnlyList<IssueLegendRow> rows;
            using (var tr = document.Database.TransactionManager.StartTransaction())
            {
                var space = (BlockTableRecord)tr.GetObject(
                    SymbolUtilityServices.GetBlockModelSpaceId(document.Database), OpenMode.ForRead);

                rows = new DwgIssueReporter(new RoomMarkerStore()).ReadLegend(tr, space);
                tr.Commit();
            }

            var dialog = new CadBridge.AutoCAD.UI.IssueLegendDialog(rows);
            dialog.ShowDialog();
        }
        catch (System.Exception ex)
        {
            editor.WriteMessage(string.Format("\n[CadBridge] CADBRIDGELEGEND failed: {0}", ex.Message));
        }
    }
}
