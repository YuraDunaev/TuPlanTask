using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CadBridge.Revit.UI;

namespace CadBridge.Revit.Commands;

// TODO: IExternalCommand — открывает диалог; позже импорт целиком в TransactionGroup, потом счётчики и предупреждения
[Transaction(TransactionMode.Manual)]
public class ImportRoomsCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        try
        {
            new ImportRoomsDialog().ShowDialog();
            return Result.Succeeded;
        }
        catch (Exception ex)
        {
            message = ex.Message;
            return Result.Failed;
        }
    }
}

