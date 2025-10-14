using GenAIEshop.Catalogs.Products.Dtos;
using GenAIEshop.Catalogs.Products.Models;

namespace GenAIEshop.Catalogs.Products;

public static class ProductMapper
{
    public static ProductDto ToDto(this Product product)
    {
        ArgumentNullException.ThrowIfNull(product, nameof(product));

        return new ProductDto(
            Id: product.Id,
            Name: product.Name,
            Description: product.Description,
            Price: product.Price,
            IsAvailable: product.IsAvailable,
            ImageUrl: product.ImageUrl
        );
    }

    public static List<ProductDto> ToDtoList(this IEnumerable<Product> products)
    {
        return products?.Select(ToDto).ToList() ?? new List<ProductDto>();
    }

    public static Product ToDomain(this ProductDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto, nameof(dto));

        return new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            IsAvailable = dto.IsAvailable,
            ImageUrl = dto.ImageUrl,
        };
    }

    public static void UpdateFromDto(this Product product, ProductDto dto)
    {
        ArgumentNullException.ThrowIfNull(product, nameof(product));
        ArgumentNullException.ThrowIfNull(dto, nameof(dto));

        product.Name = dto.Name;
        product.Description = dto.Description;
        product.Price = dto.Price;
        product.IsAvailable = dto.IsAvailable;
        product.ImageUrl = dto.ImageUrl;
    }
}
