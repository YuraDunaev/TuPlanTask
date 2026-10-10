using System.Linq;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using CadBridge.Core.Models;

namespace CadBridge.AutoCAD.Drawing;

public sealed class DwgIssueReporter
{
    public const string LayerName = "A-ROOM-ISSUE";
    private const short IssueColorIndex = 30;
    private const double TextHeightCm = 20;
    private const double TextOffsetCm = 50;
    private const double CircleRadiusCm = 30;
    private const string IssueKind = "issue";
    private const string IssueLabelKind = "issuelabel";

    private readonly RoomMarkerStore _markers;

    public DwgIssueReporter(RoomMarkerStore markers)
    {
        _markers = markers ?? throw new ArgumentNullException(nameof(markers));
    }

    public void Report(Transaction tr, BlockTableRecord space, IReadOnlyList<Warning> warnings)
    {
        ArgumentNullException.ThrowIfNull(tr);
        ArgumentNullException.ThrowIfNull(space);
        ArgumentNullException.ThrowIfNull(warnings);

        EnsureLayer(space.Database, tr);

        var positions = BuildPositionIndex(tr, space);
        var touched = new HashSet<string>(System.StringComparer.Ordinal);

        foreach (var warning in warnings)
        {
            if (string.IsNullOrWhiteSpace(warning.EntityId))
                continue;
            if (!positions.TryGetValue(warning.EntityId, out var at))
                continue;

            var key = $"{warning.EntityId}:{warning.Code}";
            touched.Add($"{key}|{IssueKind}");
            touched.Add($"{key}|{IssueLabelKind}");
            DrawIssue(tr, space, key, ShortLabel(warning.Code), at);
        }

        PruneIssues(tr, space, touched);
    }

    private static Dictionary<string, Point2D> BuildPositionIndex(Transaction tr, BlockTableRecord space)
    {
        var index = new Dictionary<string, Point2D>(System.StringComparer.Ordinal);
        foreach (ObjectId id in space)
        {
            if (id.IsErased)
                continue;
            if (tr.GetObject(id, OpenMode.ForRead) is not Entity entity)
                continue;
            if (PositionOf(entity) is { } pos)
                index[entity.Handle.ToString()] = pos;
        }

        return index;
    }

    private static Point2D? PositionOf(Entity entity)
    {
        switch (entity)
        {
            case Polyline polyline when polyline.NumberOfVertices > 0:
                double sumX = 0;
                double sumY = 0;
                for (var i = 0; i < polyline.NumberOfVertices; i++)
                {
                    var point = polyline.GetPoint2dAt(i);
                    sumX += point.X;
                    sumY += point.Y;
                }
                return new Point2D(sumX / polyline.NumberOfVertices, sumY / polyline.NumberOfVertices);
            case BlockReference block:
                return new Point2D(block.Position.X, block.Position.Y);
            case DBText text:
                return new Point2D(text.Position.X, text.Position.Y);
            default:
                return null;
        }
    }

    private void DrawIssue(Transaction tr, BlockTableRecord space, string key, string label, Point2D at)
    {
        var center = new Point3d(at.X, at.Y, 0);

        if (FindOwned(tr, space, key, IssueKind) is Circle existingCircle)
        {
            existingCircle.Center = center;
        }
        else
        {
            var circle = new Circle(center, Vector3d.ZAxis, CircleRadiusCm)
            {
                Layer = LayerName,
                ColorIndex = IssueColorIndex,
                LineWeight = LineWeight.LineWeight100,
            };
            space.AppendEntity(circle);
            tr.AddNewlyCreatedDBObject(circle, true);
            _markers.Mark(circle, key, IssueKind);
        }

        var labelPosition = new Point3d(at.X, at.Y + TextOffsetCm, 0);
        if (FindOwned(tr, space, key, IssueLabelKind) is DBText existingText)
        {
            existingText.Position = labelPosition;
            existingText.TextString = label;
        }
        else
        {
            var text = new DBText
            {
                Position = labelPosition,
                TextString = label,
                Height = TextHeightCm,
                Layer = LayerName,
                ColorIndex = IssueColorIndex,
                HorizontalMode = TextHorizontalMode.TextCenter,
                VerticalMode = TextVerticalMode.TextVerticalMid,
            };
            space.AppendEntity(text);
            tr.AddNewlyCreatedDBObject(text, true);
            _markers.Mark(text, key, IssueLabelKind);
        }
    }

    private Entity? FindOwned(Transaction tr, BlockTableRecord space, string sourceId, string kind)
    {
        foreach (ObjectId id in space)
        {
            if (id.IsErased)
                continue;
            if (tr.GetObject(id, OpenMode.ForRead) is not Entity entity)
                continue;

            var mark = _markers.Read(entity);
            if (mark is { } m && m.SourceId == sourceId && m.Kind == kind)
                return (Entity)tr.GetObject(id, OpenMode.ForWrite);
        }

        return null;
    }

    private void PruneIssues(Transaction tr, BlockTableRecord space, HashSet<string> touched)
    {
        foreach (ObjectId id in space)
        {
            if (id.IsErased)
                continue;
            if (tr.GetObject(id, OpenMode.ForRead) is not Entity entity)
                continue;

            var mark = _markers.Read(entity);
            if (mark is not { } m)
                continue;
            if (m.Kind != IssueKind && m.Kind != IssueLabelKind)
                continue;
            if (touched.Contains($"{m.SourceId}|{m.Kind}"))
                continue;

            if (tr.GetObject(id, OpenMode.ForWrite) is Entity orphan)
                orphan.Erase();
        }
    }

    private static void EnsureLayer(Database database, Transaction tr)
    {
        var layers = (LayerTable)tr.GetObject(database.LayerTableId, OpenMode.ForRead);
        if (layers.Has(LayerName))
            return;

        layers.UpgradeOpen();
        var record = new LayerTableRecord
        {
            Name = LayerName,
            Color = Color.FromColorIndex(ColorMethod.ByAci, IssueColorIndex),
        };
        layers.Add(record);
        tr.AddNewlyCreatedDBObject(record, true);
    }

    private static string ShortLabel(WarningCode code) => code switch
    {
        WarningCode.OpenPolyline => "OPEN",
        WarningCode.NoTag => "NO TAG",
        WarningCode.MultipleTags => "TAGS x2",
        WarningCode.OrphanTag => "ORPHAN",
        WarningCode.CalculationFailed => "CALC",
        _ => "ISSUE",
    };
}
