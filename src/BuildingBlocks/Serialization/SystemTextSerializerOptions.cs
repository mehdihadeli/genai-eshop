using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace BuildingBlocks.Serialization;

public static class SystemTextJsonSerializerOptions
{
    public static JsonSerializerOptions DefaultSerializerOptions { get; } = CreateDefaultSerializerOptions();

    public static JsonSerializerOptions CreateDefaultSerializerOptions(bool camelCase = true, bool indented = true)
    {
        var options = new JsonSerializerOptions
        {
            IncludeFields = true,
            PropertyNameCaseInsensitive = true,
            // Equivalent to ReferenceLoopHandling.Ignore
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            WriteIndented = indented,
            PropertyNamingPolicy = camelCase ? JsonNamingPolicy.CamelCase : null,
        };

        options.Converters.Add(new JsonStringEnumConverter());

        return options;
    }

    public static JsonSerializerOptions SetDefaultOptions(JsonSerializerOptions jsonSerializerOptions)
    {
        // Copy the settings from DefaultSerializerOptions
        var defaultOptions = DefaultSerializerOptions;

        jsonSerializerOptions.IncludeFields = defaultOptions.IncludeFields;
        jsonSerializerOptions.PropertyNameCaseInsensitive = defaultOptions.PropertyNameCaseInsensitive;
        jsonSerializerOptions.WriteIndented = defaultOptions.WriteIndented;
        jsonSerializerOptions.ReferenceHandler = defaultOptions.ReferenceHandler;
        jsonSerializerOptions.PropertyNamingPolicy = defaultOptions.PropertyNamingPolicy;

        // Add converters from DefaultSerializerOptions
        foreach (var converter in defaultOptions.Converters)
        {
            jsonSerializerOptions.Converters.Add(converter);
        }

        return jsonSerializerOptions;
    }
}
