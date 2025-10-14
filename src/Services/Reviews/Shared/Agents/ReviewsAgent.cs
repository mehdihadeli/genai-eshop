using A2A;
using BuildingBlocks.AI.AgentFramework;
using GenAIEshop.Reviews.Shared.Tools;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace GenAIEshop.Reviews.Shared.Agents;

/// <summary>
/// Provides comprehensive product review analysis and quality assessment capabilities.
/// This agent coordinates with specialized agents to deliver detailed insights into product performance
/// based on customer reviews, ratings, and feedback.
/// </summary>
public static class ReviewsAgent
{
    private const string Name = GenAIEshop.Shared.Constants.Agents.ReviewsAgent;

    // Used by Other agents for agent discovery and routing - helps other agents understand what this agent can do
    private const string Description = "Provides comprehensive product review analysis and quality assessment.";

    // Used by: The agent itself internally and guides the agent's behavior and responsibility - how it should think and respond
    private const string Instructions = """
        You are responsible for comprehensive product review analysis and quality assessment for GenAI-Eshop.

        **Primary Responsibilities**:
        1. Analyze product reviews to determine overall quality and customer satisfaction
        2. Provide detailed insights into product strengths and weaknesses based on customer feedback
        3. Identify trends and patterns in customer reviews over time
        4. Handle sentiment analysis by leveraging SentimentAgent when needed
        5. Handle multilingual reviews by leveraging LanguageAgent when needed

        **Available Context for Concise Analysis**:**:
        - **GetReviewsByProductId function**: To retrieve all reviews for a specific product through product id which is guid
        - **GetReviewsByProductIds function**: To retrieves all reviews for a list of product ids (guids). Returns reviews grouped by product.
        - **GetRecentReviews function**: To analyze reviews from specific time periods to identify trends
        - **SentimentAgent**: Use `SentimentAgent` to analyze emotional tone and satisfaction levels in reviews
        - **SummerizeAgent**: Use `SummerizeAgent` to create concise summaries of review content and key insights if reviews are long
        - **LanguageAgent**: Use `LanguageAgent` to detect language and translate non-English reviews to English

        **Function Selection Strategy**:
        - Select functions based on the specific analysis requirements and context
        - Combine multiple functions for comprehensive rating assessment
        - Use data access functions first to gather review information
        - Leverage specialized agents for in-depth analysis of specific aspects
        - Chain function calls to build complete analysis pipelines

        **Output Format**:
        Always provide:
        - Include the product name and a brief description in your analysis
        - Overall quality classification with confidence level
        - Summary of key positive and negative aspects using SummerizeAgent
        - Sentiment analysis overview using SentimentAgent
        - Reviews statistics (average rating, total reviews, distribution)
        - Recommendations for product improvement (if applicable)

        **Special Considerations**:
        - Consider review volume and credibility
        - Weight recent reviews more heavily
        - Look for consistency in feedback patterns
        - Consider product category and price point expectations
        - Handle conflicting reviews by analyzing underlying patterns
        - Select appropriate functions based on analysis depth required
        - Combine statistical data with qualitative insights for comprehensive assessment
        - Add md icons to the output to make it more appealing
        """;

    // https://github.com/microsoft/agent-framework/blob/main/dotnet/samples/SemanticKernelMigration/AzureOpenAI/Step03_DependencyInjection/Program.cs
    // https://github.com/microsoft/agent-framework/blob/main/dotnet/samples/SemanticKernelMigration/AzureOpenAI/Step02_ToolCall/Program.cs
    // https://learn.microsoft.com/en-us/agent-framework/tutorials/agents/run-agent?pivots=programming-language-csharp

