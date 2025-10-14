using A2A;
using BuildingBlocks.AI.AgentFramework;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;

namespace GenAIEshop.Reviews.Shared.Agents;

public static class SummerizeAgent
{
    private const string Name = GenAIEshop.Shared.Constants.Agents.SummarizeAgent;

    // Used by Other agents for agent discovery and routing - helps other agents understand what this agent can do
    private const string Description =
        "An agent that summarizes product reviews and comments while preserving key insights and sentiment.";

    // Used by The agent itself internally and guides the agent's behavior and responsibility - how it should think and respond
    private const string Instructions = """
        You are a content summarization assistant for GenAI-eShop. Your role is to process English text from the other agents and create concise, meaningful summaries.

        **Primary Responsibilities**:
        - Try to keep overall input text and tone of the content
        - Extract key positive and negative points from user feedback
        - Identify common themes and patterns across content
        - Preserve the overall sentiment and trend information
        - Highlight key strengths and weaknesses mentioned by users

        **Summarization Approach**:
        - **Preserve Sentiment**: Maintain the overall positive/negative sentiment balance
        - **Extract Key Points**: Identify frequently mentioned topics, features, and concerns
        - **Maintain Context**: Ensure summary provides enough detail about the content's essence
        - **Concise Output**: Create brief, clear summaries that capture essential information

        **Special Considerations**:
        - Weight detailed content more heavily than simple statements
        - Consider the credibility and specificity of information
        - Look for patterns across multiple items rather than isolated content
        - Maintain objectivity and avoid introducing personal bias

        Your summaries help the other agents understand user needs efficiently and provide better responses.
        """;

    public static AIAgent CreateAgent(IChatClient chatClient)
    {
        return chatClient.CreateAIAgent(name: Name, description: Description, instructions: Instructions);
    }

    public static AgentCard GetAgentCard()
    {
        var capabilities = new AgentCapabilities { Streaming = false, PushNotifications = false };

        var summarize = new AgentSkill
        {
            Id = "id_summerize_agent",
            Name = Name,
            Description = Description,
            Tags = ["summarization", "reviews", "product-feedback", "semantic-kernel"],
            Examples =
            [
                "Summarize the reviews for product XYZ-123",
                "Create a summary of customer feedback for the latest smartphone",
                "Provide an overview of what customers are saying about this laptop",
                "Condense these product reviews into key takeaways",
            ],
        };

        return new AgentCard
        {
            Name = Name,
            Description = Description,
            Version = "1.0.0",
            Provider = new AgentProvider { Organization = nameof(GenAIEshop) },
            DefaultInputModes = ["text"],
            DefaultOutputModes = ["text"],
            Capabilities = capabilities,
            Skills = [summarize],
        };
    }
}
