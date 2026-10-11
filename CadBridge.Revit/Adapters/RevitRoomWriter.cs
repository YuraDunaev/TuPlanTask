using Autodesk.Revit.DB;
using CadBridge.Core.Models;
using CadBridge.Core.Services.Geometry;

namespace CadBridge.Revit.Adapters;

public sealed class RevitRoomWriter
{
    private readonly Document _document;
    private readonly Level _level;
    private readonly SourceIdStore _sourceIdStore = new();
    private readonly SeparationLineStore _separationLineStore = new();

    public RevitRoomWriter(Document document, Level level)
    {
        _document = document;
        _level = level;
    }

    public Autodesk.Revit.DB.Architecture.Room Create(Room domainRoom)
    {
        ArgumentNullException.ThrowIfNull(domainRoom);

        using var transaction = new Transaction(_document, "CadBridge.CreateRoom");
        transaction.Start();
        try
        {
            var room = CreateCore(domainRoom);
            var status = transaction.Commit();
            if (status != TransactionStatus.Committed)
                throw new InvalidOperationException(
                    $"Create transaction for room {domainRoom} was not committed (status: {status}).");

            return room;
        }
        catch (System.Exception ex)
        {
            if (transaction.GetStatus() == TransactionStatus.Started)
                transaction.RollBack();
            throw new InvalidOperationException($"Failed to create room {domainRoom}: {ex.Message}", ex);
        }
    }

    public void Update(Autodesk.Revit.DB.Architecture.Room existing, Room domainRoom)
    {
        ArgumentNullException.ThrowIfNull(existing);
        ArgumentNullException.ThrowIfNull(domainRoom);

        using var transaction = new Transaction(_document, "CadBridge.UpdateRoom");
        transaction.Start();
        try
        {
            DeleteSeparationLines(existing, domainRoom.Id.Value);

            CreateSeparationLines(domainRoom.Boundary, domainRoom.Id.Value);

            if (!string.IsNullOrWhiteSpace(domainRoom.Number))
                existing.Number = domainRoom.Number;
            if (!string.IsNullOrWhiteSpace(domainRoom.Name))
                existing.Name = domainRoom.Name;

            _sourceIdStore.SetSourceId(existing, domainRoom.Id.Value);
            _sourceIdStore.SetCadArea(existing, domainRoom.Area);

            var status = transaction.Commit();
            if (status != TransactionStatus.Committed)
                throw new InvalidOperationException(
                    $"Update transaction for room {domainRoom} was not committed (status: {status}).");
        }
        catch (System.Exception ex)
        {
            if (transaction.GetStatus() == TransactionStatus.Started)
                transaction.RollBack();
            throw new InvalidOperationException($"Failed to update room {domainRoom}: {ex.Message}", ex);
        }
    }

    private Autodesk.Revit.DB.Architecture.Room CreateCore(Room domainRoom)
    {
        CreateSeparationLines(domainRoom.Boundary, domainRoom.Id.Value);

        var label = domainRoom.LabelPoint;
        var location = new UV(CmToFt(label.X), CmToFt(label.Y));
        var room = _document.Create.NewRoom(_level, location);
        if (room is null)
            throw new InvalidOperationException(
                $"Room not created: label point is outside the boundary (or the boundary is not closed). id={domainRoom.Id}");

        if (!string.IsNullOrWhiteSpace(domainRoom.Number))
            room.Number = domainRoom.Number;
        if (!string.IsNullOrWhiteSpace(domainRoom.Name))
            room.Name = domainRoom.Name;

        _sourceIdStore.SetSourceId(room, domainRoom.Id.Value);
        _sourceIdStore.SetCadArea(room, domainRoom.Area);
        return room;
    }

    private void CreateSeparationLines(Boundary boundary, string ownerSourceId)
    {
        var view = FindPlanView();
        var plane = Plane.CreateByNormalAndOrigin(XYZ.BasisZ, new XYZ(0, 0, _level.Elevation));
        var sketchPlane = SketchPlane.Create(_document, plane);

        var curves = new CurveArray();
        var vertices = boundary.Vertices;
        for (var i = 0; i < vertices.Count; i++)
        {
            var current = vertices[i];
            var next = vertices[(i + 1) % vertices.Count];
            curves.Append(BuildCurve(current, next));
        }

        var created = _document.Create.NewRoomBoundaryLines(sketchPlane, curves, view);
        if (created is null)
            return;

        foreach (ModelCurve curve in created)
            _separationLineStore.Mark(curve, ownerSourceId);
    }

    private Curve BuildCurve(BoundaryVertex current, BoundaryVertex next)
    {
        var z = _level.Elevation;
        var from = ToXyz(current.Point, z);
        var to = ToXyz(next.Point, z);

        if (current.Bulge == 0.0)
            return Line.CreateBound(from, to);

        var arc = BulgeArc.Create(current.Point, next.Point, current.Bulge);
        var midAngle = arc.StartAngle + arc.Sweep / 2.0;
        var mid = new Point2D(
            arc.Center.X + arc.Radius * Math.Cos(midAngle),
            arc.Center.Y + arc.Radius * Math.Sin(midAngle));

        return Arc.Create(from, to, ToXyz(mid, z));
    }

    private void DeleteSeparationLines(Autodesk.Revit.DB.Architecture.Room room, string ownerSourceId)
    {
        var options = new SpatialElementBoundaryOptions
        {
            SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Center,
        };

        var ids = new HashSet<ElementId>();
        foreach (var loop in room.GetBoundarySegments(options))
        {
            foreach (var segment in loop)
            {
                var id = segment.ElementId;
                if (id == ElementId.InvalidElementId)
                    continue;

                var element = _document.GetElement(id);
                if (element?.Category?.Id.Value != (int)BuiltInCategory.OST_RoomSeparationLines)
                    continue;

                if (_separationLineStore.GetOwner(element) == ownerSourceId)
                    ids.Add(id);
            }
        }

        foreach (var id in ids)
            _document.Delete(id);
    }

    private View FindPlanView()
    {
        var views = new FilteredElementCollector(_document)
            .OfClass(typeof(ViewPlan))
            .Cast<ViewPlan>()
            .Where(v => !v.IsTemplate)
            .ToList();

        var view = views.FirstOrDefault(v => v.GenLevel is not null && v.GenLevel.Id == _level.Id)
                   ?? views.FirstOrDefault(v => v.ViewType == ViewType.FloorPlan)
                   ?? views.FirstOrDefault();

        return view ?? _document.ActiveView;
    }

    private static double CmToFt(double centimeters) =>
        UnitUtils.ConvertToInternalUnits(centimeters, UnitTypeId.Centimeters);

    private XYZ ToXyz(Point2D point, double z) =>
        new(CmToFt(point.X), CmToFt(point.Y), z);
}
