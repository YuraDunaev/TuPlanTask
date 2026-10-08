using CadBridge.Core.Models;

namespace CadBridge.Core.Tests.Models;

public sealed class MeasureTests
{
    [Fact]
    public void Constructor_KeepsValueAndUnit()
    {
        var measure = new Measure(18.45, MeasureUnit.Meter);

        Assert.Equal(18.45, measure.Value);
        Assert.Equal(MeasureUnit.Meter, measure.Unit);
    }

    [Fact]
    public void Constructor_RejectsNegativeOrNonFiniteValue()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Measure(-1, MeasureUnit.Meter));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Measure(double.NaN, MeasureUnit.Meter));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Measure(double.PositiveInfinity, MeasureUnit.Meter));
    }

    [Fact]
    public void Constructor_AllowsZero()
    {
        var measure = new Measure(0, MeasureUnit.Meter);

        Assert.Equal(0, measure.Value);
    }

    [Fact]
    public void Equals_ComparesValueAndUnit()
    {
        var a = new Measure(10, MeasureUnit.Centimeter);
        var b = new Measure(10, MeasureUnit.Centimeter);
        var c = new Measure(10, MeasureUnit.Meter);

        Assert.True(a == b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
        Assert.True(a != c);
        Assert.NotEqual(a, c);
    }
}

public sealed class MeasureUnitExtensionsTests
{
    [Theory]
    [InlineData(MeasureUnit.Millimeter, "mm")]
    [InlineData(MeasureUnit.Centimeter, "cm")]
    [InlineData(MeasureUnit.Meter, "m")]
    [InlineData(MeasureUnit.Inch, "in")]
    [InlineData(MeasureUnit.Foot, "ft")]
    public void ToSymbol_MapsEveryKnownUnit(MeasureUnit unit, string symbol)
    {
        Assert.Equal(symbol, unit.ToSymbol());
    }

    [Fact]
    public void ToSymbol_FallsBackToNameForUnknownUnit()
    {
        var unknown = (MeasureUnit)999;

        Assert.Equal("999", unknown.ToSymbol());
    }

    [Fact]
    public void ToString_AppendsUnitSymbol()
    {
        var measure = new Measure(18.45, MeasureUnit.Meter);

        Assert.Equal("18.45 m", measure.ToString());
    }
}
