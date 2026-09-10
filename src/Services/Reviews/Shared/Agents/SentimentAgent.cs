using A2A;
using BuildingBlocks.AI.AgentFramework;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;

namespace GenAIEshop.Reviews.Shared.Agents;

public static class SentimentAgent
{
    private const string Name = GenAIEshop.Shared.Constants.Agents.SentimentAgent;

    // Used by Other agents for agent discovery and routing - helps other agents understand what this agent can do
    private const string Description =
        "An agent that get input from previous chat history or agent and evaluates the sentiment of input data as negative, positive, or neutral.";

    // Used by The agent itself internally and guides the agent's behavior and responsibility - how it should think and respond
    private const string Instructions = """
        You are a sentiment analysis assistant for GenAI-eShop. Your role is to evaluate the emotional tone of user messages that have been processed by other Agent.

        **Primary Responsibilities**:
        - Try to Keep previous chat history because it is **important** and add sentiment analysis to the end of the conversation.
        - Evaluate the sentiment of input data as negative, positive, or neutral.
        - Analyze user-generated content, feedback, and communications
        - Classify sentiment as: Positive, Negative, or Neutral
        - Consider context and nuance when making assessments
        - Pay attention to emotional indicators and user satisfaction signals

        **Analysis Criteria**:
        - **Positive**: Happy, satisfied, excited, pleased, enthusiastic about products/services
        - **Negative**: Frustrated, disappointed, angry, dissatisfied, complaints
        - **Neutral**: Informational, factual, mixed feedback without strong emotional tone

        **Output Requirements**:
        - Keep previous chat history in the output because it is **important** and add sentiment analysis to the end of the conversation.
        - Provide clear sentiment classification (Positive/Negative/Neutral)
        - Include confidence level (High/Medium/Low)
        - Brief explanation of reasoning behind the sentiment assessment
        - Highlight key phrases or aspects that influenced the sentiment
        - Consider overall user satisfaction context

        **Special Considerations**:
        - Use some emoji to indicate sentiment

        Your analysis helps the other agents understand the user's emotional state to provide appropriate responses.
        """;

    public static AIAgent CreateAgent(IChatClient chatClient)
    {
        return new ChatClientAgent(chatClient, name: Name, description: Description, instructions: Instructions);
    }

    public static AgentCard GetAgentCard()
    {
        var capabilities = new AgentCapabilities { Streaming = false, PushNotifications = false };

        var sentiment = new A2A.AgentSkill
        {
            Id = "id_sentiment_agent",
            Name = Name,
            Description = Description,
            Tags = ["sentiment-analysis", "reviews", "customer-feedback", "semantic-kernel"],
            Examples =
            [
                "Analyze the sentiment of these product reviews",
                "What is the overall sentiment for customer feedback on product XYZ?",
                "Classify the emotional tone of this customer comment",
                "Evaluate whether reviews are positive, negative, or neutral",
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
            Skills = [sentiment],
        };
    }
}
