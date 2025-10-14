using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using BuildingBlocks.AI.AgentFramework;
using BuildingBlocks.Serialization;
using BuildingBlocks.Types;
using BuildingBlocks.VectorDB.Contracts;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.VectorData;

namespace BuildingBlocks.VectorDB.SearchServices;

public sealed class SemanticSearch(
    IChatClient chatClient,
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
    VectorStore vectorStore,
    IOptions<AgentFrameworkOptions> AgentFrameworkOptions
) : ISemanticSearch
{
    private readonly AgentFrameworkOptions _agentFrameworkOptions = AgentFrameworkOptions.Value;

    private const string InitPrompt =
        @"
You are an intelligent, friendly GenAI-Eshop assistant designed to help users understand their search results. 

Follow these guidelines in every response:
- Always respond in a warm, professional, and conversational tone.
- Clearly explain how the results relate to the user’s search intent.
- When items are found, highlight 2–3 key details per item (e.g., name, description, price, standout feature) and briefly compare them to aid decision-making.
- If no items match, empathetically explain the lack of results and suggest trying alternative terms.
- Never mention internal processes, AI, or technical terms—keep it shopper-focused.
- Do NOT retain or reference past interactions; treat each query as independent.
- Keep responses concise: 1–2 short paragraphs max.
";

    // https://learn.microsoft.com/en-us/semantic-kernel/concepts/vector-store-connectors/hybrid-search?pivots=programming-language-csharp
    // https://devblogs.microsoft.com/semantic-kernel/announcing-hybrid-search-with-semantic-kernel-for-net/
    public async Task<VectorSearchResult<TEntity>> HybridSearchAsync<TVectorEntity, TEntity>(
        string searchTerm,
        ICollection<string> keywords,
        DbContext dbContext,
        int take = 5,
        int skip = 0,
        double? threshold = null,
        Expression<Func<TVectorEntity, bool>>? filter = null,
        Expression<Func<TVectorEntity, object?>>? fullTextSearchFiled = null,
        CancellationToken cancellationToken = default
    )
        where TVectorEntity : VectorEntityBase
        where TEntity : Entity
    {
        // https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/embedding-generation/?tabs=csharp-Ollama&pivots=programming-language-csharp#using-text-embedding-generation-services
        var vector = await embeddingGenerator.GenerateVectorAsync(searchTerm, cancellationToken: cancellationToken);

        var collection = vectorStore.GetCollection<Guid, TVectorEntity>(typeof(TVectorEntity).Name.Underscore());
        await collection.EnsureCollectionExistsAsync(cancellationToken);

        // https://learn.microsoft.com/en-us/semantic-kernel/concepts/vector-store-connectors/hybrid-search?pivots=programming-language-csharp#top-and-skip
        var options = new HybridSearchOptions<TVectorEntity>
        {
            VectorProperty = r => r.Vector,
            // field to search on the full-text search column with `IsFullTextIndexed=true` attribute
            AdditionalProperty = fullTextSearchFiled ?? (r => r.Description),
            Skip = skip,
            IncludeVectors = false,
            // https://learn.microsoft.com/en-us/semantic-kernel/concepts/vector-store-connectors/vector-search?pivots=programming-language-csharp#filter
            Filter = filter,
        };

        var vectorCollection = (IKeywordHybridSearchable<TVectorEntity>)collection;
        // https://learn.microsoft.com/en-us/semantic-kernel/concepts/vector-store-connectors/hybrid-search?pivots=programming-language-csharp
        // keywords will use to search on full-text search column with `IsFullTextIndexed=true` attribute
        var searchResults = vectorCollection.HybridSearchAsync(
            searchValue: vector,
            keywords: keywords,
            top: take,
            options: options,
            cancellationToken: cancellationToken
        );

        var result = new VectorSearchResult<TEntity>
        {
            Search = searchTerm,
            AIExplanationMessage = "No matching results found.",
        };

        var sbFoundItems = new StringBuilder();
        int position = 1;

        var appliedThreshold = threshold ?? _agentFrameworkOptions.SearchThreshold;
        ArgumentNullException.ThrowIfNull(appliedThreshold);

        await foreach (var searchResult in searchResults.OrderByDescending(x => x.Score))
        {
            // Higher scores indicate better relevance in vector similarity search
            if (searchResult.Score >= appliedThreshold)
            {
                var item = await dbContext.FindAsync<TEntity>(searchResult.Record.Id);

                if (item != null)
                {
                    result.Data.Add(item);

                    sbFoundItems.AppendLine(
                        $"- {
                            typeof(TEntity).Name
                        } {
                            position
                        }: \n {
                            JsonSerializer.Serialize(
                                item,
                                options: SystemTextJsonSerializerOptions.DefaultSerializerOptions)
                        }"
                    );

                    position++;
                }
            }
        }

        if (result.Data.Count == 0)
            return result;

        var dynamicPrompt =
            $@"
User searched for: {searchTerm}
        Search terms used to find matching items: {keywords}

        Matching items:
        {sbFoundItems}

        Based on the guidelines above, generate a helpful, natural-language explanation of these results for the user.
        ";

        List<ChatMessage> conversationHistory = [new(ChatRole.System, InitPrompt), new(ChatRole.System, dynamicPrompt)];

        try
        {
            ChatResponse aiResponse = await chatClient.GetResponseAsync(
                conversationHistory,
                // https://ollama.com/blog/thinking
                ChatOptionsDefaults.GetProviderChatOptions(_agentFrameworkOptions),
                cancellationToken: cancellationToken
            );

            result.AIExplanationMessage = aiResponse.Text;
        }
        catch (Exception ex)
        {
            result.AIExplanationMessage = $"Search completed, but explanation could not be generated: {ex.Message}";
        }

        return result;
    }

    // https://learn.microsoft.com/en-us/semantic-kernel/concepts/vector-store-connectors/vector-search?pivots=programming-language-csharp
    public async Task<VectorSearchResult<TEntity>> SemanticSearchAsync<TVectorEntity, TEntity>(
        string searchTerm,
        DbContext dbContext,
        int take = 5,
        int skip = 0,
        double? threshold = null,
        Expression<Func<TVectorEntity, bool>>? filter = null,
        CancellationToken cancellationToken = default
    )
        where TVectorEntity : VectorEntityBase
        where TEntity : Entity
    {
        var collection = vectorStore.GetCollection<Guid, TVectorEntity>(typeof(TVectorEntity).Name.Underscore());
        await collection.EnsureCollectionExistsAsync(cancellationToken);

        var options = new VectorSearchOptions<TVectorEntity>
        {
            VectorProperty = r => r.Vector,
            Skip = skip,
            IncludeVectors = false,
            // https://learn.microsoft.com/en-us/semantic-kernel/concepts/vector-store-connectors/vector-search?pivots=programming-language-csharp#filter
            Filter = filter,
        };

        // https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/embedding-generation/?tabs=csharp-Ollama&pivots=programming-language-csharp#using-text-embedding-generation-services
        var vector = await embeddingGenerator.GenerateVectorAsync(searchTerm, cancellationToken: cancellationToken);
        var searchResults = collection.SearchAsync(
            searchValue: vector,
            top: take,
            options: options,
            cancellationToken: cancellationToken
        );

        var result = new VectorSearchResult<TEntity>
        {
            Search = searchTerm,
            AIExplanationMessage = "No matching results found.",
        };

        var sbFoundItems = new StringBuilder();
        int position = 1;

        var appliedThreshold = threshold ?? _agentFrameworkOptions.SearchThreshold;
        ArgumentNullException.ThrowIfNull(appliedThreshold);

        await foreach (var searchResult in searchResults.OrderByDescending(x => x.Score))
        {
            // Higher scores indicate better relevance in vector similarity search
            if (searchResult.Score >= appliedThreshold)
            {
                var item = await dbContext.FindAsync<TEntity>(searchResult.Record.Id);

                if (item != null)
                {
                    result.Data.Add(item);

                    sbFoundItems.AppendLine(
                        $"- {
                            typeof(TEntity).Name
                        } {
                            position
                        }: \n {
                            JsonSerializer.Serialize(
                                item,
                                options: SystemTextJsonSerializerOptions.DefaultSerializerOptions)
                        }"
                    );

                    position++;
                }
            }
        }

        if (result.Data.Count == 0)
            return result;

        var dynamicPrompt =
            $@"
Generate a natural language explanation for the search results below. 
Make the response sound natural and helpful to the user.

- User Search Term: {
    searchTerm
}
- Matching Items:
{
    sbFoundItems
}
";

        List<ChatMessage> conversationHistory = [new(ChatRole.System, InitPrompt), new(ChatRole.System, dynamicPrompt)];

        try
        {
            ChatResponse aiResponse = await chatClient.GetResponseAsync(
                conversationHistory,
                // https://ollama.com/blog/thinking
                ChatOptionsDefaults.GetProviderChatOptions(_agentFrameworkOptions),
                cancellationToken: cancellationToken
            );

            result.AIExplanationMessage = aiResponse.Text;
        }
        catch (Exception ex)
        {
            result.AIExplanationMessage = $"Search completed, but explanation could not be generated: {ex.Message}";
        }

        return result;
    }
}
