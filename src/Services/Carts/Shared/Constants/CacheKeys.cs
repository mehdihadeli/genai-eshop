namespace GenAIEshop.Carts.Shared.Constants;

public class CacheKeys
{
    public static string GetCartKey(Guid userId) => $"cart:{userId}";
}
