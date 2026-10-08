using CadBridge.Core.Models;

namespace CadBridge.AutoCAD.Reading;

public sealed record RoomReadResult(IReadOnlyList<RoomDraft> Rooms, IReadOnlyList<Warning> Warnings);

