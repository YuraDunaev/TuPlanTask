namespace CadBridge.Core.Models;

public readonly struct BoundaryVertex : IEquatable<BoundaryVertex>
{
    public Point2D Point { get; }

    public double Bulge { get; }

    public BoundaryVertex(Point2D point, double bulge)
    {
        if (!double.IsFinite(bulge))
            throw new ArgumentOutOfRangeException(nameof(bulge), "Bulge must be a finite number.");
        Point = point;
        Bulge = bulge;
    }

    public BoundaryVertex(double x, double y, double bulge)
        : this(new Point2D(x, y), bulge)
    {
    }

    public bool IsArc => Bulge != 0.0;

    public bool Equals(BoundaryVertex other) => Point.Equals(other.Point) && Bulge.Equals(other.Bulge);
    public override bool Equals(object? obj) => obj is BoundaryVertex other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Point, Bulge);

    public static bool operator ==(BoundaryVertex left, BoundaryVertex right) => left.Equals(right);
    public static bool operator !=(BoundaryVertex left, BoundaryVertex right) => !left.Equals(right);

    public override string ToString() => IsArc
        ? FormattableString.Invariant($"{Point} bulge={Bulge}")
        : Point.ToString();
}

