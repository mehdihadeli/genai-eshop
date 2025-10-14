using GenAIEshop.Catalogs.Products.Features.CreatingProduct;
using GenAIEshop.Catalogs.Products.Features.DeletingProduct;
using GenAIEshop.Catalogs.Products.Features.GettingProductById;
using GenAIEshop.Catalogs.Products.Features.GettingProducts;
using GenAIEshop.Catalogs.Products.Features.SearchingProducts;
using GenAIEshop.Catalogs.Products.Features.UpdatingProduct;
using GenAIEshop.Catalogs.Products.Models;
using Humanizer;

namespace GenAIEshop.Catalogs.Products;

public static class ProductsConfig
{
    public static IHostApplicationBuilder AddProductsServices(this IHostApplicationBuilder builder)
    {
        return builder;
    }

    public static IEndpointRouteBuilder MapProductsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var products = endpoints.NewVersionedApi(nameof(Product).Pluralize().Kebaberize());
        var productsV1 = products.MapGroup("/api/v{version:apiVersion}/products").HasApiVersion(1.0);

        productsV1.MapGetProductByIdEndpoint();
        productsV1.MapGetProductsByIdsEndpoint();
        productsV1.MapGetProductsEndpoint();
        productsV1.MapSearchProductsEndpoint();
        productsV1.MapCreateProductEndpoint();
        productsV1.MapUpdateProductEndpoint();
        productsV1.MapDeleteProductEndpoint();

        return endpoints;
    }
}
