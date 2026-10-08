using CadBridge.Core.Models;

namespace CadBridge.Core.Geometry;

public static class PointInPolygon
{
    private const int ArcSegments = 32;

    private const double OnEdgeTolerance = 1e-6;

    public static bool IsInside(Point2D point, Boundary boundary)
    {
        ArgumentNullException.ThrowIfNull(boundary);

        var polygon = Flatten(boundary);
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
                AddArcPoints(polygon, vertex.Point, next, vertex.Bulge);
        }

        return polygon;
    }

    private static void AddArcPoints(List<Point2D> polygon, Point2D from, Point2D to, double bulge)
    {
        var chord = from.DistanceTo(to);
        if (chord <= OnEdgeTolerance)
            return;

        var sweep = 4.0 * Math.Atan(bulge);
        var radius = chord / (2.0 * Math.Sin(Math.Abs(sweep) / 2.0));
        var offset = chord / 2.0 / Math.Tan(sweep / 2.0);
        var mid = new Point2D((from.X + to.X) / 2.0, (from.Y + to.Y) / 2.0);
        var left = new Point2D(-(to.Y - from.Y) / chord, (to.X - from.X) / chord);
        var center = mid + left * offset;
        var startAngle = Math.Atan2(from.Y - center.Y, from.X - center.X);

        for (var i = 1; i < ArcSegments; i++)
        {
            var angle = startAngle + sweep * i / ArcSegments;
            polygon.Add(new Point2D(center.X + radius * Math.Cos(angle), center.Y + radius * Math.Sin(angle)));
        }
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

