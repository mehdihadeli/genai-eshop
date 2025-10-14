using Humanizer;

namespace BuildingBlocks.Constants;

public static class AspireResources
{
    public static readonly string Postgres = nameof(Postgres).Kebaberize();
    public static readonly string Redis = nameof(Redis).Kebaberize();
    public static readonly string Qdrant = nameof(Qdrant).Kebaberize();
    public static readonly string Ollama = nameof(Ollama).Kebaberize();
    public static readonly string OllamaChat = $"{nameof(OllamaChat).Kebaberize()}";
    public static readonly string OllamaEmbedding = $"{nameof(OllamaEmbedding).Kebaberize()}";
    public static readonly string McpInspector = $"{nameof(McpInspector).Kebaberize()}";
}
