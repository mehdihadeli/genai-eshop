using System.Net;
using BuildingBlocks.Serialization;
using GenAIEshop.Orders.Shared.Contracts;
using GenAIEshop.Orders.Shared.Dtos;

namespace GenAIEshop.Orders.Shared.Clients;

public class CartsServiceClient(HttpClient httpClient, ILogger<CartsServiceClient> logger) : ICartsServiceClient
{
    private const string BasePath = "api/v1/carts";

    public async Task<CartDto?> GetCartAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"{BasePath}?userId={userId}";
            var response = await httpClient.GetAsync(url, cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();

            var cartDto = await response.Content.ReadFromJsonAsync<CartDto>(
                options: SystemTextJsonSerializerOptions.DefaultSerializerOptions,
                cancellationToken: cancellationToken
            );
            return cartDto;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to fetch cart for user {UserId} from Carts service", userId);
            throw;
        }
    }

    public async Task<bool> ClearCartAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"{BasePath}?userId={userId}";
            var response = await httpClient.DeleteAsync(url, cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return true;

            response.EnsureSuccessStatusCode();
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to clear cart for user {UserId} in Carts service", userId);
            return false;
        }
    }
}
