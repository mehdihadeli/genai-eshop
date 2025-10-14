// using Microsoft.SemanticKernel.Agents;
//
// namespace GenAIEshop.Reviews.Shared.Agents.OrchestrationsAgents;
//
// public sealed class ReviewsChatOrchestrationAgent(
//     [FromKeyedServices(GenAIEshop.Shared.Constants.Agents.LanguageAgent)] Agent languageAgent,
//     [FromKeyedServices(GenAIEshop.Shared.Constants.Agents.SentimentAgent)] Agent sentimentAgent,
//     [FromKeyedServices(GenAIEshop.Shared.Constants.Agents.InsightsSynthesizerAgent)] Agent insightsSynthesizerAgent,
//     [FromKeyedServices(GenAIEshop.Shared.Constants.Agents.ReviewsCollectorAgent)] Agent reviewsCollectorAgent
// )
// {
//     public Agent ReviewsCollectorAgent { get; } = reviewsCollectorAgent;
//     public Agent LanguageAgent { get; } = languageAgent;
//     public Agent SentimentAgent { get; } = sentimentAgent;
//     public Agent InsightsSynthesizerAgent { get; } = insightsSynthesizerAgent;
// }
