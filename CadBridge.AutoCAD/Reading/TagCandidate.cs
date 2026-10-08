using CadBridge.Core.Models;

namespace CadBridge.AutoCAD.Reading;

public sealed record TagCandidate(string Handle, Point2D Position, string Number, string Name);

