using System.Text.Json;
using System.Text.Json.Serialization;

namespace AnySoftDesktop.Utils;

public static class CustomJsonSerializerOptions
{
    public static JsonSerializerOptions Options = new ()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
}