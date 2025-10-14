using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using BuildingBlocks.Serialization;
using ConsoleApp.Dtos;
using ConsoleApp.Enums;

namespace ConsoleApp;

class Program
{
    private static readonly HttpClient httpClient = new();
    private static readonly string catalogBaseUrl = "https://localhost:5001";
    private static readonly string recommendationBaseUrl = "https://localhost:2001";
    private static readonly string reviewsBaseUrl = "https://localhost:8001";

    static async Task Main(string[] args)
    {
        Console.WriteLine("====================================");
        Console.WriteLine("        API Client");
        Console.WriteLine("====================================");
        Console.WriteLine("Product Search, Recommendations and Reviews API Client");
        Console.WriteLine();

        while (true)
        {
            var choice = DisplayMainMenu();

            if (choice == "q" || choice == "Q")
            {
                break;
            }

            if (int.TryParse(choice, out int endpointNumber) && endpointNumber >= 1 && endpointNumber <= 8)
            {
                await CallEndpoint(endpointNumber);

                // Wait for user to press Enter before showing main menu again
                WaitForEnterKey();
            }
            else
            {
                Console.WriteLine("Invalid selection. Please try again.");
                WaitForEnterKey();
            }
        }

        Console.WriteLine("Goodbye! 👋");
    }

    static string DisplayMainMenu()
    {
        Console.WriteLine("====================================");
        Console.WriteLine("      Available Endpoints");
        Console.WriteLine("====================================");
        Console.WriteLine("1. Search Products - REGULAR Search");
        Console.WriteLine("2. Search Products - SEMANTIC Search");
        Console.WriteLine("3. Search Products - HYBRID Search");
        Console.WriteLine("4. Get Personalized Recommendations");
        Console.WriteLine("5. Get AI-Powered Recommendations");
        Console.WriteLine("6. Analyze Reviews - NORMAL Mode");
        Console.WriteLine("7. Analyze Reviews - SEQUENTIAL Mode");
        Console.WriteLine("8. Analyze Reviews - GROUP-CHAT Mode");
        Console.WriteLine();

        Console.Write("Enter endpoint number (1-8) or 'q' to quit: ");
        return Console.ReadLine()?.Trim() ?? "";
    }

