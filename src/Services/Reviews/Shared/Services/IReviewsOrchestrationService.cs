namespace GenAIEshop.Reviews.Shared.Services;

public interface IReviewsOrchestrationService
{
    Task<string> AnalyzeReviewsUsingChatGroupOrchestrationAsync(string prompt);
    Task<string> AnalyzeReviewsUsingSequentialOrchestrationAsync(string prompt);
}
