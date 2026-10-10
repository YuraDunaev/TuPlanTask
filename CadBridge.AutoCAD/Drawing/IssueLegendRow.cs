using CadBridge.Core.Models;

namespace CadBridge.AutoCAD.Drawing;

public sealed record IssueLegendRow(string Marker, string Label, string Description, WarningCode Code);
