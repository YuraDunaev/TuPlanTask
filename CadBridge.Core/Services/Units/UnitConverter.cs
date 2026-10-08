namespace CadBridge.Core.Services.Units;

public static class UnitConverter
{
    public const double CmPerMeter = 100.0;
    public const double Cm2PerM2 = 10_000.0;
    public const double MmPerMeter = 1000.0;
    public const double InchesPerFoot = 12.0;
    public const double InchesPerMeter = 39.37007874015748;

    /// <summary>Сантиметры → метры.</summary>
    public static double CmToMeters(double cm) => cm / CmPerMeter;

    /// <summary>Сантиметры² → метры².</summary>
    public static double CmSquaredToSquareMeters(double cm2) => cm2 / Cm2PerM2;

    /// <summary>Метры → сантиметры (обратно для проверок).</summary>
    public static double MetersToCm(double meters) => meters * CmPerMeter;

    /// <summary>Метры² → сантиметры².</summary>
    public static double SquareMetersToCm2(double m2) => m2 * Cm2PerM2;

    /// <summary>Футы (внутренние единицы Revit) → метры.</summary>
    public static double FeetToMeters(double feet) => feet * InchesPerFoot / InchesPerMeter;

    /// <summary>Метры → футы (внутренние единицы Revit).</summary>
    public static double MetersToFeet(double meters) => meters * InchesPerMeter / InchesPerFoot;

    public static bool ExceedsTolerance(double autocadArea, double revitArea, double relativeTolerance = 0.01)
    {
        if (autocadArea == 0.0)
            return Math.Abs(revitArea) > relativeTolerance;
        return Math.Abs(revitArea - autocadArea) / Math.Abs(autocadArea) > relativeTolerance;
    }
}
