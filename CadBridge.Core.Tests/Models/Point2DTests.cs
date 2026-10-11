using CadBridge.Core.Models;

namespace CadBridge.Core.Tests.Models;

public sealed class Point2DTests
{
    [Fact]
    public void Constructor_RejectsNonFiniteCoordinates()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Point2D(double.NaN, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Point2D(0, double.PositiveInfinity));
    }

    [Fact]
    public void DistanceTo_ComputesEuclideanDistance()
    {
        var a = new Point2D(0, 0);
        var b = new Point2D(300, 400);

        Assert.Equal(500, a.DistanceTo(b), 9);
    }

    [Fact]
    public void Operators_AddSubtractScale()
    {
        var a = new Point2D(10, 20);
        var b = new Point2D(1, 2);

        Assert.Equal(new Point2D(11, 22), a + b);
        Assert.Equal(new Point2D(9, 18), a - b);
        Assert.Equal(new Point2D(30, 60), a * 3);
    }

    [Fact]
    public void Equals_UsesExactValueComparison()
    {
        var a = new Point2D(1.5, 2.5);
        var b = new Point2D(1.5, 2.5);
        var c = new Point2D(1.5, 2.6);

        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.NotEqual(a, c);
        Assert.True(a != c);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void ToString_FormatsPair()
    {
        Assert.Equal("(1, 2)", new Point2D(1, 2).ToString());
    }

    [Fact]
    public void ToString_UsesInvariantCultureForFractions()
    {
        Assert.Equal("(1.5, 2.25)", new Point2D(1.5, 2.25).ToString());
    }
}

public sealed class RoomIdTests
{
    [Fact]
    public void From_RejectsBlankHandles()
    {
        Assert.Throws<ArgumentException>(() => RoomId.From("  "));
        Assert.Throws<ArgumentException>(() => RoomId.From(string.Empty));
    }

    [Fact]
    public void TryParse_FailsForBlankAndSucceedsForHandle()
    {
        Assert.False(RoomId.TryParse(null, out _));
        Assert.False(RoomId.TryParse("   ", out _));
        Assert.True(RoomId.TryParse("2A5", out var id));
        Assert.Equal("2A5", id.Value);
    }

    [Fact]
    public void Default_IsEmpty()
    {
        Assert.True(default(RoomId).IsEmpty);
        Assert.Equal(string.Empty, default(RoomId).Value);
    }

    [Fact]
    public void Equals_IsCaseSensitiveOrdinal()
    {
        Assert.Equal(RoomId.From("abc"), RoomId.From("abc"));
        Assert.NotEqual(RoomId.From("abc"), RoomId.From("ABC"));
    }
}
