namespace CadBridge.Core.Services.Units;

public static class UnitConverter
{
    public const double CmPerMeter = 100.0;
    public const double Cm2PerM2 = 10_000.0;
    public const double MmPerMeter = 1000.0;
    public const double InchesPerFoot = 12.0;
    public const double InchesPerMeter = 39.37007874015748;

    public static double CmToMeters(double cm) => cm / CmPerMeter;

    public static double CmSquaredToSquareMeters(double cm2) => cm2 / Cm2PerM2;

    public static double MetersToCm(double meters) => meters * CmPerMeter;

    public static double SquareMetersToCm2(double m2) => m2 * Cm2PerM2;

    public static double FeetToMeters(double feet) => feet * InchesPerFoot / InchesPerMeter;

    public static double MetersToFeet(double meters) => meters * InchesPerMeter / InchesPerFoot;

    public static bool ExceedsTolerance(double autocadArea, double revitArea, double relativeTolerance = 0.01)
    {
        if (autocadArea == 0.0)
            return Math.Abs(revitArea) > relativeTolerance;
        return Math.Abs(revitArea - autocadArea) / Math.Abs(autocadArea) > relativeTolerance;
    }
}
