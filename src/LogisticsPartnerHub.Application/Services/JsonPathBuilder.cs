using System.Text.Json;
using System.Text.Json.Nodes;

namespace LogisticsPartnerHub.Application.Services;

public static class JsonPathBuilder
{
    public static void SetValue(JsonObject root, string targetPath, JsonNode? value)
    {
        var segments = ParseSegments(targetPath);
        if (segments.Length == 0) return;

        var current = root;

        for (var i = 0; i < segments.Length - 1; i++)
        {
            var segment = segments[i];

            if (current[segment] is JsonObject existingObj)
            {
                current = existingObj;
            }
            else
            {
                var newObj = new JsonObject();
                current[segment] = newObj;
                current = newObj;
            }
        }

        current[segments[^1]] = value;
    }

    public static JsonNode? ExtractValue(JsonNode source, string sourcePath)
    {
        var segments = ParseSegments(sourcePath);
        JsonNode? current = source;

        foreach (var segment in segments)
        {
            if (current is JsonObject obj)
            {
                current = obj[segment];
                if (current is null) return null;
            }
            else
            {
                return null;
            }
        }

        return current?.DeepClone();
    }

    public static JsonNode? ParseDefaultValue(string defaultValue)
    {
        try
        {
            return JsonNode.Parse(defaultValue);
        }
        catch (JsonException)
        {
            return JsonValue.Create(defaultValue);
        }
    }

    private static string[] ParseSegments(string path)
    {
        var normalized = path.StartsWith("$.") ? path[2..] : path;
        return normalized.Split('.', StringSplitOptions.RemoveEmptyEntries);
    }
}
