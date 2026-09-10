using A2A;
using BuildingBlocks.AI.AgentFramework;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;

namespace GenAIEshop.Reviews.Shared.Agents;

public static class LanguageAgent
{
    private const string Name = GenAIEshop.Shared.Constants.Agents.LanguageAgent;
    private const string Description =
        "An agent that get input from previous chat history and translates it to English for better context understanding in product reviews and queries.";

    private const string Instructions = """
        You are a language detection and translation assistant for GenAI-Eshop. Your primary responsibilities are:

        **Language Detection:**
        - If language is english don't change the input text.
        - Keep md structure and formatting for input text when translating that text to English
        - Ensure the last conversation history is passed to the subsequent agent and translate it to English for better context understanding in product reviews and queries.
        - Automatically detect the language of user input across all microservices
        - Identify whether the text is in English or another language
        - Recognize common languages used in e-commerce contexts

        **Translation to English:**
        - If user input is not in English, translate it to clear, understandable English
        - Preserve the original meaning, context, and emotional tone during translation
        - Maintain the intent and sentiment of the original message
        - Ensure translations are natural, grammatically correct, and culturally appropriate
        - Pay special attention to domain-specific terminology and nuances

        **E-commerce Specific Considerations:**
        - Preserve product names, brands, and technical specifications accurately
        - Maintain the emotional tone of user content (positive, negative, neutral)
        - Handle colloquial expressions and slang commonly used in customer interactions
        - Ensure product features and specifications are translated correctly

        **Output Format:**
        - Provide ONLY the translated English text for non-English inputs
        - Keep English inputs unchanged
        - Do NOT provide multiple translation options
        - Do NOT include explanations, alternatives, or additional commentary
        - Output should be the single most natural and clear translation
        - Maintain the original structure and formatting when appropriate

        **Special Cases:**
        - Mixed language inputs: Translate non-English portions to English
        - Code-switching: Handle smoothly when users switch between languages
        - Product names/brands: Keep proper nouns and brand names in original form unless commonly translated
        - Technical terms: Ensure accurate translation of domain-specific terminology

        Your goal is to ensure all user communications are accessible in English for proper processing by other agents in the system.
        """;

    public static AIAgent CreateAgent(IChatClient chatClient)
    {
        return new ChatClientAgent(chatClient, name: Name, description: Description, instructions: Instructions);
    }

    public static AgentCard GetAgentCard()
    {
        var capabilities = new AgentCapabilities { Streaming = false, PushNotifications = false };

        var languageSkill = new A2A.AgentSkill
        {
            Id = "id_language_agent",
            Name = Name,
            Description = Description,
            Tags =
            [
                "language-detection",
                "translation",
                "multilingual",
                "customer-reviews",
                "e-commerce",
                "semantic-kernel",
            ],
            Examples =
            [
                "Translate this Spanish product review to English",
                "Detect the language of this customer feedback and convert it to English",
                "Convert this French search query to English for product search",
                "Translate this German customer complaint about product quality",
                "Process this multilingual product review and provide English translation",
                "Convert this Italian product description to English",
            ],
        };

        return new AgentCard
        {
            Name = Name,
            Description =
                "Detects user input language and translates it to English for better context understanding in product reviews and queries",
            Version = "1.0.0",
            Provider = new AgentProvider { Organization = nameof(GenAIEshop) },
            DefaultInputModes = ["text"],
            DefaultOutputModes = ["text"],
            Capabilities = capabilities,
            Skills = [languageSkill],
        };
    }
}
