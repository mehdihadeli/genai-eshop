using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

#pragma warning disable SKEXP0001

namespace BuildingBlocks.AI.AgentFramework;

public static class ChatOptionsDefaults
{
    public static ChatOptions GetDefaultChatOptions(AgentFrameworkOptions agentFrameworkOptions)
    {
        // https://ollama.com/blog/thinking
        // - in ollama cli we can `ollama run qwen3:0.6b --think=false` to turn off thinking
        // - in ollama api with passing `think=false` as parameter
        var defaultExtensionData = new Dictionary<string, object>
        {
            { "think", false },
            { "temperature", agentFrameworkOptions.Temperature },
            { "num_predict", 10000 },
            { "max_tokens", 10000 },
        };

        var mergedExtensionData = MergeExtensionData(defaultExtensionData, agentFrameworkOptions.ChatExtensionData);

        return new ChatOptions
        {
            // https://learn.microsoft.com/en-us/dotnet/ai/microsoft-extensions-ai#tool-calling
            ToolMode = ChatToolMode.Auto,
            AllowMultipleToolCalls = false,
            MaxOutputTokens = 10000,
            AdditionalProperties = new AdditionalPropertiesDictionary(mergedExtensionData),
        };
    }

    public static ChatClientAgentRunOptions GetDefaultAgentRunOptions(AgentFrameworkOptions agentFrameworkOptions)
    {
        return new ChatClientAgentRunOptions { ChatOptions = GetProviderChatOptions(agentFrameworkOptions) };
    }

    public static ChatOptions GetProviderChatOptions(AgentFrameworkOptions agentFrameworkOptions)
    {
        switch (agentFrameworkOptions.ChatProviderType)
        {
            case ProviderType.Ollama:
                // https://ollama.com/blog/thinking
                // - in ollama cli we can `ollama run qwen3:0.6b --think=false` to turn off thinking
                // - in ollama api with passing `think=false` as parameter
                var ollamaDefaultExtensionData = new Dictionary<string, object>
                {
                    { "think", false },
                    { "temperature", agentFrameworkOptions.Temperature },
                    { "num_predict", 10000 },
                };
                var ollamaMergedExtensionData = MergeExtensionData(
                    ollamaDefaultExtensionData,
                    agentFrameworkOptions.ChatExtensionData
                );

                return new ChatOptions
                {
                    // https://learn.microsoft.com/en-us/dotnet/ai/microsoft-extensions-ai#tool-calling
                    ToolMode = ChatToolMode.Auto,
                    AllowMultipleToolCalls = false,
                    Temperature = agentFrameworkOptions.Temperature,
                    MaxOutputTokens = 10000,
                    AdditionalProperties = new AdditionalPropertiesDictionary(ollamaMergedExtensionData),
                };
            case ProviderType.Azure:
                var azureDefaultExtensionData = new Dictionary<string, object> { };
                var azureMergedExtensionData = MergeExtensionData(
                    azureDefaultExtensionData,
                    agentFrameworkOptions.ChatExtensionData
                );
                return new ChatOptions
                {
                    // https://learn.microsoft.com/en-us/dotnet/ai/microsoft-extensions-ai#tool-calling
                    ToolMode = ChatToolMode.Auto,
                    AllowMultipleToolCalls = false,
                    Temperature = agentFrameworkOptions.Temperature,
                    AdditionalProperties = new AdditionalPropertiesDictionary(azureMergedExtensionData),
                    MaxOutputTokens = 10000,
                };
            case ProviderType.OpenAI:
                var openAIDefaultExtensionData = new Dictionary<string, object> { };
                var openAIMergedExtensionData = MergeExtensionData(
                    openAIDefaultExtensionData,
                    agentFrameworkOptions.ChatExtensionData
                );
                return new ChatOptions
                {
                    // https://learn.microsoft.com/en-us/dotnet/ai/microsoft-extensions-ai#tool-calling
                    ToolMode = ChatToolMode.Auto,
                    AllowMultipleToolCalls = false,
                    Temperature = agentFrameworkOptions.Temperature,
                    AdditionalProperties = new AdditionalPropertiesDictionary(openAIMergedExtensionData),
                    MaxOutputTokens = 10000,
                };
            default:
                throw new Exception("Unknown provider type");
        }
    }

    private static Dictionary<string, object?> MergeExtensionData(
        Dictionary<string, object> baseExtensionData,
        Dictionary<string, object> appSettingsExtensionData
    )
    {
        var merged = new Dictionary<string, object?>(baseExtensionData);

        foreach (var item in appSettingsExtensionData)
        {
            merged[item.Key] = item.Value;
        }

        return merged;
    }
}
