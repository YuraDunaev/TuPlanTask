namespace CadBridge.Core.Models;

public sealed record SourceInfo(string Application, string File, DateTimeOffset ExportedAt, string Units);

