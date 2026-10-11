using AcDocument = Autodesk.AutoCAD.ApplicationServices.Document;
using CadBridge.AutoCAD.Drawing;
using CadBridge.AutoCAD.Reading;
using CadBridge.Core.Models;
using CadBridge.Core.Serialization;
using CadBridge.Core.Services.Analysis;
using CadBridge.Core.Services.Geometry;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System.IO;
using System.Linq;

namespace CadBridge.AutoCAD.Application;

public sealed class UpdateRoomsUseCase
{
    private const double ToleranceRatio = 0.01;
    private const short MarkerColorIndex = 1;
    private const short MarkerAlertColorIndex = 2;
    private static readonly LineWeight MarkerLineWeight = LineWeight.LineWeight200;
    private const double TextHeightCm = 25;
    private const double TextOffsetCm = 70;
    private const short TextColorIndex = 8;
    private const double CircleRadiusCm = 40;

    private readonly RoomMarkerStore _markers = new();

    public UpdateResult Execute(AcDocument document, string reportPath)
    {
        ArgumentNullException.ThrowIfNull(document);
        if (string.IsNullOrWhiteSpace(reportPath) || !File.Exists(reportPath))
            throw new FileNotFoundException("rooms_report.json was not found.", reportPath);

        var report = RoomsReportJsonSerializer.Deserialize(File.ReadAllText(reportPath));
        if (report is null)
            throw new InvalidDataException("Failed to parse rooms_report.json.");

        var readResult = new DwgRoomReader().Read(document);
        var labelByHandle = BuildLabelIndex(readResult);

        var marked = 0;
        var mismatched = 0;
        var missing = 0;
        var touchedIds = new HashSet<string>(System.StringComparer.Ordinal);

        var database = document.Database;
        using var tr = database.TransactionManager.StartTransaction();
        try
        {
            EnsureRegApp(database, tr);
            EnsureLayer(database, tr);

            var space = (BlockTableRecord)tr.GetObject(
                SymbolUtilityServices.GetBlockModelSpaceId(database), OpenMode.ForWrite);

            foreach (var room in report.Rooms)
            {
                if (!labelByHandle.TryGetValue(room.Id, out var label))
                {
                    if (room.CadContourValid)
                        missing++;
                    continue;
                }

                touchedIds.Add(room.Id);
                DrawText(tr, space, room, label);

                if (AreaDelta.Exceeds(room.RevitArea, room.CadArea, ToleranceRatio))
                {
                    DrawMarker(tr, space, room, label);
                    mismatched++;
                }

                marked++;
            }

            PruneMarkers(tr, space, touchedIds, RoomMarkerStore.KindText, RoomMarkerStore.KindMarker);

            new DwgIssueReporter(_markers).Report(tr, space, readResult.Warnings);

            tr.Commit();
        }
        catch (System.Exception)
        {
            tr.Abort();
            throw;
        }

        return new UpdateResult(marked, mismatched, missing);
    }

    private static Dictionary<string, Point2D> BuildLabelIndex(RoomReadResult readResult)
    {
        var index = new Dictionary<string, Point2D>(System.StringComparer.Ordinal);
        foreach (var draft in readResult.Rooms)
        {
            var handle = draft.Boundary.Handle;
            var label = draft.Tag?.Position
                ?? (LabelPointResolver.TryResolve(draft.Boundary.Boundary, out var resolved)
                    ? resolved
                    : (Point2D?)null);

            if (label is not null)
                index[handle] = label.Value;
        }

        return index;
    }

