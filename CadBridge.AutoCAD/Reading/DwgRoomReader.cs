using AcDocument = Autodesk.AutoCAD.ApplicationServices.Document;
using AcException = Autodesk.AutoCAD.Runtime.Exception;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using CadBridge.Core.Geometry;
using CadBridge.Core.Models;

namespace CadBridge.AutoCAD.Reading;

public sealed class DwgRoomReader
{
    public const string BoundaryLayer = "RZ_AREA";
    public const string TagBlockName = "RZ_AREA_SYM";

    public RoomReadResult Read(AcDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var warnings = new List<Warning>();
        var boundaries = ReadBoundaries(document, warnings);
        var tags = ReadTags(document, warnings);
        var rooms = Match(boundaries, tags, warnings);

        return new RoomReadResult(rooms, warnings);
    }

    private static List<BoundaryCandidate> ReadBoundaries(AcDocument document, List<Warning> warnings)
    {
        var filter = new SelectionFilter(new[]
        {
            new TypedValue((int)DxfCode.Start, "LWPOLYLINE"),
            new TypedValue((int)DxfCode.LayerName, BoundaryLayer),
            new TypedValue((int)DxfCode.LayoutName, "Model"),
        });

        var boundaries = new List<BoundaryCandidate>();
        using var tr = document.Database.TransactionManager.StartTransaction();

        foreach (var id in Select(document, filter))
        {
            try
            {
                if (tr.GetObject(id, OpenMode.ForRead) is not Polyline polyline)
                    continue;

                var handle = polyline.Handle.ToString();
                if (!polyline.Closed)
                {
                    warnings.Add(new Warning(
                        WarningCode.OpenPolyline, handle,
                        $"Polyline {handle} on {BoundaryLayer} is not closed."));
                    continue;
                }

                var vertices = ReadVertices(polyline);
                if (vertices.Count < 3)
                {
                    warnings.Add(new Warning(
                        WarningCode.CalculationFailed, handle,
                        $"Polyline {handle} has {vertices.Count} vertices — too few for a room."));
                    continue;
                }

                try
                {
                    boundaries.Add(new BoundaryCandidate(handle, new Boundary(vertices)));
                }
                catch (ArgumentException ex)
                {
                    warnings.Add(new Warning(
                        WarningCode.CalculationFailed, handle,
                        $"Polyline {handle} is degenerate: {ex.Message}"));
                }
            }
            catch (AcException ex)
            {
                warnings.Add(new Warning(
                    WarningCode.CalculationFailed, id.ToString(),
                    $"Cannot read polyline: {ex.Message}"));
            }
        }

        return boundaries;
    }

    private static List<BoundaryVertex> ReadVertices(Polyline polyline)
    {
        var vertices = new List<BoundaryVertex>(polyline.NumberOfVertices);
        for (var i = 0; i < polyline.NumberOfVertices; i++)
        {
            var point = polyline.GetPoint2dAt(i);
            vertices.Add(new BoundaryVertex(point.X, point.Y, polyline.GetBulgeAt(i)));
        }

        return vertices;
    }

    private static List<TagCandidate> ReadTags(AcDocument document, List<Warning> warnings)
    {
        var filter = new SelectionFilter(new[]
        {
            new TypedValue((int)DxfCode.Start, "INSERT"),
            new TypedValue((int)DxfCode.BlockName, TagBlockName),
            new TypedValue((int)DxfCode.LayerName, BoundaryLayer),
            new TypedValue((int)DxfCode.LayoutName, "Model"),
        });

        var tags = new List<TagCandidate>();
        using var tr = document.Database.TransactionManager.StartTransaction();

        foreach (var id in Select(document, filter))
        {
            try
            {
                if (tr.GetObject(id, OpenMode.ForRead) is not BlockReference block)
                    continue;

                var handle = block.Handle.ToString();
                var position = block.Position;
                var (number, name) = TagTextParser.Parse(ReadAttributes(tr, block));

                tags.Add(new TagCandidate(handle, new Point2D(position.X, position.Y), number, name));
            }
            catch (AcException ex)
            {
                warnings.Add(new Warning(
                    WarningCode.CalculationFailed, id.ToString(),
                    $"Cannot read tag block: {ex.Message}"));
            }
            catch (ArgumentException ex)
            {
                warnings.Add(new Warning(
                    WarningCode.CalculationFailed, id.ToString(),
                    $"Tag block has invalid position: {ex.Message}"));
            }
        }

        return tags;
    }

    private static List<(string Tag, string Value)> ReadAttributes(Transaction tr, BlockReference block)
    {
        var attributes = new List<(string Tag, string Value)>();
        foreach (ObjectId attributeId in block.AttributeCollection)
        {
            if (tr.GetObject(attributeId, OpenMode.ForRead) is AttributeReference attribute)
                attributes.Add((attribute.Tag, attribute.TextString ?? string.Empty));
        }

        return attributes;
    }

    private static List<RoomDraft> Match(
        List<BoundaryCandidate> boundaries,
        List<TagCandidate> tags,
        List<Warning> warnings)
    {
        var rooms = new List<RoomDraft>(boundaries.Count);
        var claimed = new HashSet<string>(StringComparer.Ordinal);

        foreach (var boundary in boundaries)
        {
            var inside = new List<TagCandidate>();
            foreach (var tag in tags)
            {
                if (claimed.Contains(tag.Handle))
                    continue;

                if (PointInPolygon.IsInside(tag.Position, boundary.Boundary))
                    inside.Add(tag);
            }

            foreach (var tag in inside)
                claimed.Add(tag.Handle);

            if (inside.Count == 0)
            {
                warnings.Add(new Warning(
                    WarningCode.NoTag, boundary.Handle,
                    $"Room {boundary.Handle} has no tag block inside."));
                rooms.Add(new RoomDraft(boundary, null));
            }
            else
            {
                if (inside.Count > 1)
                {
                    warnings.Add(new Warning(
                        WarningCode.MultipleTags, boundary.Handle,
                        $"Room {boundary.Handle} has {inside.Count} tag blocks; using {inside[0].Handle}."));
                }

                rooms.Add(new RoomDraft(boundary, inside[0]));
            }
        }

        foreach (var tag in tags)
        {
            if (!claimed.Contains(tag.Handle))
            {
                warnings.Add(new Warning(
                    WarningCode.OrphanTag, tag.Handle,
                    $"Tag {tag.Handle} lies outside any room boundary."));
            }
        }

        return rooms;
    }

    private static ObjectId[] Select(AcDocument document, SelectionFilter filter)
    {
        try
        {
            var selection = document.Editor.SelectAll(filter);
            return selection?.Value?.GetObjectIds() ?? Array.Empty<ObjectId>();
        }
        catch (AcException)
        {
            return Array.Empty<ObjectId>();
        }
    }
}

