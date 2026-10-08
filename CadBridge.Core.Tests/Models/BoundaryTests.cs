using CadBridge.Core.Models;

namespace CadBridge.Core.Tests.Models;

public sealed class BoundaryTests
{
    [Fact]
    public void Constructor_DropsDuplicatedClosingVertex()
    {
        // AutoCAD нередко отдаёт последнюю вершину, равную первой
        var boundary = new Boundary(new[]
        {
            new BoundaryVertex(0, 0, 0),
            new BoundaryVertex(100, 0, 0),
            new BoundaryVertex(100, 100, 0),
            new BoundaryVertex(0, 0, 0),
        });

        Assert.Equal(3, boundary.Count);
    }

    [Fact]
    public void Constructor_RejectsFewerThanThreeVertices()
    {
        Assert.Throws<ArgumentException>(() => new Boundary(new[]
        {
            new BoundaryVertex(0, 0, 0),
            new BoundaryVertex(100, 0, 0),
        }));
    }

    [Fact]
    public void Constructor_ThrowsForNullSequence()
    {
        Assert.Throws<ArgumentNullException>(() => new Boundary(null!));
    }

    [Fact]
    public void IsArc_ReflectsBulge()
    {
        Assert.True(new BoundaryVertex(0, 0, 0.5).IsArc);
        Assert.False(new BoundaryVertex(0, 0, 0).IsArc);
        Assert.True(new BoundaryVertex(0, 0, -1).IsArc);
    }

    [Fact]
    public void ToString_UsesInvariantCultureForBulge()
    {
        // На ru-RU без инварианта bulge=0.5 превратился бы в "0,5" в логах.
        Assert.Equal("(10, 20) bulge=0.5", new BoundaryVertex(10, 20, 0.5).ToString());
        Assert.Equal("(10, 20)", new BoundaryVertex(10, 20, 0).ToString());
    }

    [Fact]
    public void Equals_ComparesPointAndBulge()
    {
        var a = new BoundaryVertex(10, 20, 0.5);
        var b = new BoundaryVertex(10, 20, 0.5);
        var c = new BoundaryVertex(10, 20, 0);

        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
        Assert.NotEqual(a, c);
    }

    [Fact]
    public void Indexer_ExposesVerticesInOrder()
    {
        var boundary = new Boundary(new[]
        {
            new BoundaryVertex(1, 2, 0),
            new BoundaryVertex(3, 4, 0),
            new BoundaryVertex(5, 6, 0),
        });

        Assert.Equal(new Point2D(1, 2), boundary[0].Point);
        Assert.Equal(new Point2D(5, 6), boundary[2].Point);
    }
}