    private void DrawText(Transaction tr, BlockTableRecord space, RoomReport room, Point2D label)
    {
        var center = new Point3d(label.X, label.Y - TextOffsetCm, 0);
        var content = $"Revit: {room.RevitArea:F2} m²";

        var existing = FindOwned(tr, space, room.Id, RoomMarkerStore.KindText);
        if (existing is DBText text)
        {
            text.Position = center;
            text.AlignmentPoint = center;
            text.TextString = content;
            text.ColorIndex = TextColorIndex;
            return;
        }
        
        var dbText = new DBText
        {
            TextString = content,
            Height = TextHeightCm,
            Layer = RoomMarkerStore.LayerName,
            ColorIndex = TextColorIndex,
            HorizontalMode = TextHorizontalMode.TextCenter,
            VerticalMode = TextVerticalMode.TextVerticalMid,
            Position = center,
            AlignmentPoint = center,
        };

        space.AppendEntity(dbText);
        tr.AddNewlyCreatedDBObject(dbText, true);
        _markers.Mark(dbText, room.Id, RoomMarkerStore.KindText);
    }

    private void DrawMarker(Transaction tr, BlockTableRecord space, RoomReport room, Point2D label)
    {
        var cx = label.X;
        var cy = label.Y;
        var r = CircleRadiusCm;

        FindOwned(tr, space, room.Id, RoomMarkerStore.KindMarker)?.Erase();

        var diamond = BuildClosedPolyline(
            new Point2d(cx, cy + r),
            new Point2d(cx + r, cy),
            new Point2d(cx, cy - r),
            new Point2d(cx - r, cy));
        diamond.Layer = RoomMarkerStore.LayerName;
        diamond.ColorIndex = MarkerAlertColorIndex;
        diamond.LineWeight = MarkerLineWeight;

        space.AppendEntity(diamond);
        tr.AddNewlyCreatedDBObject(diamond, true);
        _markers.Mark(diamond, room.Id, RoomMarkerStore.KindMarker);
    }

    private static Polyline BuildClosedPolyline(params Point2d[] points)
    {
        var poly = new Polyline(points.Length);
        for (var i = 0; i < points.Length; i++)
            poly.AddVertexAt(i, points[i], 0, 0, 0);
        poly.Closed = true;
        return poly;
    }

    private Entity? FindOwned(Transaction tr, BlockTableRecord space, string sourceId, string kind)
    {
        foreach (ObjectId id in space)
        {
            if (id.IsErased || tr.GetObject(id, OpenMode.ForRead) is not Entity entity)
                continue;

            var mark = _markers.Read(entity);
            if (mark is { } m && m.SourceId == sourceId && m.Kind == kind)
                return (Entity)tr.GetObject(id, OpenMode.ForWrite);
        }

        return null;
    }

    private void PruneMarkers(Transaction tr, BlockTableRecord space, HashSet<string> touchedIds, params string[] kinds)
    {
        foreach (ObjectId id in space)
        {
            if (id.IsErased || tr.GetObject(id, OpenMode.ForRead) is not Entity entity)
                continue;

            var mark = _markers.Read(entity);
            if (mark is { } m && Array.IndexOf(kinds, m.Kind) >= 0 && !touchedIds.Contains(m.SourceId)
                && tr.GetObject(id, OpenMode.ForWrite) is Entity orphan)
                orphan.Erase();
        }
    }

    private static void EnsureRegApp(Database database, Transaction tr)
    {
        var regApps = (RegAppTable)tr.GetObject(database.RegAppTableId, OpenMode.ForRead);
        if (regApps.Has(RoomMarkerStore.RegAppName))
            return;

        regApps.UpgradeOpen();
        var record = new RegAppTableRecord { Name = RoomMarkerStore.RegAppName };
        regApps.Add(record);
        tr.AddNewlyCreatedDBObject(record, true);
    }

    private static void EnsureLayer(Database database, Transaction tr)
    {
        var layers = (LayerTable)tr.GetObject(database.LayerTableId, OpenMode.ForRead);
        if (layers.Has(RoomMarkerStore.LayerName))
            return;

        layers.UpgradeOpen();
        var record = new LayerTableRecord
        {
            Name = RoomMarkerStore.LayerName,
            Color = Color.FromColorIndex(ColorMethod.ByAci, MarkerColorIndex),
        };
        layers.Add(record);
        tr.AddNewlyCreatedDBObject(record, true);
    }
}

public readonly record struct UpdateResult(int Marked, int Mismatched, int Missing);
