using System.IO;
using CadApplication = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Autodesk.AutoCAD.Runtime;

namespace CadBridge.AutoCAD;

public class App : IExtensionApplication
{
    private static readonly string LogPath =
        System.IO.Path.Combine(System.IO.Path.GetTempPath(), "CadBridge_load.log");

    private static void Log(string message)
    {
        try
        {
            File.AppendAllText(LogPath,
                System.DateTime.Now.ToString("HH:mm:ss") + "  " + message + System.Environment.NewLine);
        }
        catch (System.Exception)
        {
        }
    }

    public void Initialize()
    {
        Log("Initialize() called. Bundle loaded OK.");
        try
        {
            CadApplication.Idle += OnIdle;
        }
        catch (System.Exception ex)
        {
            Log("Initialize Idle subscribe failed: " + ex);
        }
    }

    [CommandMethod("CADBRIDGERIBBON")]
    public void CreateRibbon()
    {
        var editor = CadApplication.DocumentManager.MdiActiveDocument?.Editor;
        Ribbon.CadBridgeRibbonBuilder.Build(editor);
    }

    private void OnIdle(object? sender, EventArgs e)
    {
        CadApplication.Idle -= OnIdle;
        Log("OnIdle fired. Building ribbon...");

        var editor = CadApplication.DocumentManager.MdiActiveDocument?.Editor;
        var ok = Ribbon.CadBridgeRibbonBuilder.Build(editor);
        Log("Build result: " + ok);
    }

    public void Terminate()
    {
        Log("Terminate() called.");
    }
}

