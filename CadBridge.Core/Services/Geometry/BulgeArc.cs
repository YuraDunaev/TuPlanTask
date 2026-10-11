using CadBridge.Core.Models;

namespace CadBridge.Core.Services.Geometry;

public readonly struct BulgeArc
{
    private const double Epsilon = 1e-23;

    public const int DefaultSegments = 32;

    public bool IsArc { get; }

    public double Sweep { get; }

    public double Radius { get; }

    public Point2D Center { get; }

    public double StartAngle { get; }

    public double Chord { get; }

    public Point2D From { get; }

    public Point2D To { get; }

    public double ArcLength => IsArc ? Radius * Math.Abs(Sweep) : Chord;

    public double SegmentSignedArea => IsArc
        ? 0.5 * Radius * Radius * (Sweep - Math.Sin(Sweep))
        : 0.0;

    private BulgeArc(Point2D from, Point2D to, double sweep, double radius, Point2D center, double startAngle)
    {
        From = from;
        To = to;
        IsArc = true;
        Sweep = sweep;
        Radius = radius;
        Center = center;
        StartAngle = startAngle;
        Chord = from.DistanceTo(to);
    }

    private BulgeArc(Point2D from, Point2D to, double chord)
    {
        From = from;
        To = to;
        IsArc = false;
        Sweep = 0.0;
        Radius = 0.0;
        Center = default;
        StartAngle = 0.0;
        Chord = chord;
    }

    public static BulgeArc Create(Point2D from, Point2D to, double bulge)
    {
        var chord = from.DistanceTo(to);
        if (bulge == 0.0 || chord <= Epsilon)
            return new BulgeArc(from, to, chord);

        var sweep = 4.0 * Math.Atan(bulge);
        var radius = chord / (2.0 * Math.Sin(Math.Abs(sweep) / 2.0));
        var offset = chord / 2.0 / Math.Tan(sweep / 2.0);
        var mid = new Point2D((from.X + to.X) / 2.0, (from.Y + to.Y) / 2.0);
        var left = new Point2D(-(to.Y - from.Y) / chord, (to.X - from.X) / chord);
        var center = new Point2D(mid.X + left.X * offset, mid.Y + left.Y * offset);
        var startAngle = Math.Atan2(from.Y - center.Y, from.X - center.X);

        return new BulgeArc(from, to, sweep, radius, center, startAngle);
    }

    public IEnumerable<Point2D> Flatten(int segments = DefaultSegments)
    {
        if (!IsArc || segments < 1)
            yield break;

        for (var i = 1; i < segments; i++)
        {
            var angle = StartAngle + Sweep * i / segments;
            yield return new Point2D(
                Center.X + Radius * Math.Cos(angle),
                Center.Y + Radius * Math.Sin(angle));
        }
    }
}
