using BuildingBlocks.Env;

namespace GenAIEshop.BuildingBlocks.UnitTests;

public sealed class DotEnvTests
{
    [Fact]
    public void Load_reads_values_without_overwriting_existing_environment_variables()
    {
        var key = $"GENAI_TEST_{Guid.NewGuid():N}";
        var filePath = Path.Combine(Path.GetTempPath(), $"genai-{Guid.NewGuid():N}.env");

        try
        {
            File.WriteAllText(filePath, $"{key}=from-file{Environment.NewLine}");
            Environment.SetEnvironmentVariable(key, "from-process");

            DotEnv.Load(filePath);

            Environment.GetEnvironmentVariable(key).ShouldBe("from-process");
        }
        finally
        {
            Environment.SetEnvironmentVariable(key, null);
            File.Delete(filePath);
        }
    }
}
