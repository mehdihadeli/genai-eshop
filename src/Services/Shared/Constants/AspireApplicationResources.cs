using Humanizer;

namespace GenAIEshop.Shared.Constants;

public static class AspireApplicationResources
{
    public static class PostgresDatabase
    {
        private const string Postfix = "db";
        private const string Prefix = "pg";
        public static readonly string Catalogs = $"{Prefix}-{nameof(Catalogs).Kebaberize()}{Postfix}";
        public static readonly string Orders = $"{Prefix}-{nameof(Orders).Kebaberize()}{Postfix}";
        public static readonly string Reviews = $"{Prefix}-{nameof(Reviews).Kebaberize()}{Postfix}";
    }

    public static class Api
    {
        public static readonly string CatalogsApi = $"{nameof(CatalogsApi).Kebaberize()}";
        public static readonly string OrdersApi = $"{nameof(OrdersApi).Kebaberize()}";
        public static readonly string CartsApi = $"{nameof(CartsApi).Kebaberize()}";
        public static readonly string ReviewsApi = $"{nameof(ReviewsApi).Kebaberize()}";
        public static readonly string RecommendationApi = $"{nameof(RecommendationApi).Kebaberize()}";
        public static readonly string McpServerApi = $"{nameof(McpServerApi).Kebaberize()}";
    }
}
