namespace GenAIEshop.Tests.Shared;

public sealed class TestConfigurations : Dictionary<string, string?>
{
    public TestConfigurations Add(string key, object? value)
    {
        this[key] = value?.ToString();
        return this;
    }
}
