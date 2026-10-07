using CadApplication = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Autodesk.AutoCAD.Runtime;

namespace CadBridge.AutoCAD;

public class App : IExtensionApplication
{
    public void Initialize()
    {
        CadApplication.DocumentManager.MdiActiveDocument?
            .Editor.WriteMessage("\n[CadBridge] loaded. Type EXPORTROOMS.");
    }

    public void Terminate()
    {
    }
}
