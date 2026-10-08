using CadBridge.Core.Geometry;
using CadBridge.Core.Models;

namespace CadBridge.Core.Services.Geometry;

public static class LabelPointResolver
{
    private const int GridResolution = 64;

    public static bool TryResolve(Boundary boundary, out Point2D labelPoint)
    {
        ArgumentNullException.ThrowIfNull(boundary);
        labelPoint = default;

        if (boundary.Count < 3)
            return false;

        var polygon = Flatten(boundary);
        if (polygon.Count < 3)
            return false;

        var centroid = ComputeCentroid(polygon);
        if (PointInPolygon.IsInside(centroid, boundary))
        {
            labelPoint = centroid;
            return true;
        }

        return TryPoleOfInaccessibility(polygon, boundary, out labelPoint);
    }

    private static List<Point2D> Flatten(Boundary boundary)
    {
        var polygon = new List<Point2D>(boundary.Count * 2);
        for (var i = 0; i < boundary.Count; i++)
        {
            var vertex = boundary[i];
            var next = boundary[(i + 1) % boundary.Count].Point;
            polygon.Add(vertex.Point);
            if (vertex.IsArc)
                polygon.AddRange(BulgeArc.Create(vertex.Point, next, vertex.Bulge).Flatten());
        }

        return polygon;
    }

    private static Point2D ComputeCentroid(List<Point2D> polygon)
    {
        var sumX = 0.0;
        var sumY = 0.0;
        foreach (var p in polygon)
        {
            sumX += p.X;
            sumY += p.Y;
        }

        return new Point2D(sumX / polygon.Count, sumY / polygon.Count);
    }

    private static bool TryPoleOfInaccessibility(List<Point2D> polygon, Boundary boundary, out Point2D best)
    {
        best = default;

        var minX = double.MaxValue;
        var minY = double.MaxValue;
        var maxX = double.MinValue;
        var maxY = double.MinValue;
        foreach (var p in polygon)
        {
            minX = Math.Min(minX, p.X);
            minY = Math.Min(minY, p.Y);
            maxX = Math.Max(maxX, p.X);
            maxY = Math.Max(maxY, p.Y);
        }

        var bestDistance = -1.0;
        for (var ix = 0; ix <= GridResolution; ix++)
        {
            var x = minX + (maxX - minX) * ix / GridResolution;
            for (var iy = 0; iy <= GridResolution; iy++)
            {
                var y = minY + (maxY - minY) * iy / GridResolution;
                var candidate = new Point2D(x, y);
                if (!PointInPolygon.IsInside(candidate, boundary))
                    continue;

                var distance = DistanceToNearestEdge(candidate, polygon);
                if (distance > bestDistance)
                {
                    bestDistance = distance;
                    best = candidate;
                }
            }
        }

        return bestDistance >= 0.0;
    }

    private static double DistanceToNearestEdge(Point2D point, List<Point2D> polygon)
    {
        var min = double.MaxValue;
        for (var i = 0; i < polygon.Count; i++)
        {
            var a = polygon[i];
            var b = polygon[(i + 1) % polygon.Count];
            var abx = b.X - a.X;
            var aby = b.Y - a.Y;
            var lengthSquared = abx * abx + aby * aby;
            if (lengthSquared == 0.0)
                continue;

            var t = ((point.X - a.X) * abx + (point.Y - a.Y) * aby) / lengthSquared;
            t = Math.Clamp(t, 0.0, 1.0);
            var projection = new Point2D(a.X + abx * t, a.Y + aby * t);
            min = Math.Min(min, point.DistanceTo(projection));
        }

        return min;
    }
}
