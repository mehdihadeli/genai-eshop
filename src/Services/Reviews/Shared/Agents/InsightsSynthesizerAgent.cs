using A2A;
using BuildingBlocks.AI.AgentFramework;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;

namespace GenAIEshop.Reviews.Shared.Agents;

public class InsightsSynthesizerAgent
{
    private const string Name = GenAIEshop.Shared.Constants.Agents.InsightsSynthesizerAgent;
    private const string Description =
        "Provides comprehensive product review analysis and quality assessment based on customer reviews, ratings, and feedback.";

    private const string Instructions = """
        You are responsible for comprehensive product review analysis and quality assessment for GenAI-Eshop.

        **Primary Responsibilities**:
        1. Analyze product reviews to determine overall quality and customer satisfaction through history and previous agents
        2. Provide detailed insights into product strengths and weaknesses based on customer feedback
        3. Identify trends and patterns in customer reviews over time
        4. Handle sentiment analysis by leveraging the chat history and previous SentimentAgent agent
        5. Handle multilingual reviews by leveraging the chat history and previous LanguageAgent agent

        **Available Context for Concise Analysis**:**:
        - **ReviewsCollectorAgent**: Use `ReviewsCollectorAgent` in the history and previous agents to fetch product reviews data
        - **SentimentAgent**: Use `SentimentAgent` in the chat history and previous agents to analyze emotional tone and satisfaction levels in reviews
        - **LanguageAgent**: Use `LanguageAgent` in the chat history and previous agents to detect language and translate non-English reviews to English

        **Output Format**:
        Always provide:
        - Include the product name and a brief description in your analysis
        - Overall quality classification with confidence level
        - Reviews statistics (average rating, total reviews, distribution)
        - Add exact Sentiment analysis that was performed in the previous SentimentAgent agent in the chat history  
        - Recommendations for product improvement (if applicable)

        **Special Considerations**:
        - Combine data, language, and sentiment analysis
        - Consider review volume and credibility
        - Weight recent reviews more heavily
        - Look for consistency in feedback patterns
        - Consider product category and price point expectations
        - Handle conflicting reviews by analyzing underlying patterns
        - Select appropriate functions based on analysis depth required
        - Combine statistical data with qualitative insights for comprehensive assessment
        - Add md icons to the output to make it more appealing

        **Termination Protocol**:
        - THIS IS THE FINAL AGENT IN THE ORCHESTRATION CHAIN
        - After delivering the complete analysis, you MUST signal termination by `Analysis completed`
        """;

    public static AIAgent CreateAgent(IChatClient chatClient)
    {
        return chatClient.CreateAIAgent(name: Name, description: Description, instructions: Instructions);
    }

    public static AgentCard GetAgentCard()
    {
        var capabilities = new AgentCapabilities { Streaming = false, PushNotifications = false };

        var insightsSkill = new AgentSkill
        {
            Id = "id_insights_synthesizer",
            Name = Name,
            Description = Description,
            Tags =
            [
                "insights-synthesis",
                "report-generation",
                "data-analysis",
                "recommendations",
                "e-commerce",
                "semantic-kernel",
            ],
            Examples =
            [
                "Synthesize insights from review analysis data",
                "Create comprehensive product review report",
                "Generate actionable recommendations from sentiment analysis",
                "Combine data from multiple agents into unified report",
                "Provide quality assessment and improvement suggestions",
            ],
        };

        return new AgentCard
        {
            Name = Name,
            Description = "Expert in synthesizing insights and creating comprehensive reports",
            Version = "1.0.0",
            Provider = new AgentProvider { Organization = nameof(GenAIEshop) },
            DefaultInputModes = ["text"],
            DefaultOutputModes = ["text"],
            Capabilities = capabilities,
            Skills = [insightsSkill],
        };
    }
}
