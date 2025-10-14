namespace BuildingBlocks.AI.AgentFramework;

public class AgentFrameworkOptions
{
    public string EmbeddingModel { get; set; } = default!;
    public string EmbeddingEndpoint { get; set; } = default!;
    public string ChatModel { get; set; } = default!;
    public string ChatApiVersion { get; set; } = default!;
    public string ChatEndpoint { get; set; } = default!;
    public string ChatDeploymentName { get; set; } = default!;
    public string ChatApiKey { get; set; } = default!;
    public string EmbeddingApiKey { get; set; } = default!;
    public string EmbeddingApiVersion { get; set; } = default!;
    public string EmbeddingDeploymentName { get; set; } = default!;
    public ProviderType ChatProviderType { get; set; } = ProviderType.Ollama;
    public ProviderType EmbeddingProviderType { get; set; } = ProviderType.Ollama;
    public float Temperature { get; set; } = 0.8f;
    public double? SearchThreshold { get; set; }
    public int MaximumInvocationCount { get; set; }

    public Dictionary<string, object> ChatExtensionData { get; set; } = new();
}

public enum ProviderType
{
    Ollama,
    Azure,
    OpenAI,
}
