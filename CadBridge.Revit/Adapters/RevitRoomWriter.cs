using Autodesk.Revit.DB;
using CadBridge.Core.Models;
using CadBridge.Core.Services.Geometry;

namespace CadBridge.Revit.Adapters;

public sealed class RevitRoomWriter
{
    private readonly Document _document;
    private readonly Level _level;
    private readonly SourceIdStore _sourceIdStore = new();

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
            transaction.Commit();
            return room;
        }
        catch (System.Exception ex)
        {
            transaction.RollBack();
            throw new InvalidOperationException($"Не удалось создать помещение {domainRoom}: {ex.Message}", ex);
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
            DeleteSeparationLines(existing);

            CreateSeparationLines(domainRoom.Boundary);

            if (!string.IsNullOrWhiteSpace(domainRoom.Number))
                existing.Number = domainRoom.Number;
            if (!string.IsNullOrWhiteSpace(domainRoom.Name))
                existing.Name = domainRoom.Name;

            _sourceIdStore.SetSourceId(existing, domainRoom.Id.Value);

            transaction.Commit();
        }
        catch (System.Exception ex)
        {
            transaction.RollBack();
            throw new InvalidOperationException($"Не удалось обновить помещение {domainRoom}: {ex.Message}", ex);
        }
    }

    private Autodesk.Revit.DB.Architecture.Room CreateCore(Room domainRoom)
    {
        CreateSeparationLines(domainRoom.Boundary);

        var label = domainRoom.LabelPoint;
        var location = new UV(CmToFt(label.X), CmToFt(label.Y));
        var room = _document.Create.NewRoom(_level, location);
        if (room is null)
            throw new InvalidOperationException(
                $"Room не создан: точка label вне границы (или граница не замкнута). id={domainRoom.Id}");

        if (!string.IsNullOrWhiteSpace(domainRoom.Number))
            room.Number = domainRoom.Number;
        if (!string.IsNullOrWhiteSpace(domainRoom.Name))
            room.Name = domainRoom.Name;

        _sourceIdStore.SetSourceId(room, domainRoom.Id.Value);
        return room;
    }

    private void CreateSeparationLines(Boundary boundary)
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

        _document.Create.NewRoomBoundaryLines(sketchPlane, curves, view);
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

    private void DeleteSeparationLines(Autodesk.Revit.DB.Architecture.Room room)
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
                if (element?.Category?.Id.Value == (int)BuiltInCategory.OST_RoomSeparationLines)
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
