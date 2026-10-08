namespace CadBridge.Core.Models;

public sealed class Boundary : IEquatable<Boundary>
{
    private readonly BoundaryVertex[] _vertices;

    public Boundary(IEnumerable<BoundaryVertex> vertices)
    {
        ArgumentNullException.ThrowIfNull(vertices);

        var list = vertices as BoundaryVertex[] ?? vertices.ToArray();

        if (list.Length >= 2 && list[0].Point == list[^1].Point)
            list = list[..^1];

        if (list.Length < 3)
            throw new ArgumentException("A boundary needs at least 3 distinct vertices.", nameof(vertices));

        _vertices = list;
    }

    public IReadOnlyList<BoundaryVertex> Vertices => _vertices;

    public int Count => _vertices.Length;

    public BoundaryVertex this[int index] => _vertices[index];

    public bool Equals(Boundary? other)
    {
        if (other is null || other.Count != Count)
            return false;
        for (var i = 0; i < Count; i++)
        {
            if (!_vertices[i].Equals(other._vertices[i]))
                return false;
        }

        return true;
    }

    public override bool Equals(object? obj) => obj is Boundary other && Equals(other);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(_vertices.Length);
        foreach (var vertex in _vertices)
            hash.Add(vertex);
        return hash.ToHashCode();
    }

    public override string ToString() => $"Boundary({Count} vertices)";
}

