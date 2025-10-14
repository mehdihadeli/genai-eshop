using Microsoft.Extensions.Configuration;

namespace BuildingBlocks.Extensions;

public static class ConfigurationExtensions
{
    public static TOptions BindOptions<TOptions>(
        this IConfiguration configuration,
        string section,
        Action<TOptions>? configurator = null
    )
        where TOptions : new()
    {
        var options = new TOptions();

        var optionsSection = configuration.GetSection(section);
        optionsSection.Bind(options);

        configurator?.Invoke(options);

        return options;
    }

    public static TOptions BindOptions<TOptions>(
        this IConfiguration configuration,
        Action<TOptions>? configurator = null
    )
        where TOptions : new()
    {
        return BindOptions(configuration, typeof(TOptions).Name, configurator);
    }
}
