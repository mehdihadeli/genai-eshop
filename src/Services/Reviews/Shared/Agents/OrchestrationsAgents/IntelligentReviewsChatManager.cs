// using System.Globalization;
// using BuildingBlocks.AI.AgentFramework;
// using Microsoft.Extensions.Options;
// using Microsoft.SemanticKernel;
// using Microsoft.SemanticKernel.Agents.Orchestration.GroupChat;
// using Microsoft.SemanticKernel.ChatCompletion;
//
// #pragma warning disable SKEXP0110
//
// namespace GenAIEshop.Reviews.Shared.Agents.OrchestrationsAgents;
//
// public class IntelligentReviewsChatManager : GroupChatManager
// {
//     private readonly Kernel _kernel;
//     private readonly IChatCompletionService _chatCompletion;
//     private readonly ReviewsChatOrchestrationAgent _reviewsChatOrchestrationAgent;
//     private readonly PromptExecutionSettings _executionSettings;
//
//     public IntelligentReviewsChatManager(
//         Kernel kernel,
//         IChatCompletionService chatCompletion,
//         ReviewsChatOrchestrationAgent reviewsChatOrchestrationAgent,
//         IOptions<AgentFrameworkOptions> AgentFrameworkOptions
//     )
//     {
//         _kernel = kernel;
//         _chatCompletion = chatCompletion;
//         _reviewsChatOrchestrationAgent = reviewsChatOrchestrationAgent;
//         // - MaximumInvocationCount is the total number of times any agent can speak in your group chat, acting as a conversation turn limit that prevents infinite loops and controls costs
//         // - if you have 4 agents and set it to 4, each agent gets exactly one turn before the chat automatically ends
//         MaximumInvocationCount = AgentFrameworkOptions.Value.MaximumInvocationCount;
//
//         // https://ollama.com/blog/thinking
//         // - in ollama cli we can `ollama run qwen3:0.6b --think=false` to turn off thinking
//         // - in ollama api with passing `think=false` as parameter
//         _executionSettings = ChatOptionsDefaults.GetProviderExecutionSettings(AgentFrameworkOptions.Value);
//     }
//
//     public override async ValueTask<GroupChatManagerResult<string>> SelectNextAgent(
//         ChatHistory history,
//         GroupChatTeam team,
//         CancellationToken cancellationToken = default
//     )
//     {
//         var currentHistory = history;
//
//         var lastMessage = currentHistory?.LastOrDefault();
//
//         if (lastMessage is null || currentHistory is null)
//         {
//             return new GroupChatManagerResult<string>(_reviewsChatOrchestrationAgent.ReviewsCollectorAgent.Name!)
//             {
//                 Reason = "Starting review analysis with data collection",
//             };
//         }
//
//         var recentMessages = currentHistory.TakeLast(3).ToList();
//         var conversationContext = string.Join(
//             "\n",
//             recentMessages.Select(m => $"{m.AuthorName}: {TruncateMessage(m.Content, 200)}")
//         );
//
//         // Analyze the conversation to decide who should speak next
//         var selectionPrompt = $"""
//                                Analyze the conversation and select the most appropriate agent to respond next.
//
//                                RECENT CONVERSATION (last 3 messages):
//                                {
//                                    conversationContext
//                                }
//
//                                AVAILABLE AGENTS:
//                                - ReviewsCollectorAgent: Fetches product reviews data using functions (GetReviewsByProductId, GetReviewsByProductIds, GetRecentReviews)
//                                - LanguageAgent: Detects languages, translates non-English content to English, If language is english don't change the input text.
//                                - SentimentAgent: Analyzes emotional tone, classifies sentiment (Positive/Negative/Neutral), adds sentiment analysis to conversation
//                                - InsightsSynthesizerAgent: Creates final comprehensive reports, synthesizes all previous analysis, provides quality assessment
//
//                                SELECTION RULES - APPLY IN ORDER:
//                                1. First agent to start is `ReviewsCollectorAgent`
//                                2. If ReviewsCollectorAgent hasn't provided review data → Choose ReviewsCollectorAgent
//                                3. If review data exists but LanguageAgent hasn't processed languages → Choose LanguageAgent
//                                4. If languages are processed but SentimentAgent hasn't analyzed sentiment → Choose SentimentAgent
//                                5. If sentiment is analyzed but InsightsSynthesizerAgent hasn't created final report → Choose InsightsSynthesizerAgent
//                                6. If any agent requests clarification → Choose the agent that can provide the needed information
//                                7. If same agent speaks twice in a row without progress → Choose the next agent in the pipeline
//                                8. If conversation circles without reaching final report → Choose ReviewsCollectorAgent to restart with fresh data
//
//                                SPECIFIC TRIGGERS:
//                                - "product id", "reviews", "fetch data", "GetReviewsBy" → ReviewsCollectorAgent
//                                - "language", "translate", "Spanish", "French", "non-English" → LanguageAgent
//                                - "sentiment", "emotional tone", "positive/negative", "emoji" → SentimentAgent
//                                - "summary", "report", "insights", "quality assessment", "recommendations" → InsightsSynthesizerAgent
//                                - "Analysis completed" → No further agents needed (terminate)
//
//                                Respond with ONLY the agent name (ReviewsCollectorAgent, LanguageAgent, SentimentAgent, or InsightsSynthesizerAgent) without any explanation.
//
//                                """;
//
//         var selectionChatHistory = new ChatHistory();
//         selectionChatHistory.AddUserMessage(selectionPrompt);
//
//         var response = await _chatCompletion.GetChatMessageContentAsync(
//             selectionChatHistory,
//             executionSettings: _executionSettings,
//             kernel: _kernel,
//             cancellationToken: cancellationToken
//         );
//
//         var selectedAgent = response.Content?.Trim() ?? _reviewsChatOrchestrationAgent.ReviewsCollectorAgent.Name;
//
//         // Map to actual agent names
//         string? agentName = selectedAgent switch
//         {
//             "ReviewsCollectorAgent" => _reviewsChatOrchestrationAgent.ReviewsCollectorAgent.Name,
//             "LanguageAgent" => _reviewsChatOrchestrationAgent.LanguageAgent.Name,
//             "SentimentAgent" => _reviewsChatOrchestrationAgent.SentimentAgent.Name,
//             "InsightsSynthesizerAgent" => _reviewsChatOrchestrationAgent.InsightsSynthesizerAgent.Name,
//             _ => _reviewsChatOrchestrationAgent.ReviewsCollectorAgent.Name,
//         };
//
//         return new GroupChatManagerResult<string>(agentName!)
//         {
//             Reason =
//                 $"Selected {
//                            agentName
//                        } after {
//                            lastMessage.AuthorName
//                        } provided: '{
//                            TruncateMessage(lastMessage.Content, 50)
//                        }'",
//         };
//     }
//
//     public override async ValueTask<GroupChatManagerResult<bool>> ShouldTerminate(
//         ChatHistory history,
//         CancellationToken cancellationToken = default
//     )
//     {
//         // First, check base termination (max iterations)
//         var baseResult = await base.ShouldTerminate(history, cancellationToken);
//
//         if (baseResult.Value)
//         {
//             return baseResult;
//         }
//
//         var lastMessage = history.LastOrDefault();
//
//         if (lastMessage?.Content != null)
//         {
//             // Check for the termination signal from InsightsSynthesizerAgent
//             if (lastMessage.AuthorName == _reviewsChatOrchestrationAgent.InsightsSynthesizerAgent.Name)
//             {
//                 // Check for explicit termination signal
//                 if (lastMessage.Content.Contains("Analysis completed", StringComparison.InvariantCultureIgnoreCase))
//                 {
//                     return new GroupChatManagerResult<bool>(true)
//                     {
//                         Reason = "InsightsSynthesizerAgent explicitly signaled analysis completion",
//                     };
//                 }
//
//                 // Check for comprehensive report completion indicators
//                 var completionIndicators = new[]
//                 {
//                     "comprehensive report",
//                     "final assessment",
//                     "quality assessment complete",
//                     "analysis concluded",
//                     "summary completed",
//                     "overall quality classification",
//                     "recommendations provided",
//                     "key insights summary",
//                 };
//
//                 if (
//                     completionIndicators.Any(indicator =>
//                         lastMessage
//                             .Content.ToLower(CultureInfo.InvariantCulture)
//                             .Contains(indicator, StringComparison.InvariantCultureIgnoreCase)
//                     )
//                 )
//                 {
//                     return new GroupChatManagerResult<bool>(true)
//                     {
//                         Reason = "InsightsSynthesizerAgent provided comprehensive final analysis",
//                     };
//                 }
//
//                 // Check if all analysis components are present in a final report
//                 var analysisComponents = new[]
//                 {
//                     "sentiment analysis",
//                     "quality assessment",
//                     "review statistics",
//                     "recommendations",
//                     "key insights",
//                 };
//
//                 var componentsPresent = analysisComponents.Count(component =>
//                     lastMessage
//                         .Content.ToLower(CultureInfo.InvariantCulture)
//                         .Contains(component, StringComparison.InvariantCultureIgnoreCase)
//                 );
//
//                 if (componentsPresent >= 3)
//                 {
//                     return new GroupChatManagerResult<bool>(true)
//                     {
//                         Reason = "Final report contains multiple analysis components",
//                     };
//                 }
//             }
//
//             // Check if we have a complete analysis pipeline
//             var agentContributions = history
//                 .Where(m => m.Role == AuthorRole.Assistant)
//                 .Select(m => m.AuthorName)
//                 .Distinct()
//                 .Count();
//
//             // If we have contributions from all 4 agents and InsightsSynthesizerAgent has spoken
//             if (
//                 agentContributions >= 4
//                 && history.Any(m => m.AuthorName == _reviewsChatOrchestrationAgent.InsightsSynthesizerAgent.Name)
//             )
//             {
//                 return new GroupChatManagerResult<bool>(true)
//                 {
//                     Reason = "Complete analysis pipeline executed with all agents contributing",
//                 };
//             }
//
//             // Check for repetitive patterns
//             if (history.Count > 6)
//             {
//                 var recentMessages = history.TakeLast(4).ToList();
//
//                 var uniqueContentCount = recentMessages
//                     .Select(m => m.Content?.ToLower(CultureInfo.InvariantCulture))
//                     .Distinct()
//                     .Count();
//
//                 if (uniqueContentCount <= 2)
//                 {
//                     return new GroupChatManagerResult<bool>(true)
//                     {
//                         Reason = "Conversation showing repetitive patterns - analysis likely complete",
//                     };
//                 }
//             }
//         }
//
//         return new GroupChatManagerResult<bool>(false)
//         {
//             Reason = "Continuing conversation - analysis pipeline in progress",
//         };
//     }
//
//     public override ValueTask<GroupChatManagerResult<string>> FilterResults(
//         ChatHistory history,
//         CancellationToken cancellationToken = default
//     )
//     {
//         // Prioritize the final report from InsightsSynthesizerAgent
//         var finalReport = history.LastOrDefault(m =>
//             m.AuthorName == _reviewsChatOrchestrationAgent.InsightsSynthesizerAgent.Name
//         );
//
//         if (finalReport != null && !string.IsNullOrEmpty(finalReport.Content))
//         {
//             return ValueTask.FromResult(new GroupChatManagerResult<string>(finalReport.Content));
//         }
//
//         // Fallback: return conversation summary
//         var agentMessages = history
//             .Where(m => m.Role == AuthorRole.Assistant)
//             .Select(m => $"[{m.AuthorName}] {m.Content}")
//             .ToList();
//
//         var result = string.Join("\n\n", agentMessages);
//         return ValueTask.FromResult(new GroupChatManagerResult<string>(result));
//     }
//
//     public override ValueTask<GroupChatManagerResult<bool>> ShouldRequestUserInput(
//         ChatHistory history,
//         CancellationToken cancellationToken = default
//     )
//     {
//         // Only request user input in specific edge cases
//         if (history.Count > 10)
//         {
//             var recentMessages = history.TakeLast(5).ToList();
//
//             // Check if the conversation is stuck with one agent
//             var recentSpeakers = recentMessages
//                 .Where(m => m.Role == AuthorRole.Assistant)
//                 .Select(m => m.AuthorName)
//                 .Distinct()
//                 .Count();
//
//             if (recentSpeakers <= 1)
//             {
//                 return ValueTask.FromResult(
//                     new GroupChatManagerResult<bool>(true)
//                     {
//                         Reason = "Conversation stuck with single agent - may need user direction",
//                     }
//                 );
//             }
//
//             // Check for clarification requests
//             var clarificationKeywords = new[]
//             {
//                 "clarify",
//                 "specify",
//                 "which product",
//                 "what exactly",
//                 "need more",
//                 "could you explain",
//                 "please clarify",
//             };
//
//             var lastMessage = history.LastOrDefault();
//
//             if (
//                 lastMessage != null
//                 && clarificationKeywords.Any(keyword =>
//                     lastMessage
//                         .Content?.ToLower(CultureInfo.InvariantCulture)
//                         .Contains(keyword, StringComparison.InvariantCulture) == true
//                 )
//             )
//             {
//                 return ValueTask.FromResult(
//                     new GroupChatManagerResult<bool>(true)
//                     {
//                         Reason = "Agent requested clarification - user input needed",
//                     }
//                 );
//             }
//
//             // Check if we're going in circles without reaching InsightsSynthesizerAgent
//             var hasFinalAgent = history.Any(m =>
//                 m.AuthorName == _reviewsChatOrchestrationAgent.InsightsSynthesizerAgent.Name
//             );
//
//             if (!hasFinalAgent && history.Count > 15)
//             {
//                 return ValueTask.FromResult(
//                     new GroupChatManagerResult<bool>(true)
//                     {
//                         Reason = "Extended conversation without reaching final analysis - may need user guidance",
//                     }
//                 );
//             }
//         }
//
//         return ValueTask.FromResult(
//             new GroupChatManagerResult<bool>(false)
//             {
//                 Reason = "Agents progressing through analysis pipeline - no user input needed",
//             }
//         );
//     }
//
//     private static string TruncateMessage(string? message, int maxLength)
//     {
//         if (string.IsNullOrEmpty(message))
//             return "[Empty message]";
//
//         return message.Length <= maxLength ? message : message.Substring(0, maxLength) + "...";
//     }
//
//     public async Task<ChatHistory?> GetReducedChatHistoryAsync(
//         ChatHistory fullHistory,
//         CancellationToken cancellationToken = default
//     )
//     {
//         ArgumentNullException.ThrowIfNull(fullHistory);
//
//         // If there are 3 or fewer messages, nothing to summarize, just return the original history
//         if (fullHistory.Count <= 3)
//             return fullHistory;
//
//         // For 4+ messages, use the summarization reducer
//         // Create a reducer that will summarize all messages except the last 2.
//         // - targetCount: 2 => keep the last 2 messages unmodified
//         // - thresholdCount: 4 => only trigger summarization if there are at least 4 messages in the history
//         // When reduction is triggered, a single summary message (covering all older messages) is inserted
//         // before the final 2, resulting in a history like: [Summary of older messages], [second-last message], [last message]
//         var reducer = new ChatHistorySummarizationReducer(_chatCompletion, targetCount: 2, thresholdCount: 4);
//
//         var reducedMessages = await reducer.ReduceAsync(fullHistory, cancellationToken);
//
//         if (reducedMessages is null)
//         {
//             // Fallback: if summarization fails, return the last 3 messages
//             var fallbackMessages = fullHistory.TakeLast(3).ToList();
//             var fallbackHistory = new ChatHistory();
//             foreach (var msg in fallbackMessages)
//             {
//                 fallbackHistory.Add(msg);
//             }
//             return fallbackHistory;
//         }
//
//         var reducedHistory = new ChatHistory();
//         foreach (var msg in reducedMessages)
//         {
//             reducedHistory.Add(msg);
//         }
//
//         return reducedHistory;
//     }
// }