    static async Task CallEndpoint(int endpointNumber)
    {
        try
        {
            Console.WriteLine($"Calling API...");

            switch (endpointNumber)
            {
                case 1:
                case 2:
                case 3:
                    await CallSearchEndpoint(endpointNumber);
                    break;
                case 4:
                    await CallPersonalizedRecommendations();
                    break;
                case 5:
                    await CallAIRecommendations();
                    break;
                case 6:
                case 7:
                case 8:
                    await CallAnalyzeReviews(endpointNumber);
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception occurred: {ex.Message}");
        }
    }

    static async Task CallSearchEndpoint(int searchType)
    {
        Console.WriteLine($"====================================");
        Console.WriteLine($"Search Products - {GetSearchTypeName(searchType)}");
        Console.WriteLine($"====================================");

        // Get common parameters
        Console.Write("Enter Search Term: ");
        var searchTerm = Console.ReadLine() ?? "";

        Console.Write("Enter Page Number (default: 1): ");
        var pageNumberInput = Console.ReadLine();
        var pageNumber = int.TryParse(pageNumberInput, out var pn) ? pn : 1;

        Console.Write("Enter Page Size (default: 5): ");
        var pageSizeInput = Console.ReadLine();
        var pageSize = int.TryParse(pageSizeInput, out var ps) ? ps : 5;

        var queryParams = new Dictionary<string, string>
        {
            ["SearchTerm"] = searchTerm,
            ["PageNumber"] = pageNumber.ToString(),
            ["PageSize"] = pageSize.ToString(),
        };

        switch (searchType)
        {
            case 1: // Regular Search
                Console.Write("Enter Keywords (comma-separated, optional): ");
                var keywordsInput = Console.ReadLine() ?? "";
                if (!string.IsNullOrEmpty(keywordsInput))
                {
                    var keywords = keywordsInput.Split(',');
                    foreach (var keyword in keywords)
                    {
                        queryParams["Keywords"] = keyword.Trim();
                    }
                }
                queryParams["SearchType"] = "Regular";
                break;

            case 2: // Semantic Search
                queryParams["SearchType"] = "Semantic";
                break;

            case 3: // Hybrid Search
                Console.Write("Enter Keywords (comma-separated, optional): ");
                keywordsInput = Console.ReadLine() ?? "";
                if (!string.IsNullOrEmpty(keywordsInput))
                {
                    var keywords = keywordsInput.Split(',');
                    foreach (var keyword in keywords)
                    {
                        queryParams["Keywords"] = keyword.Trim();
                    }
                }
                queryParams["SearchType"] = "Hybrid";
                break;
        }

        // Build URL with query parameters
        var url = $"{catalogBaseUrl}/api/v1/products/search?";
        bool firstParam = true;
        foreach (var param in queryParams)
        {
            if (!firstParam)
                url += "&";
            url += $"{Uri.EscapeDataString(param.Key)}={Uri.EscapeDataString(param.Value)}";
            firstParam = false;
        }

        Console.WriteLine($"Calling: {url}");
        Console.WriteLine();

        // Make the HTTP request
        var response = await httpClient.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            var searchResponse = await response.Content.ReadFromJsonAsync<SearchProductsResponse>(
                SystemTextJsonSerializerOptions.DefaultSerializerOptions
            );
            DisplaySearchResults(searchResponse);
        }
        else
        {
            Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
            var errorContent = await response.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(errorContent))
            {
                Console.WriteLine($"Error details: {errorContent}");
            }
        }
    }

    static async Task CallPersonalizedRecommendations()
    {
        Console.WriteLine($"====================================");
        Console.WriteLine($"Personalized Recommendations");
        Console.WriteLine($"====================================");

        // Get user input with validation
        Guid userId;
        while (true)
        {
            Console.Write("Enter User ID (GUID format): ");
            var userIdInput = Console.ReadLine();
            if (Guid.TryParse(userIdInput, out userId))
                break;
            Console.WriteLine("Please enter a valid GUID");
        }

        Console.Write("Enter Query (optional): ");
        var query = Console.ReadLine() ?? "";

        Console.Write("Enter Preferences (optional): ");
        var preferences = Console.ReadLine() ?? "";

        Console.Write("Enter Category (optional): ");
        var category = Console.ReadLine() ?? "";

        var request = new GetPersonalizedRecommendationsRequest(
            UserId: userId,
            Query: string.IsNullOrEmpty(query) ? null : query,
            Preferences: string.IsNullOrEmpty(preferences) ? null : preferences,
            Category: string.IsNullOrEmpty(category) ? null : category
        );

        var url = $"{recommendationBaseUrl}/api/v1/recommendations/personalized-recommend";

        Console.WriteLine($"Calling: {url}");
        Console.WriteLine(
            $"Request: {JsonSerializer.Serialize(request, new JsonSerializerOptions { WriteIndented = true })}"
        );
        Console.WriteLine();

        var response = await httpClient.PostAsJsonAsync(url, request);

        if (response.IsSuccessStatusCode)
        {
            var recommendationResponse =
                await response.Content.ReadFromJsonAsync<GetPersonalizedRecommendationsResponse>();
            DisplayRecommendationResults(recommendationResponse, "Personalized");
        }
        else
        {
            Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
            var errorContent = await response.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(errorContent))
            {
                Console.WriteLine($"Error details: {errorContent}");
            }
        }
    }

    static async Task CallAIRecommendations()
    {
        Console.WriteLine($"====================================");
        Console.WriteLine($"AI-Powered Product Recommendations");
        Console.WriteLine($"====================================");

        Console.Write("Enter Query: ");
        var query = Console.ReadLine() ?? "";

        if (string.IsNullOrEmpty(query))
        {
            Console.WriteLine("Query is required for AI recommendations.");
            return;
        }

        var request = new GetProductRecommendationsRequest(Query: query);
        var url = $"{recommendationBaseUrl}/api/v1/recommendations/recommend";

        Console.WriteLine($"Calling: {url}");
        Console.WriteLine(
            $"Request: {JsonSerializer.Serialize(request, new JsonSerializerOptions { WriteIndented = true })}"
        );
        Console.WriteLine();

        var response = await httpClient.PostAsJsonAsync(url, request);

        if (response.IsSuccessStatusCode)
        {
            var recommendationResponse = await response.Content.ReadFromJsonAsync<GetProductRecommendationsResponse>();
            DisplayRecommendationResults(recommendationResponse, "AI-Powered");
        }
        else
        {
            Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
            var errorContent = await response.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(errorContent))
            {
                Console.WriteLine($"Error details: {errorContent}");
            }
        }
    }

    static async Task CallAnalyzeReviews(int endpointNumber)
    {
        var orchestrationType = GetOrchestrationType(endpointNumber);
        Console.WriteLine($"====================================");
        Console.WriteLine($"Analyze Reviews - {orchestrationType.ToString().ToUpper(CultureInfo.CurrentCulture)} Mode");
        Console.WriteLine($"====================================");

        Guid productId;
        while (true)
        {
            Console.Write("Enter Product ID (GUID format): ");
            var productIdInput = Console.ReadLine();
            if (Guid.TryParse(productIdInput, out productId))
                break;
            Console.WriteLine("Please enter a valid GUID");
        }

        var url = $"{reviewsBaseUrl}/api/v1/reviews/{productId}/analyze?AgentOrchestrationType={orchestrationType}";

        Console.WriteLine($"Calling: {url}");
        Console.WriteLine();

        var response = await httpClient.PostAsync(url, null);

        if (response.IsSuccessStatusCode)
        {
            var analysisResponse = await response.Content.ReadFromJsonAsync<AnalyzeProductReviewsResponse>();
            DisplayAnalysisResults(analysisResponse);
        }
        else
        {
            Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
            var errorContent = await response.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(errorContent))
            {
                Console.WriteLine($"Error details: {errorContent}");
            }
        }
    }

    static void DisplaySearchResults(SearchProductsResponse response)
    {
        if (response == null)
        {
            Console.WriteLine("No response received.");
            return;
        }

        Console.WriteLine("====================================");
        Console.WriteLine("           Search Results");
        Console.WriteLine("====================================");
        Console.WriteLine($"Search Type: {response.SearchType}");
        Console.WriteLine($"Page Size: {response.PageSize}");
        Console.WriteLine($"Page Count: {response.PageCount}");
        Console.WriteLine($"Total Count: {response.TotalCount}");

        // Display AI Explanation prominently
        if (!string.IsNullOrEmpty(response.AIExplanationMessage))
        {
            Console.WriteLine();
            Console.WriteLine("🤖 AI EXPLANATION:");
            Console.WriteLine("────────────────────────────────────");
            Console.WriteLine(response.AIExplanationMessage);
            Console.WriteLine("────────────────────────────────────");
        }
        Console.WriteLine();

        if (response.Products?.Count > 0)
        {
            Console.WriteLine($"📦 FOUND {response.Products.Count} PRODUCTS:");
            Console.WriteLine();

            for (int i = 0; i < response.Products.Count; i++)
            {
                var product = response.Products[i];
                DisplayProductCard(product, i + 1);

                // Add some spacing between products, but not after the last one
                if (i < response.Products.Count - 1)
                {
                    Console.WriteLine();
                    Console.WriteLine("──────────────────────────────────────────────────────────────────────────────");
                    Console.WriteLine();
                }
            }
        }
        else
        {
            Console.WriteLine("❌ No products found.");
        }
    }

    static void DisplayProductCard(ProductDto product, int index)
    {
        Console.WriteLine($"🛍️  PRODUCT #{index}");
        Console.WriteLine("──────────────────────────────────────────────────────────────────────────────");
        Console.WriteLine($"🔹 Name: {product.Name}");
        Console.WriteLine($"🔹 ID: {product.Id}");
        Console.WriteLine($"🔹 Price: ${product.Price}");
        Console.WriteLine();
        Console.WriteLine($"🔹 Description:");
        Console.WriteLine(FormatLongText(product.Description, "   "));

        Console.WriteLine("──────────────────────────────────────────────────────────────────────────────");
    }

    static string FormatLongText(string text, string indent = "")
    {
        if (string.IsNullOrEmpty(text))
            return indent + "(No description)";

        const int maxLineLength = 80;
        var lines = new List<string>();
        var currentLine = indent;

        var words = text.Split(' ');
        foreach (var word in words)
        {
            if (currentLine.Length + word.Length + 1 > maxLineLength)
            {
                lines.Add(currentLine);
                currentLine = indent + word;
            }
            else
            {
                if (currentLine != indent)
                    currentLine += " ";
                currentLine += word;
            }
        }

        if (currentLine != indent)
            lines.Add(currentLine);

        return string.Join(Environment.NewLine, lines);
    }

    static void DisplayRecommendationResults(object response, string recommendationType)
    {
        string recommendations = "";
        DateTime generatedAt = DateTime.Now;

        if (response is GetPersonalizedRecommendationsResponse personalizedResponse)
        {
            recommendations = personalizedResponse.Recommendations;
            generatedAt = personalizedResponse.GeneratedAt;
        }
        else if (response is GetProductRecommendationsResponse aiResponse)
        {
            recommendations = aiResponse.Recommendations;
            generatedAt = aiResponse.GeneratedAt;
        }
        else
        {
            Console.WriteLine("Invalid response type received.");
            return;
        }

        Console.WriteLine("====================================");
        Console.WriteLine($"{recommendationType} Recommendations - {generatedAt}");
        Console.WriteLine("====================================");
        Console.WriteLine(recommendations);
        Console.WriteLine("====================================");
    }

    static void DisplayAnalysisResults(AnalyzeProductReviewsResponse response)
    {
        if (response == null)
        {
            Console.WriteLine("No response received.");
            return;
        }

        Console.WriteLine("====================================");
        Console.WriteLine($"Review Analysis - {response.GeneratedAt}");
        Console.WriteLine("====================================");
        Console.WriteLine(response.Analysis);
        Console.WriteLine("====================================");
    }

    static string GetSearchTypeName(int searchType)
    {
        return searchType switch
        {
            1 => "REGULAR",
            2 => "SEMANTIC",
            3 => "HYBRID",
            _ => "UNKNOWN",
        };
    }

    static AgentOrchestrationType GetOrchestrationType(int endpointNumber)
    {
        return endpointNumber switch
        {
            6 => AgentOrchestrationType.Normal,
            7 => AgentOrchestrationType.Sequential,
            8 => AgentOrchestrationType.GroupChat,
            _ => AgentOrchestrationType.Normal,
        };
    }

    static void WaitForEnterKey()
    {
        Console.WriteLine();
        Console.WriteLine("Press Enter to return to main menu...");
        Console.ReadLine();
        Console.Clear();
    }
}
