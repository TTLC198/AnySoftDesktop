using System.Text.Json;

namespace AnySoftDesktop.Utils;

public static class CustomJsonSerializerOptions
{
    public static JsonSerializerOptions Options = new ()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };
}