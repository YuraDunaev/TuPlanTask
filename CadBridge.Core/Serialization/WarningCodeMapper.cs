using CadBridge.Core.Models;

namespace CadBridge.Core.Serialization;

public static class WarningCodeMapper
{
    public static string ToWire(WarningCode code) => code switch
    {
        WarningCode.OpenPolyline => "OPEN_POLYLINE",
        WarningCode.NoTag => "NO_TAG",
        WarningCode.MultipleTags => "MULTIPLE_TAGS",
        WarningCode.OrphanTag => "ORPHAN_TAG",
        WarningCode.CalculationFailed => "CALCULATION_FAILED",
        WarningCode.NoLabelPoint => "NO_LABEL_POINT",
        _ => "UNKNOWN",
    };

    public static WarningCode FromWire(string? wire) => wire?.Trim().ToUpperInvariant() switch
    {
        "OPEN_POLYLINE" => WarningCode.OpenPolyline,
        "NO_TAG" => WarningCode.NoTag,
        "MULTIPLE_TAGS" => WarningCode.MultipleTags,
        "ORPHAN_TAG" => WarningCode.OrphanTag,
        "CALCULATION_FAILED" => WarningCode.CalculationFailed,
        "NO_LABEL_POINT" => WarningCode.NoLabelPoint,
        _ => WarningCode.UnknownCode,
    };
}
