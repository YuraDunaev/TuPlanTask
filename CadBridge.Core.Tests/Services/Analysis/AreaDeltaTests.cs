using CadBridge.Core.Services.Analysis;
using Xunit;

namespace CadBridge.Core.Tests.Services.Analysis;

public class AreaDeltaTests
{
    [Theory]
    [InlineData(100.0, 100.0, false)]
    [InlineData(100.5, 100.0, false)]
    [InlineData(101.0, 100.0, false)]
    [InlineData(102.0, 100.0, true)]
    [InlineData(98.0, 100.0, true)]
    [InlineData(100.0, 98.0, true)]
    [InlineData(101.5, 100.0, true)]
    [InlineData(101.9, 100.0, true)]
    public void Exceeds_uses_one_percent_tolerance(double revit, double cad, bool expected)
    {
        Assert.Equal(expected, AreaDelta.Exceeds(revit, cad, 0.01));
    }

    [Fact]
    public void Exceeds_returns_false_when_cad_area_missing_or_zero()
    {
        Assert.False(AreaDelta.Exceeds(100.0, null, 0.01));
        Assert.False(AreaDelta.Exceeds(100.0, 0.0, 0.01));
    }

    [Fact]
    public void Ratio_is_null_when_no_cad_area()
    {
        Assert.Null(AreaDelta.Ratio(100.0, null));
        Assert.Null(AreaDelta.Ratio(100.0, 0.0));
    }

    [Fact]
    public void Ratio_computes_relative_difference()
    {
        var ratio = AreaDelta.Ratio(105.0, 100.0);
        Assert.NotNull(ratio);
        Assert.Equal(0.05, ratio!.Value, 6);
    }
}