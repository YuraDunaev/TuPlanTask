namespace CadBridge.Core.Models;

public enum MeasureUnit
{
    Millimeter,
    Centimeter,
    Meter,
    Inch,
    Foot,
}

public static class MeasureUnitExtensions
{
    public static string ToSymbol(this MeasureUnit unit) => unit switch
    {
        MeasureUnit.Millimeter => "mm",
        MeasureUnit.Centimeter => "cm",
        MeasureUnit.Meter => "m",
        MeasureUnit.Inch => "in",
        MeasureUnit.Foot => "ft",
        _ => unit.ToString(),
    };
}