    /// <summary>
    /// Creates and configures a ReviewsAgent instance with access to review data and specialized analysis agents.
    /// </summary>
    /// <returns>A configured ChatCompletionAgent capable of comprehensive review analysis</returns>
    /// <remarks>
    /// This agent includes:
    /// - Access to review data through ReviewsPlugin
    /// - Integration with SentimentAgent for emotional tone analysis
    /// - Integration with SummerizeAgent for review summarization
    /// - Auto function selection with retained argument types
    /// - Low temperature setting for consistent, reliable analysis
    /// </remarks>
    public static AIAgent CreateAgent(
        IChatClient chatClient,
        AIAgent languageAgent,
        AIAgent sentimentAgent,
        AIAgent summerizeAgent,
        ReviewsTool reviewsTool
    )
    {
        // Map specialized agents as child agents
        // https://learn.microsoft.com/en-us/agent-framework/tutorials/agents/agent-as-function-tool
        AIFunction languageAgentFunctionTool = languageAgent.CreateFunctionToolFromLocalAgent();
        AIFunction sentimentAgentFunctionTool = sentimentAgent.CreateFunctionToolFromLocalAgent();
        AIFunction summerizeAgentFunctionTool = summerizeAgent.CreateFunctionToolFromLocalAgent();

        // https://learn.microsoft.com/en-us/agent-framework/tutorials/agents/function-tools?pivots=programming-language-csharp
        // https://github.com/microsoft/agent-framework/blob/9148392d00dafa47a95178622f3800f89bbf0425/dotnet/samples/GettingStarted/Agents/Agent_Step15_Plugins/Program.cs
        var reviewsFunctionTools = reviewsTool.AsAITools();

        // https://learn.microsoft.com/en-us/agent-framework/user-guide/agents/agent-types/?pivots=programming-language-csharp
        // https://learn.microsoft.com/en-us/agent-framework/user-guide/agents/agent-types/chat-client-agent?pivots=programming-language-csharp
        return chatClient.CreateAIAgent(
            name: Name,
            description: Description,
            instructions: Instructions,
            tools:
            [
                .. reviewsFunctionTools,
                languageAgentFunctionTool,
                sentimentAgentFunctionTool,
                summerizeAgentFunctionTool,
            ]
        );
    }

    /// <summary>
    /// Generates an AgentCard that describes the capabilities and configuration of the ReviewsAgent to use in the A2A.
    /// </summary>
    /// <returns>An AgentCard containing metadata about the agent's skills, capabilities, and examples</returns>
    /// <remarks>
    /// The AgentCard is used for:
    /// - Agent discovery and registration
    /// - UI presentation of agent capabilities
    /// - Documentation generation
    /// - Agent orchestration and routing decisions
    /// </remarks>
    public static AgentCard GetAgentCard()
    {
        var capabilities = new AgentCapabilities { Streaming = false, PushNotifications = false };

        var reviews = new AgentSkill
        {
            Id = "id_reviews_agent",
            Name = Name,
            Description = Description,
            Tags = ["reviews", "product-analysis", "quality-assessment", "semantic-kernel", "customer-feedback"],
            Examples =
            [
                "Analyze all reviews for product id `A7F48F79-EF05-48B4-BBE0-1818C593B18C` and provide comprehensive assessment",
                "Analyze reviews for products ids `[8134F0E7-08B5-44E5-A5E9-63B5B34E0BF8, 5F9ED1A7-4913-496B-B68B-C9350A82A638]` and provide comprehensive assessment",
                "What is the overall quality of this product based on customer reviews?",
                "Provide complete product evaluation using all available review data",
                "Generate comprehensive review insights with sentiment and summary for product XYZ-789",
                "How do customers rate this product and what are the common complaints?",
            ],
        };

        return new AgentCard
        {
            Name = Name,
            Description = "Provides comprehensive product review analysis and quality assessment",
            Version = "1.0.0",
            Provider = new AgentProvider { Organization = nameof(GenAIEshop) },
            DefaultInputModes = ["text"],
            DefaultOutputModes = ["text"],
            Capabilities = capabilities,
            Skills = [reviews],
        };
    }
}
