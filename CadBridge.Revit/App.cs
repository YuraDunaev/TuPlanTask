using Autodesk.Revit.UI;
using CadBridge.Revit.Ribbon;

namespace CadBridge.Revit;

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
