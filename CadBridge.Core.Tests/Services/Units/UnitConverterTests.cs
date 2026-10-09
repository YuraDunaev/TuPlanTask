using CadBridge.Core.Services.Units;

namespace CadBridge.Core.Tests.Services.Units;

public sealed class UnitConverterTests
{
    [Fact]
    public void CmToMeters() => Assert.Equal(1.0, UnitConverter.CmToMeters(100), precision: 12);

    [Fact]
    public void CmSquaredToSquareMeters() => Assert.Equal(1.0, UnitConverter.CmSquaredToSquareMeters(10_000), precision: 12);

    [Fact]
    public void Room500x369cm_Is18_45m2()
    {
        // 500 см × 369 см = 184500 см² = 18.45 м²
        Assert.Equal(18.45, UnitConverter.CmSquaredToSquareMeters(500 * 369), precision: 12);
    }

    [Fact]
    public void MetersToCm_RoundTrips()
    {
        Assert.Equal(250, UnitConverter.MetersToCm(2.5), precision: 12);
        Assert.Equal(500, UnitConverter.MetersToCm(UnitConverter.CmToMeters(500)), precision: 12);
    }

    [Fact]
    public void SquareMetersToCm2_RoundTrips()
    {
        var m2 = UnitConverter.CmSquaredToSquareMeters(184_500);
        Assert.Equal(184_500, UnitConverter.SquareMetersToCm2(m2), precision: 6);
    }

    [Fact]
    public void FeetToMeters_AndBack()
    {
        Assert.Equal(1.0, UnitConverter.MetersToFeet(UnitConverter.FeetToMeters(1.0)), precision: 9);
        Assert.Equal(3.280839895, UnitConverter.MetersToFeet(1.0), precision: 6);
    }

    [Theory]
    [InlineData(18.45, 18.45, false)]      // равны — не превышает
    [InlineData(18.45, 18.46, false)]      // 0.05 % — в пределах 1 %
    [InlineData(18.45, 18.70, true)]       // ~1.35 % — превышает 1 %
    [InlineData(18.45, 18.24, true)]       // ~1.14 % — превышает 1 %
    public void ExceedsTolerance_OnePercent(double autocad, double revit, bool expected)
    {
        Assert.Equal(expected, UnitConverter.ExceedsTolerance(autocad, revit));
    }

    [Fact]
    public void ExceedsTolerance_ZeroBaseline()
    {
        Assert.False(UnitConverter.ExceedsTolerance(0, 0.005));
        Assert.True(UnitConverter.ExceedsTolerance(0, 0.02));
    }
}
