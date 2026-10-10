namespace CadBridge.Core.Services.Analysis;

public static class AreaDelta
{
    public static double? Ratio(double revitArea, double? cadArea)
    {
        if (cadArea is null || cadArea.Value <= 0)
            return null;

        return Math.Abs(revitArea - cadArea.Value) / cadArea.Value;
    }

    public static bool Exceeds(double revitArea, double? cadArea, double toleranceRatio)
    {
        var ratio = Ratio(revitArea, cadArea);
        return ratio is not null && ratio.Value > toleranceRatio;
    }
}