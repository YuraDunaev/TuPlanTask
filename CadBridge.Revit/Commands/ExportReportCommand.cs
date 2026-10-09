using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace CadBridge.Revit.Commands;

[Transaction(TransactionMode.ReadOnly)]
public class ExportReportCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        message = "ExportReport not implemented yet";
        return Result.Failed;
    }
}
