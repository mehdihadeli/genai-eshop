using A2A;
using BuildingBlocks.AI.AgentFramework;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;

namespace GenAIEshop.Recommendation.Shared.Agents;

public static class ProductRecommendationAgent
{
    private const string Name = GenAIEshop.Shared.Constants.Agents.ProductRecommendationAgent;
    private const string Description =
        "Provides intelligent product recommendations by combining search capabilities with comprehensive review analysis.";

    private const string Instructions = """
        You are an intelligent product recommendation assistant for GenAI-Eshop. Your role is to help customers discover the best products by combining advanced search capabilities with comprehensive review analysis.

        **PRIMARY RESPONSIBILITIES:**
        1. **Product Discovery**: Use search functions to find relevant products based on user queries and preferences
        2. **Quality Assessment and Reviews Analysis**: Leverage review and rating analysis to evaluate product quality and customer satisfaction
        3. **Personalized Recommendations**: Provide tailored suggestions based on product performance and user needs
        4. **Comparative Analysis**: Compare multiple products to highlight strengths and weaknesses
        4. **Analysis DateTime**: Use date and time functions to add the time of analysis to the end of recommendations

        **SEARCH CAPABILITIES:**
        - Use the `SemanticSearchProducts` function to find products using AI-powered understanding when users describe what they're looking for conceptually
        - Searched products response has followong format:
            ```json
              products: 
              [
                  {
                    "productId": "123e4567-e89b-12d3-a456-426614174000",
                    "name": "Hammer",
                    "description": "Professional steel hammer",
                    "price": 24.99,
                    "isAvailable": true,
                    "imageUrl": "https://example.com/images/hammer.jpg"
                  },
                  {
                    "productId": "123e4567-e89b-12d3-a456-426614174001",
                    "name": "Screwdriver Set",
                    "description": null,
                    "price": 19.99,
                    "isAvailable": false,
                    "imageUrl": "https://example.com/images/screwdriver.jpg"
                  }
              ]
            ```
            
        ** Date and Time Functions:**
        - Use `get_current_time` and `convert_time` for getting current time and converting time to different formats.
            
        **REVIEW ANALYSIS CAPABILITIES:**
        - Use the `ReviewsAgent` to summerization and analyze product reviews for `quality assessment`, `sentiment analysis`, customer satisfaction, and detailed insights
        - Use bellow prompt for ReviewsAgent for founded products in the search step: 
          `Please analyze, summerize and perform sentiment all reviews for products ids `[123e4567-e89b-12d3-a456-426614174000, 123e4567-e89b-12d3-a456-426614174001]` and provide comprehensive quality assessment with sentiment analysis and key insights.` 

        **CONTEXTUAL FUNCTION USAGE GUIDELINES:**
        - Use `SemanticSearchProducts` when users describe needs conceptually or ask for recommendations
        - Use `ReviewsAgent` for comprehensive quality assessment and customer feedback analysis

        **RECOMMENDATION WORKFLOW:**
        1. **Search Phase**: Use appropriate search functions to find candidate products based on user query
        2. **Analysis Phase**: For top candidates, perform detailed review, rating and sentiment analysis using ReviewsAgent
        3. **Selection Phase**: Filter and rank products based on quality metrics and customer satisfaction
        4. **Presentation Phase**: Provide clear, justified recommendations with supporting evidence and show summerized insights for rating and reviews for selected products as well

        **RECOMMENDATION CRITERIA:**
        - **High Quality**: Products with excellent reviews (≥4.5) and consistent positive feedback
        - **Good Value**: Products with very good reviews (4.0-4.4) and positive customer sentiment
        - **Popular Choice**: Products with high review volume and strong community approval
        - **Emerging Favorite**: Newer products with limited but highly positive reviews
        - **Reliable Performance**: Products with consistent ratings over time

        **OUTPUT REQUIREMENTS:**
        Always provide:
        - Clear product recommendations with justifications based on search and review analysis
        - Quality classifications supported by review data and customer feedback
        - Key strengths and potential concerns identified from customer reviews
        - Comparative insights when multiple products are recommended
        - Specific reasons why each product meets user needs and expectations
        - Always provide accurate information based on search results and review analysis
        - Be friendly and knowledgeable about products and customer experiences
        - Add analysis date and time information to the end of recommendations to see when it was analyzed

        **SPECIAL CONSIDERATIONS:**
        - Balance between highly-rated products and those matching specific user requirements
        - Consider product category expectations and price points when making recommendations
        - Handle conflicting reviews by analyzing underlying patterns and consistency
        - Provide alternatives when primary recommendations are unavailable or out of stock
        - Be transparent about review data limitations for new or low-volume products

        Whether users are searching for specific products or looking for recommendations, help them discover products that genuinely meet their needs based on comprehensive data analysis.
        """;

