using CadBridge.Core.Models;
using CadBridge.Core.Services.Geometry;

namespace CadBridge.Core.Geometry;

public static class PointInPolygon
{
    private const double OnEdgeTolerance = 1e-6;

    private const double ChordToleranceCm = 0.05;

    private const int MinArcSegments = 8;

    private const int MaxArcSegments = 512;

    public static bool IsInside(Point2D point, Boundary boundary)
    {
        ArgumentNullException.ThrowIfNull(boundary);
        return IsInside(point, Flatten(boundary));
    }

    public static bool IsInside(Point2D point, List<Point2D> polygon)
    {
        if (polygon.Count < 3)
            return false;

        if (IsOnBoundary(point, polygon))
            return true;

        var inside = false;
        for (var i = 0; i < polygon.Count; i++)
        {
            var a = polygon[i];
            var b = polygon[(i + 1) % polygon.Count];
            if ((a.Y > point.Y) == (b.Y > point.Y))
                continue;

            var x = a.X + (point.Y - a.Y) / (b.Y - a.Y) * (b.X - a.X);
            if (x > point.X)
                inside = !inside;
        }

        return inside;
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
            {
                var arc = BulgeArc.Create(vertex.Point, next, vertex.Bulge);
                polygon.AddRange(arc.Flatten(SegmentCountFor(arc)));
            }
        }

        return polygon;
    }

    private static int SegmentCountFor(BulgeArc arc)
    {
        if (!arc.IsArc || arc.Radius <= 0.0)
            return 1;

        var ratio = 1.0 - ChordToleranceCm / arc.Radius;
        if (ratio <= -1.0)
            return MaxArcSegments;

        var maxStep = 2.0 * Math.Acos(Math.Clamp(ratio, -1.0, 1.0));
        if (maxStep <= 0.0)
            return MaxArcSegments;

        var segments = (int)Math.Ceiling(Math.Abs(arc.Sweep) / maxStep);
        return Math.Clamp(segments, MinArcSegments, MaxArcSegments);
    }

    private static bool IsOnBoundary(Point2D point, List<Point2D> polygon)
    {
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
            if (t < 0.0 || t > 1.0)
                continue;

            var projection = new Point2D(a.X + abx * t, a.Y + aby * t);
            if (point.DistanceTo(projection) <= OnEdgeTolerance)
                return true;
        }

        return false;
    }
}

