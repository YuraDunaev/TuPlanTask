using Autodesk.Revit.UI;
using CadBridge.Revit.Ribbon;

namespace CadBridge.Revit;

// TODO: IExternalApplication — OnStartup строит вкладку ленты с двумя кнопками, OnShutdown чистит
public class App : IExternalApplication
{
    public Result OnStartup(UIControlledApplication application)
    {
        RibbonBuilder.Build(application);
        return Result.Succeeded;
    }

    public Result OnShutdown(UIControlledApplication application)
    {
        return Result.Succeeded;
    }
}
