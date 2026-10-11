using CadBridge.Core.Models;
using CadBridge.Core.Services.Units;

namespace CadBridge.Core.Services.Geometry;

public static class AreaCalculator
{
    public static double Calculate(Boundary boundary)
    {
        ArgumentNullException.ThrowIfNull(boundary);
        if (boundary.Count < 3)
            throw new ArgumentException("Boundary must contain at least 3 vertices.", nameof(boundary));

        var vertices = boundary.Vertices;
        var n = vertices.Count;
        var shoelace = 0.0;
        var segmentSum = 0.0;

        for (var i = 0; i < n; i++)
        {
            var a = vertices[i].Point;
            var b = vertices[(i + 1) % n].Point;
            shoelace += a.X * b.Y - b.X * a.Y;
            segmentSum += BulgeArc.Create(a, b, vertices[i].Bulge).SegmentSignedArea;
        }

        var areaCm2 = Math.Abs(shoelace / 2.0 + segmentSum);
        return UnitConverter.CmSquaredToSquareMeters(areaCm2);
    }
    
    public static double Perimeter(Boundary boundary)
    {
        ArgumentNullException.ThrowIfNull(boundary);
        if (boundary.Count < 3)
            throw new ArgumentException("Boundary must contain at least 3 vertices.", nameof(boundary));

        var vertices = boundary.Vertices;
        var n = vertices.Count;
        var perimeterCm = 0.0;

        for (var i = 0; i < n; i++)
        {
            var a = vertices[i].Point;
            var b = vertices[(i + 1) % n].Point;
            perimeterCm += BulgeArc.Create(a, b, vertices[i].Bulge).ArcLength;
        }

        return UnitConverter.CmToMeters(perimeterCm);
    }
}
