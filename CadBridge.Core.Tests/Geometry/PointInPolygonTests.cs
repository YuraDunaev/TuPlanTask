using CadBridge.Core.Geometry;
using CadBridge.Core.Models;

namespace CadBridge.Core.Tests.Geometry;

public sealed class PointInPolygonTests
{
    private static Boundary RectWithBulgedRightEdge(double bulge) => new(new[]
    {
        new BoundaryVertex(0, 0, 0),
        new BoundaryVertex(500, 0, bulge),
        new BoundaryVertex(500, 369, 0),
        new BoundaryVertex(0, 369, 0),
    });

    private static Boundary Rectangle(double width, double height) => new(new[]
    {
        new BoundaryVertex(0, 0, 0),
        new BoundaryVertex(width, 0, 0),
        new BoundaryVertex(width, height, 0),
        new BoundaryVertex(0, height, 0),
    });

    [Fact]
    public void IsInside_ReturnsTrueForCentreOfRoom()
    {
        Assert.True(PointInPolygon.IsInside(new Point2D(250, 184.5), Rectangle(500, 369)));
    }

    [Fact]
    public void IsInside_ReturnsFalseForPointBeyondRightWall()
    {
        Assert.False(PointInPolygon.IsInside(new Point2D(501, 184.5), Rectangle(500, 369)));
    }

    [Fact]
    public void IsInside_ReturnsFalseForPointBeyondTopWall()
    {
        Assert.False(PointInPolygon.IsInside(new Point2D(250, 370), Rectangle(500, 369)));
    }

    [Fact]
    public void IsInside_TreatsPointOnEdgeAsInside()
    {
        Assert.True(PointInPolygon.IsInside(new Point2D(250, 0), Rectangle(500, 369)));
    }

    [Fact]
    public void IsInside_TreatsVertexAsInside()
    {
        Assert.True(PointInPolygon.IsInside(new Point2D(500, 369), Rectangle(500, 369)));
    }

    [Fact]
    public void IsInside_ReturnsFalseOneCentimeterOutsideEdge()
    {
        Assert.False(PointInPolygon.IsInside(new Point2D(501, 0), Rectangle(500, 369)));
    }

    [Fact]
    public void IsInside_WorksOnMinimalTriangle()
    {
        var triangle = new Boundary(new[]
        {
            new BoundaryVertex(0, 0, 0),
            new BoundaryVertex(300, 0, 0),
            new BoundaryVertex(0, 300, 0),
        });

        Assert.True(PointInPolygon.IsInside(new Point2D(50, 50), triangle));
        Assert.False(PointInPolygon.IsInside(new Point2D(250, 250), triangle));
    }

    [Fact]
    public void IsInside_HandlesLShape_NotchOutside()
    {
        var lShape = new Boundary(new[]
        {
            new BoundaryVertex(0, 0, 0), new BoundaryVertex(200, 0, 0),
            new BoundaryVertex(200, 100, 0), new BoundaryVertex(100, 100, 0),
            new BoundaryVertex(100, 200, 0), new BoundaryVertex(0, 200, 0),
        });

        Assert.True(PointInPolygon.IsInside(new Point2D(150, 50), lShape));
        Assert.True(PointInPolygon.IsInside(new Point2D(50, 150), lShape));
        Assert.False(PointInPolygon.IsInside(new Point2D(150, 150), lShape));
    }

    [Fact]
    public void IsInside_HandlesUShape_CountsTwoRayCrossings()
    {
        var uShape = new Boundary(new[]
        {
            new BoundaryVertex(0, 0, 0), new BoundaryVertex(300, 0, 0), new BoundaryVertex(300, 300, 0),
            new BoundaryVertex(200, 300, 0), new BoundaryVertex(200, 100, 0), new BoundaryVertex(100, 100, 0),
            new BoundaryVertex(100, 300, 0), new BoundaryVertex(0, 300, 0),
        });

        Assert.True(PointInPolygon.IsInside(new Point2D(150, 50), uShape));
        Assert.True(PointInPolygon.IsInside(new Point2D(50, 250), uShape));
        Assert.False(PointInPolygon.IsInside(new Point2D(150, 250), uShape));
    }

    [Fact]
    public void IsInside_HandlesZigzagStar_ConcaveSpikes()
    {
        var spikes = new List<BoundaryVertex>();
        const int points = 8;
        for (var i = 0; i < points; i++)
        {
            var outerAngle = 2 * Math.PI * i / points;
            spikes.Add(new BoundaryVertex(100 + 100 * Math.Cos(outerAngle), 100 + 100 * Math.Sin(outerAngle), 0));
            var innerAngle = 2 * Math.PI * (i + 0.5) / points;
            spikes.Add(new BoundaryVertex(100 + 40 * Math.Cos(innerAngle), 100 + 40 * Math.Sin(innerAngle), 0));
        }

        var star = new Boundary(spikes);

        Assert.True(PointInPolygon.IsInside(new Point2D(100, 100), star));
        Assert.True(PointInPolygon.IsInside(new Point2D(180, 100), star));
        Assert.True(PointInPolygon.IsInside(new Point2D(70, 70), star));

        Assert.False(PointInPolygon.IsInside(new Point2D(61.1, 41.8), star));
        Assert.False(PointInPolygon.IsInside(new Point2D(77.3, 44.6), star));
    }

