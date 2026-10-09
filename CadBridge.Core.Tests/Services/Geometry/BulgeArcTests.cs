using CadBridge.Core.Models;
using CadBridge.Core.Services.Geometry;

namespace CadBridge.Core.Tests.Services.Geometry;

public sealed class BulgeArcTests
{
    [Fact]
    public void StraightEdge_HasNoArc()
    {
        var arc = BulgeArc.Create(new Point2D(0, 0), new Point2D(100, 0), bulge: 0.0);

        Assert.False(arc.IsArc);
        Assert.Equal(0.0, arc.SegmentSignedArea);
        Assert.Equal(100.0, arc.ArcLength, precision: 9);
        Assert.Empty(arc.Flatten());
    }

    [Fact]
    public void Semicircle_BulgeOne_SweepIsPi()
    {
        // bulge = 1 → θ = 4·atan(1) = π (полуокружность)
        var arc = BulgeArc.Create(new Point2D(0, 0), new Point2D(200, 0), bulge: 1.0);

        Assert.True(arc.IsArc);
        Assert.Equal(Math.PI, arc.Sweep, precision: 9);
        Assert.Equal(100.0, arc.Radius, precision: 9);      // r = chord/2
        Assert.Equal(100.0 * Math.PI, arc.ArcLength, precision: 6); // r·θ
    }

    [Fact]
    public void QuarterCircle_Bulge04142_SweepIsHalfPi()
    {
        var arc = BulgeArc.Create(new Point2D(0, 0), new Point2D(100, 0), bulge: Math.Tan(Math.PI / 8));

        Assert.True(arc.IsArc);
        Assert.Equal(Math.PI / 2, arc.Sweep, precision: 9);
    }

    [Fact]
    public void Flatten_ReturnsSegmentsMinusOne_InteriorPoints()
    {
        var arc = BulgeArc.Create(new Point2D(0, 0), new Point2D(100, 0), bulge: 0.5);

        Assert.Equal(BulgeArc.DefaultSegments - 1, arc.Flatten().Count());
        Assert.Equal(9, arc.Flatten(10).Count());
    }

    [Fact]
    public void SegmentSignedArea_SignFollowsBulge()
    {
        var positive = BulgeArc.Create(new Point2D(0, 0), new Point2D(200, 0), bulge: 0.5);
        var negative = BulgeArc.Create(new Point2D(0, 0), new Point2D(200, 0), bulge: -0.5);

        Assert.True(positive.SegmentSignedArea > 0);
        Assert.True(negative.SegmentSignedArea < 0);
        Assert.Equal(positive.SegmentSignedArea, -negative.SegmentSignedArea, precision: 9);
    }

    [Fact]
    public void DegenerateChord_IsNotArc()
    {
        var arc = BulgeArc.Create(new Point2D(50, 50), new Point2D(50, 50), bulge: 1.0);

        Assert.False(arc.IsArc);
        Assert.Empty(arc.Flatten());
    }

    [Fact]
    public void BulgeGreaterThanOne_SweepExceedsPi_AndLengthExceedsSemicircle()
    {
        // bulge = 1 → полуокружность; bulge = 2 → дуга больше полуокружности (θ > π), но меньше полного круга.
        var arc = BulgeArc.Create(new Point2D(0, 0), new Point2D(100, 0), bulge: 2.0);

        Assert.True(arc.IsArc);
        Assert.True(arc.Sweep > Math.PI, $"Sweep {arc.Sweep} должен превышать π ({Math.PI})");
        Assert.True(arc.Sweep < 2 * Math.PI, $"Sweep {arc.Sweep} не может быть больше полного круга");
        Assert.True(arc.Radius > 0);

        var semicircle = BulgeArc.Create(new Point2D(0, 0), new Point2D(100, 0), bulge: 1.0);
        Assert.True(arc.ArcLength > semicircle.ArcLength,
            $"Дуга ({arc.ArcLength:F2}) длиннее полуокружности ({semicircle.ArcLength:F2})");
    }

    [Fact]
    public void NegativeBulge_HasNegativeSweepButPositiveArcLength()
    {
        var negative = BulgeArc.Create(new Point2D(0, 0), new Point2D(200, 0), bulge: -0.5);
        var positive = BulgeArc.Create(new Point2D(0, 0), new Point2D(200, 0), bulge: 0.5);

        Assert.True(negative.Sweep < 0);
        Assert.Equal(positive.Sweep, -negative.Sweep, precision: 12);

        // Длина не зависит от направления обхода.
        Assert.Equal(positive.ArcLength, negative.ArcLength, precision: 12);

        // Знаковая площадь сегмента повторяет знак θ.
        Assert.True(negative.SegmentSignedArea < 0);
        Assert.Equal(positive.SegmentSignedArea, -negative.SegmentSignedArea, precision: 12);
    }

    [Fact]
    public void Flatten_StillProducesDefaultSegmentsForWideArc()
    {
        var arc = BulgeArc.Create(new Point2D(0, 0), new Point2D(100, 0), bulge: 2.0);

        Assert.Equal(BulgeArc.DefaultSegments - 1, arc.Flatten().Count());
        Assert.Equal(9, arc.Flatten(10).Count());
    }
}
