using Autodesk.Revit.UI;

namespace CadBridge.Revit;

public class App : IExternalApplication
{
    public Result OnStartup(UIControlledApplication application)
    {
        return Result.Succeeded;
    }

    public Result OnShutdown(UIControlledApplication application)
    {
        return Result.Succeeded;
    }
}