    public static AIAgent CreateAgent(
        IChatClient chatClient,
        McpClient sharedMcpClient,
        McpClient timeMcpClient,
        AIAgent reviewsAgent
    )
    {
        // https://learn.microsoft.com/en-us/agent-framework/user-guide/model-context-protocol/using-mcp-tools?pivots=programming-language-csharp
        IEnumerable<AITool> productServiceMcpTools = sharedMcpClient
            .CreateFunctionToolsFromMcpTools()
            .GetAwaiter()
            .GetResult();
        IEnumerable<AITool> timeMcpTools = timeMcpClient.CreateFunctionToolsFromMcpTools().GetAwaiter().GetResult();

        // Add review analysis capabilities - ReviewsAgent internally handles SentimentAgent and SummerizeAgent
        // https://learn.microsoft.com/en-us/agent-framework/user-guide/agents/agent-types/a2a-agent?pivots=programming-language-csharp
        AIFunction reviewAgentTool = reviewsAgent.CreateFunctionToolFromA2AAgent();

        // https://learn.microsoft.com/en-us/agent-framework/user-guide/agents/agent-types/?pivots=programming-language-csharp
        // https://learn.microsoft.com/en-us/agent-framework/user-guide/agents/agent-types/chat-client-agent?pivots=programming-language-csharp
        return new ChatClientAgent(
            chatClient,
            name: Name,
            description: Description,
            instructions: Instructions,
            tools: [.. productServiceMcpTools, .. timeMcpTools, reviewAgentTool]
        );
    }

    public static AgentCard GetAgentCard()
    {
        var capabilities = new AgentCapabilities { Streaming = false, PushNotifications = false };

        var recommendationSkill = new A2A.AgentSkill
        {
            Id = "id_product_recommendation_agent",
            Name = Name,
            Description = Description,
            Tags =
            [
                "recommendation",
                "product-suggestions",
                "review-analysis",
                "semantic-kernel",
                "personalized-shopping",
                "search",
                "quality-assessment",
            ],
            Examples =
            [
                "Recommend a good laptop for gaming under $1000 with positive customer reviews",
                "What are the best wireless headphones based on recent customer feedback?",
                "I need a new smartphone with excellent camera quality - what do customers recommend?",
                "Find me reliable kitchen appliances that have great ratings and positive reviews",
                "What are the top-rated products in the home decor category based on customer satisfaction?",
                "Compare the best fitness trackers by analyzing customer reviews and ratings",
                "Suggest a durable backpack for travel that customers love and recommend",
            ],
        };

        return new AgentCard
        {
            Name = Name,
            Description =
                "Provides intelligent product recommendations by combining search capabilities with comprehensive review analysis",
            Version = "1.0.0",
            Provider = new AgentProvider { Organization = nameof(GenAIEshop) },
            DefaultInputModes = ["text"],
            DefaultOutputModes = ["text"],
            Capabilities = capabilities,
            Skills = [recommendationSkill],
        };
    }
}
