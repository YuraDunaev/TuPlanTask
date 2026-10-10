using System.Text.Json;
using System.Text.Json.Serialization;
using CadBridge.Core.Models;

namespace CadBridge.Core.Serialization;

public static class RoomsReportJsonSerializer
{
    public static readonly JsonSerializerOptions Options = CreateOptions();

    private static JsonSerializerOptions CreateOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        };
    }

    public static string Serialize(RoomsReportDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        return JsonSerializer.Serialize(document, Options);
    }
}
