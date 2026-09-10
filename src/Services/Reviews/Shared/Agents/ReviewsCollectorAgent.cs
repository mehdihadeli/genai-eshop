using A2A;
using BuildingBlocks.AI.AgentFramework;
using GenAIEshop.Reviews.Shared.Tools;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;

namespace GenAIEshop.Reviews.Shared.Agents;

public static class ReviewsCollectorAgent
{
    private const string Name = GenAIEshop.Shared.Constants.Agents.ReviewsCollectorAgent;
    private const string Description = "Expert in fetching product reviews in a genai-eshop application";

    private const string Instructions = """
        You are responsible for fetching product reviews data for analysis.

        **Primary Responsibilities**:
        1. Fetch product reviews data, Total Reviews and Rating Distribution
        2. Structurize reviews data for analysis by next agents

        **Available Context for Concise Analysis**:**:
        - **GetReviewsByProductId function**: To retrieve all reviews for a specific product through product id which is guid
        - **GetReviewsByProductIds function**: To retrieves all reviews for a list of product ids (guids). Returns reviews grouped by product.
        - **GetRecentReviews function**: To analyze reviews from specific time periods to identify trends

        **Function Selection Strategy**:
        - Select functions based on the specific analysis requirements and context
        - Combine multiple functions for comprehensive rating assessment
        - Use data access functions first to gather review information
        - Chain function calls to build complete analysis pipelines
        """;

    public static AIAgent CreateAgent(IChatClient chatClient, ReviewsTool reviewsTool)
    {
        // https://learn.microsoft.com/en-us/agent-framework/tutorials/agents/function-tools?pivots=programming-language-csharp
        // https://github.com/microsoft/agent-framework/blob/9148392d00dafa47a95178622f3800f89bbf0425/dotnet/samples/GettingStarted/Agents/Agent_Step15_Plugins/Program.cs
        var reviewsFunctionTools = reviewsTool.AsAITools();

        return new ChatClientAgent(
            chatClient,
            name: Name,
            description: Description,
            instructions: Instructions,
            tools: [.. reviewsFunctionTools]
        );
    }

    public static AgentCard GetAgentCard()
    {
        var capabilities = new AgentCapabilities { Streaming = false, PushNotifications = false };

        var dataRetrievalSkill = new A2A.AgentSkill
        {
            Id = "id_data_retrieval_agent",
            Name = Name,
            Description = Description,
            Tags =
            [
                "data-retrieval",
                "review-fetching",
                "product-reviews",
                "data-structuring",
                "e-commerce",
                "semantic-kernel",
            ],
            Examples =
            [
                "Get reviews for product id `9AE02C92-8B73-4350-8C38-98CC80E90C5E`",
                "Get reviews for products ids `[76C55ED4-BBF3-46C8-882C-B152361E9C96, F0DF3D70-6332-41E4-A22E-6545E53E6156]`",
                "Fetch recent reviews from the last 30 days for product `9AE02C92-8B73-4350-8C38-98CC80E90C5E`",
            ],
        };

        return new AgentCard
        {
            Name = Name,
            Description = "Expert in fetching and structuring review data for analysis",
            Version = "1.0.0",
            Provider = new AgentProvider { Organization = nameof(GenAIEshop) },
            DefaultInputModes = ["text"],
            DefaultOutputModes = ["text"],
            Capabilities = capabilities,
            Skills = [dataRetrievalSkill],
        };
    }
}
