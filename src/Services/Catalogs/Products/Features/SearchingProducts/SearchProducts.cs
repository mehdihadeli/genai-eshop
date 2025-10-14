using System.Linq.Expressions;
using BuildingBlocks.EF;
using BuildingBlocks.Extensions;
using BuildingBlocks.VectorDB.Contracts;
using GenAIEshop.Catalogs.Products.Data.VectorModel;
using GenAIEshop.Catalogs.Products.Dtos;
using GenAIEshop.Catalogs.Products.Models;
using GenAIEshop.Catalogs.Shared.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace GenAIEshop.Catalogs.Products.Features.SearchingProducts;

public sealed record SearchProducts(
    string SearchTerm,
    IEnumerable<string> Keywords,
    SearchType SearchType,
    int PageNumber,
    int PageSize,
    double? Threshold = null
) : IQuery<SearchProductsResult>
{
    public static SearchProducts Of(
        string? searchTerm,
        IEnumerable<string>? keywords,
        SearchType searchType = SearchType.Regular,
        int pageNumber = 1,
        int pageSize = 10,
        double? threshold = null
    )
    {
        searchTerm.NotBeNullOrWhiteSpace();
        return new SearchProducts(
            searchTerm,
            keywords ?? [],
            searchType,
            pageNumber.NotBeNegativeOrZero(),
            pageSize.NotBeNegativeOrZero(),
            threshold
        );
    }

    public int Skip => (PageNumber - 1) * PageSize;
}

public sealed class SearchProductsHandler(
    CatalogsDbContext dbContext,
    ISemanticSearch semanticSearch,
    ILogger<SearchProductsHandler> logger
) : IQueryHandler<SearchProducts, SearchProductsResult>
{
    public async ValueTask<SearchProductsResult> Handle(SearchProducts query, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Searching products for '{SearchTerm}' with {SearchType} search, page {PageNumber}",
            query.SearchTerm,
            query.SearchType,
            query.PageNumber
        );

        IQueryable<Product> queryable = dbContext.Products;

        var totalCount = await queryable.CountAsync(cancellationToken);

        switch (query.SearchType)
        {
            case SearchType.Regular:
                queryable = ApplyRegularSearchFilters(queryable, query.SearchTerm, query.Keywords);

                var productsDto = await queryable
                    .OrderBy(p => p.Name)
                    .Skip(query.Skip)
                    .Take(query.PageSize)
                    .Select(p => p.ToDto())
                    .ToListAsync(cancellationToken);

                return new SearchProductsResult(
                    Products: productsDto.AsReadOnly(),
                    AIExplanationMessage: string.Empty,
                    SearchType: query.SearchType,
                    PageSize: query.PageSize,
                    TotalCount: totalCount
                );

            case SearchType.Semantic:
                var semanticSearchResult = await semanticSearch.SemanticSearchAsync<ProductVector, Product>(
                    searchTerm: query.SearchTerm,
                    dbContext: dbContext,
                    take: query.PageSize,
                    skip: query.Skip,
                    threshold: query.Threshold,
                    filter: x => x.IsAvailable == true,
                    cancellationToken: cancellationToken
                );

                return new SearchProductsResult(
                    Products: semanticSearchResult.Data.Select(p => p.ToDto()).ToList(),
                    AIExplanationMessage: semanticSearchResult.AIExplanationMessage,
                    SearchType: query.SearchType,
                    PageSize: query.PageSize,
                    TotalCount: totalCount
                );

            case SearchType.Hybrid:
                var hybridSearchResult = await semanticSearch.HybridSearchAsync<ProductVector, Product>(
                    searchTerm: query.SearchTerm,
                    keywords: query.Keywords.ToList(),
                    dbContext: dbContext,
                    take: query.PageSize,
                    skip: query.Skip,
                    threshold: query.Threshold,
                    filter: x => x.IsAvailable == true,
                    cancellationToken: cancellationToken
                );

                return new SearchProductsResult(
                    Products: hybridSearchResult.Data.Select(p => p.ToDto()).ToList(),
                    AIExplanationMessage: hybridSearchResult.AIExplanationMessage,
                    SearchType: query.SearchType,
                    PageSize: query.PageSize,
                    TotalCount: totalCount
                );

            default:
                throw new ArgumentException($"Unknown search type: {query.SearchType}");
        }
    }

    private static IQueryable<Product> ApplyRegularSearchFilters(
        IQueryable<Product> queryable,
        string searchTerm,
        IEnumerable<string> keywords
    )
    {
        queryable = queryable.Where(x => x.IsAvailable == true);

        var keywordList = keywords.Where(k => !string.IsNullOrWhiteSpace(k)).ToList();

        if (keywordList.Count > 0)
        {
            // Build the condition dynamically using a loop with OR logic
            Expression<Func<Product, bool>>? combinedCondition = null;

            // Start with the search term condition
            combinedCondition = p =>
                EF.Functions.ILike(p.Name, $"%{searchTerm}%")
                || (p.Description != null && EF.Functions.ILike(p.Description, $"%{searchTerm}%"));

            // Add each keyword with OR logic
            foreach (var keyword in keywordList)
            {
                var currentKeyword = keyword;
                var keywordCondition = BuildKeywordCondition(currentKeyword);

                combinedCondition = ExpressionBuilder.CombineWithOr(combinedCondition, keywordCondition);
            }

            queryable = queryable.Where(combinedCondition);
        }
        else
        {
            // Only search term filter when no keywords provided
            queryable = queryable.Where(p =>
                EF.Functions.ILike(p.Name, $"%{searchTerm}%")
                || (p.Description != null && EF.Functions.ILike(p.Description, $"%{searchTerm}%"))
            );
        }

        return queryable;

        static Expression<Func<Product, bool>> BuildKeywordCondition(string keyword)
        {
            return p =>
                EF.Functions.ILike(p.Name, $"%{keyword}%")
                || (p.Description != null && EF.Functions.ILike(p.Description, $"%{keyword}%"));
        }
    }
}

public sealed record SearchProductsResult(
    IReadOnlyCollection<ProductDto> Products,
    string AIExplanationMessage,
    SearchType SearchType,
    int PageSize,
    int TotalCount
)
{
    public int PageCount => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
