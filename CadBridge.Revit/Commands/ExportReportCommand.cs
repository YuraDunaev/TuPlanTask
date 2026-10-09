using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;

namespace CadBridge.Revit.Commands;

// TODO: IExternalCommand, [Transaction(TransactionMode.ReadOnly)] —
// собираем комнаты + уровень, пишем rooms_report.json через IRoomsSerializer
[Transaction(TransactionMode.ReadOnly)]
public class ExportReportCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        var doc = commandData.Application.ActiveUIDocument.Document;
        var rooms = new FilteredElementCollector(doc)
            .OfCategory(BuiltInCategory.OST_Rooms)
            .WhereElementIsNotElementType()
            .Cast<Room>().ToList();
        throw new System.NotImplementedException();
    }
}
