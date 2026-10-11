using System;
using Autodesk.Revit.UI;

namespace CadBridge.Revit.Ribbon;

public static class RibbonBuilder
{
    public static void Build(UIControlledApplication application)
    {
        application.CreateRibbonTab("CAD Bridge");
        var panel = application.CreateRibbonPanel("CAD Bridge", "Rooms");

        var importRoomsButton = new PushButtonData(
            "importRooms",
            "Import Rooms",
            typeof(RibbonBuilder).Assembly.Location,
            "CadBridge.Revit.Commands.ImportRoomsCommand");
        importRoomsButton.Image = IconLoader.Load("import-rooms-16.png");
        importRoomsButton.LargeImage = IconLoader.Load("import-rooms-32.png");
        panel.AddItem(importRoomsButton);

        var exportReportButton = new PushButtonData(
            "exportReport",
            "Export Report",
            typeof(RibbonBuilder).Assembly.Location,
            "CadBridge.Revit.Commands.ExportReportCommand");
        exportReportButton.Image = IconLoader.Load("export-report-16.png");
        exportReportButton.LargeImage = IconLoader.Load("export-report-32.png");
        panel.AddItem(exportReportButton);
    }
}
