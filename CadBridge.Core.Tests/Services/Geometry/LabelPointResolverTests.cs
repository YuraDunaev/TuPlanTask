using CadBridge.Core.Geometry;
using CadBridge.Core.Models;
using CadBridge.Core.Services.Geometry;

namespace CadBridge.Core.Tests.Services.Geometry;

public sealed class LabelPointResolverTests
{
    private static Boundary Rectangle(double width, double height) => new(new[]
    {
        new BoundaryVertex(0, 0, 0),
        new BoundaryVertex(width, 0, 0),
        new BoundaryVertex(width, height, 0),
        new BoundaryVertex(0, height, 0),
    });

    private static Boundary UShape() => new(new[]
    {
        new BoundaryVertex(0, 0, 0),
        new BoundaryVertex(400, 0, 0),
        new BoundaryVertex(400, 400, 0),
        new BoundaryVertex(300, 400, 0),
        new BoundaryVertex(300, 100, 0),
        new BoundaryVertex(100, 100, 0),
        new BoundaryVertex(100, 400, 0),
        new BoundaryVertex(0, 400, 0),
    });

    private static Boundary LShape() => new(new[]
    {
        new BoundaryVertex(0, 0, 0),
        new BoundaryVertex(400, 0, 0),
        new BoundaryVertex(400, 200, 0),
        new BoundaryVertex(200, 200, 0),
        new BoundaryVertex(200, 400, 0),
        new BoundaryVertex(0, 400, 0),
    });

    [Fact]
    public void Rectangle_LabelIsCentroid()
    {
        var resolved = LabelPointResolver.TryResolve(Rectangle(500, 369), out var point);

        Assert.True(resolved);
        Assert.Equal(250.0, point.X, precision: 6);
        Assert.Equal(184.5, point.Y, precision: 6);
    }

    [Fact]
    public void UShape_LabelIsInside_NotInGap()
    {
        var boundary = UShape();
        var resolved = LabelPointResolver.TryResolve(boundary, out var point);

        Assert.True(resolved);
        Assert.True(PointInPolygon.IsInside(point, boundary),
            $"Label point {point} fell outside the room.");
    }

    [Fact]
    public void LShape_LabelIsInside()
    {
        var boundary = LShape();
        Assert.True(LabelPointResolver.TryResolve(boundary, out var point));
        Assert.True(PointInPolygon.IsInside(point, boundary));
    }

    [Fact]
    public void CentroidOfUShape_IsIndeedOutside_ProvingNeedForScan()
    {
        var boundary = UShape();
        var centroid = new Point2D(200, 225);
        Assert.False(PointInPolygon.IsInside(centroid, boundary));
    }

    [Fact]
    public void NullBoundary_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => LabelPointResolver.TryResolve(null!, out _));
    }
}
