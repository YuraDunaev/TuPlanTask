namespace CadBridge.Core.Models;

public sealed class Warning
{
    public WarningCode Code { get; }

    public string EntityId { get; }

    public string Message { get; }

    public Warning(WarningCode code, string? entityId, string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("A warning must explain itself.", nameof(message));

        Code = code;
        EntityId = entityId ?? string.Empty;
        Message = message;
    }

    public override string ToString() => $"{Code}: {Message} [{EntityId}]";
}