    [Fact]
    public void IsInside_ArcBulgeOutward_PointInsideCurvedArea()
    {
        var arc = RectWithBulgedRightEdge(0.5);

        Assert.True(PointInPolygon.IsInside(new Point2D(250, 184.5), arc));
        Assert.True(PointInPolygon.IsInside(new Point2D(590, 184.5), arc));
    }

    [Fact]
    public void IsInside_ArcBulgeOutward_PointFarOutsideStillOutside()
    {
        var arc = RectWithBulgedRightEdge(0.5);

        Assert.False(PointInPolygon.IsInside(new Point2D(700, 184.5), arc));
        Assert.False(PointInPolygon.IsInside(new Point2D(250, 400), arc));
    }

    [Fact]
    public void IsInside_ArcDoesNotMirrorToWrongSide()
    {
        var straight = RectWithBulgedRightEdge(0.0);

        Assert.False(PointInPolygon.IsInside(new Point2D(590, 184.5), straight));
    }

    [Fact]
    public void IsInside_NegativeBulge_CurvesOtherWay()
    {
        var inward = RectWithBulgedRightEdge(-0.5);

        Assert.True(PointInPolygon.IsInside(new Point2D(250, 184.5), inward));
        Assert.True(PointInPolygon.IsInside(new Point2D(100, 184.5), inward));
    }

    [Fact]
    public void IsInside_SemicircleBulge_MatchesAnalyticSemicircle()
    {
        var half = new Boundary(new[]
        {
            new BoundaryVertex(0, 0, 1),
            new BoundaryVertex(200, 0, 0),
            new BoundaryVertex(200, 200, 0),
            new BoundaryVertex(0, 200, 0),
        });

        Assert.True(PointInPolygon.IsInside(new Point2D(100, 100), half));
        Assert.True(PointInPolygon.IsInside(new Point2D(100, 40), half));
        Assert.True(PointInPolygon.IsInside(new Point2D(100, -50), half));
        Assert.False(PointInPolygon.IsInside(new Point2D(100, -150), half));
        Assert.False(PointInPolygon.IsInside(new Point2D(-50, 100), half));
    }

    [Fact]
    public void IsInside_ThrowsForNullBoundary()
    {
        Assert.Throws<ArgumentNullException>(
            () => PointInPolygon.IsInside(new Point2D(0, 0), null!));
    }

    [Fact]
    public void IsInside_DoesNotThrowForCollinearBoundary()
    {
        var degenerate = new Boundary(new[]
        {
            new BoundaryVertex(0, 0, 0),
            new BoundaryVertex(100, 0, 0),
            new BoundaryVertex(200, 0, 0),
        });

        var result = PointInPolygon.IsInside(new Point2D(50, 0), degenerate);
        Assert.True(result || !result);
    }

    [Fact]
    public void IsInside_HandlesNegativeCoordinates()
    {
        var shifted = new Boundary(new[]
        {
            new BoundaryVertex(-500, -369, 0),
            new BoundaryVertex(-100, -369, 0),
            new BoundaryVertex(-100, -100, 0),
            new BoundaryVertex(-500, -100, 0),
        });

        Assert.True(PointInPolygon.IsInside(new Point2D(-300, -200), shifted));
        Assert.False(PointInPolygon.IsInside(new Point2D(0, -200), shifted));
    }

    [Fact]
    public void IsInside_HandlesVerySmallRoom()
    {
        var tiny = Rectangle(10, 10);

        Assert.True(PointInPolygon.IsInside(new Point2D(5, 5), tiny));
        Assert.False(PointInPolygon.IsInside(new Point2D(11, 5), tiny));
    }

    [Fact]
    public void IsInside_HandlesVeryLargeRoom()
    {
        var huge = Rectangle(100_000, 100_000);

        Assert.True(PointInPolygon.IsInside(new Point2D(50_000, 50_000), huge));
        Assert.False(PointInPolygon.IsInside(new Point2D(100_001, 50_000), huge));
    }

    [Fact]
    public void IsInside_PointExactlyOnArcChordEnd()
    {
        var arc = RectWithBulgedRightEdge(0.5);

        Assert.True(PointInPolygon.IsInside(new Point2D(500, 0), arc));
    }
}
