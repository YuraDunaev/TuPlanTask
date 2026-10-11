namespace CadBridge.Core.Models;

public static class WarningCodeText
{

    public static string ShortLabel(WarningCode code) => code switch
    {
        WarningCode.OpenPolyline => "OPEN",
        WarningCode.NoTag => "NO TAG",
        WarningCode.MultipleTags => "TAGS x2",
        WarningCode.OrphanTag => "ORPHAN",
        WarningCode.CalculationFailed => "CALC",
        WarningCode.NoLabelPoint => "NO LABEL",
        _ => "ISSUE",
    };

    public static string Description(WarningCode code) => code switch
    {
        WarningCode.OpenPolyline => "Broken contour: the room boundary polyline is not closed.",
        WarningCode.NoTag => "No tag: no label with a room number was found inside the contour.",
        WarningCode.MultipleTags => "Multiple tags: several labels inside the contour, the first one is used.",
        WarningCode.OrphanTag => "Orphan tag: the label lies outside any room boundary.",
        WarningCode.CalculationFailed => "Not calculated: the contour geometry is invalid (too few vertices or degenerate).",
        WarningCode.NoLabelPoint => "No label point: could not place a tag strictly inside the room; the room was skipped.",
        _ => "Unknown discrepancy code.",
    };
}
