using Autodesk.AutoCAD.EditorInput;
using Autodesk.Windows;

namespace CadBridge.AutoCAD.Ribbon;

internal static class CadBridgeRibbonBuilder
{
    private const string TabId = "CadBridgeTab";
    private const string TabTitle = "CAD Bridge";
    private const string PanelTitle = "Rooms";

    public static bool Build(Editor? editor)
    {
        try
        {
            var ribbon = ComponentManager.Ribbon;
            if (ribbon is null)
            {
                editor?.WriteMessage("\n[CadBridge] ribbon not ready (Ribbon is null). Try command CADBRIDGERIBBON after opening a drawing.");
                return false;
            }

            if (ribbon.FindTab(TabId) is not null)
            {
                editor?.WriteMessage("\n[CadBridge] ribbon tab 'CAD Bridge' already exists.");
                return true;
            }

            var tab = new RibbonTab { Title = TabTitle, Id = TabId };

            var panelSource = new RibbonPanelSource { Title = PanelTitle };
            var panel = new RibbonPanel { Source = panelSource };
            tab.Panels.Add(panel);

            panelSource.Items.Add(CreateButton(
                "exportRooms",
                "Export Rooms",
                "Читает RZ_AREA из DWG и пишет rooms.json",
                "EXPORTROOMS",
                "import-rooms-32.png",
                "import-rooms-16.png"));

            panelSource.Items.Add(new RibbonSeparator());

            panelSource.Items.Add(CreateButton(
                "updateRooms",
                "Update Rooms",
                "Размечает расхождения >1% по rooms_report.json",
                "UPDATEROOMS",
                "export-report-32.png",
                "export-report-16.png"));

            panelSource.Items.Add(new RibbonSeparator());

            panelSource.Items.Add(CreateButton(
                "clearMarks",
                "Clear Marks",
                "Удаляет все метки CadBridge со слоёв A-ROOM-CHECK и A-ROOM-ISSUE",
                "CLEARMARKS",
                "clear-marks-32.png",
                "clear-marks-16.png"));

            ribbon.Tabs.Add(tab);
            tab.IsActive = true;
            editor?.WriteMessage("\n[CadBridge] ribbon tab 'CAD Bridge' created.");
            return true;
        }
        catch (System.Exception ex)
        {
            editor?.WriteMessage(string.Format("\n[CadBridge] ribbon build failed: {0}", ex.Message));
            LogException(ex);
            return false;
        }
    }

    private static void LogException(System.Exception ex)
    {
        try
        {
            var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "CadBridge_load.log");
            System.IO.File.AppendAllText(path,
                System.DateTime.Now.ToString("HH:mm:ss") + "  BUILD EXCEPTION: " + ex + System.Environment.NewLine);
        }
        catch (System.Exception)
        {
        }
    }

    private static RibbonButton CreateButton(
        string id,
        string text,
        string toolTip,
        string command,
        string largeIcon,
        string smallIcon)
    {
        var button = new RibbonButton
        {
            Id = id,
            Text = text,
            ShowText = true,
            Size = RibbonItemSize.Large,
            Orientation = System.Windows.Controls.Orientation.Vertical,
            IsToolTipEnabled = true,
            ToolTip = toolTip,
            CommandHandler = new SendCommandHandler(command),
            LargeImage = RibbonIcons.Load(ResourceName(largeIcon), 32),
            Image = RibbonIcons.Load(ResourceName(smallIcon), 16),
        };

        return button;
    }

    private static string ResourceName(string fileName) =>
        "CadBridge.AutoCAD.Assets." + fileName;
}
