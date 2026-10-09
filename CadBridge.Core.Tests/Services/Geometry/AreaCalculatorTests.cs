using CadBridge.Core.Models;
using CadBridge.Core.Services.Geometry;

namespace CadBridge.Core.Tests.Services.Geometry;

public sealed class AreaCalculatorTests
{
    private static Boundary Rectangle(double width, double height) => new(new[]
    {
        new BoundaryVertex(0, 0, 0),
        new BoundaryVertex(width, 0, 0),
        new BoundaryVertex(width, height, 0),
        new BoundaryVertex(0, height, 0),
    });

    [Fact]
    public void Rect500x369_AreaAndPerimeter()
    {
        var rect = Rectangle(500, 369);

        Assert.Equal(18.45, AreaCalculator.Calculate(rect), precision: 6);   // м²
        Assert.Equal(17.38, AreaCalculator.Perimeter(rect), precision: 6);    // м
    }

    [Fact]
    public void ConvexArcBumpsOutward_IncreasesArea()
    {
        var straight = Rectangle(500, 369);
        var convex = new Boundary(new[]
        {
            new BoundaryVertex(0, 0, 0),
            new BoundaryVertex(500, 0, 0.5), // дуга выпукло наружу
            new BoundaryVertex(500, 369, 0),
            new BoundaryVertex(0, 369, 0),
        });

        Assert.True(AreaCalculator.Calculate(convex) > AreaCalculator.Calculate(straight));
        Assert.Equal(20.829069, AreaCalculator.Calculate(convex), precision: 4);
    }

    [Fact]
    public void ConcaveArcDentsInward_DecreasesArea()
    {
        var concave = new Boundary(new[]
        {
            new BoundaryVertex(0, 0, 0),
            new BoundaryVertex(500, 0, -0.5), // дуга вогнуто внутрь
            new BoundaryVertex(500, 369, 0),
            new BoundaryVertex(0, 369, 0),
        });

        Assert.Equal(16.070931, AreaCalculator.Calculate(concave), precision: 4);
    }

    [Fact]
    public void Area_IsWindingIndependent()
    {
        var cw = new Boundary(new[]
        {
            new BoundaryVertex(0, 0, 0),
            new BoundaryVertex(0, 369, 0),
            new BoundaryVertex(500, 369, 0),
            new BoundaryVertex(500, 0, 0),
        });

        Assert.Equal(18.45, AreaCalculator.Calculate(cw), precision: 6);
    }

    [Fact]
    public void Perimeter_AddsArcLength_NotChord()
    {
        var withArc = new Boundary(new[]
        {
            new BoundaryVertex(0, 0, 0),
            new BoundaryVertex(500, 0, 0.5),
            new BoundaryVertex(500, 369, 0),
            new BoundaryVertex(0, 369, 0),
        });

        // дуговое ребро длиннее хорды, поэтому периметр больше, чем у прямоугольника
        Assert.True(AreaCalculator.Perimeter(withArc) > AreaCalculator.Perimeter(Rectangle(500, 369)));
        Assert.Equal(17.967, AreaCalculator.Perimeter(withArc), precision: 3);
    }

    [Fact]
    public void Triangle_Area()
    {
        var triangle = new Boundary(new[]
        {
            new BoundaryVertex(0, 0, 0),
            new BoundaryVertex(300, 0, 0),
            new BoundaryVertex(0, 300, 0),
        });

        Assert.Equal(4.5, AreaCalculator.Calculate(triangle), precision: 6); // (300·300/2)/10000 = 4.5 м²
    }

    [Fact]
    public void WideArcBulgeGreaterThanOne_AddsMoreSignedAreaThanSemicircle()
    {
        // §3.2.1: дуговая формула должна корректно работать и при θ > π (bulge > 1).
        var semicircle = new Boundary(new[]
        {
            new BoundaryVertex(0, 0, 0),
            new BoundaryVertex(500, 0, 1.0),
            new BoundaryVertex(500, 369, 0),
            new BoundaryVertex(0, 369, 0),
        });
        var wide = new Boundary(new[]
        {
            new BoundaryVertex(0, 0, 0),
            new BoundaryVertex(500, 0, 2.0),
            new BoundaryVertex(500, 369, 0),
            new BoundaryVertex(0, 369, 0),
        });

        Assert.True(AreaCalculator.Calculate(wide) > AreaCalculator.Calculate(semicircle),
            $"bulge>1 area {AreaCalculator.Calculate(wide)} > {AreaCalculator.Calculate(semicircle)}");
    }

    [Fact]
    public void Perimeter_WithWideArcBulge_AddsArcLength()
    {
        var wide = new Boundary(new[]
        {
            new BoundaryVertex(0, 0, 0),
            new BoundaryVertex(500, 0, 2.0),
            new BoundaryVertex(500, 369, 0),
            new BoundaryVertex(0, 369, 0),
        });

        Assert.True(AreaCalculator.Perimeter(wide) > AreaCalculator.Perimeter(Rectangle(500, 369)));
    }

    [Fact]
    public void NullBoundary_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => AreaCalculator.Calculate(null!));
        Assert.Throws<ArgumentNullException>(() => AreaCalculator.Perimeter(null!));
    }
}
