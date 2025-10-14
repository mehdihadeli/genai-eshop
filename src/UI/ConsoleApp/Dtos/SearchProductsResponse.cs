using System.Collections.ObjectModel;
using ConsoleApp.Enums;

namespace ConsoleApp.Dtos;

public sealed record SearchProductsResponse(
    List<ProductDto> Products,
    string AIExplanationMessage,
    SearchType SearchType,
    int PageSize,
    int PageCount,
    int TotalCount
);
