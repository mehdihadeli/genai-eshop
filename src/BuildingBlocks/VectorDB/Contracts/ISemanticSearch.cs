using System.Linq.Expressions;
using BuildingBlocks.Types;
using BuildingBlocks.VectorDB.SearchServices;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.VectorDB.Contracts;

public interface ISemanticSearch
{
    Task<VectorSearchResult<TEntity>> HybridSearchAsync<TVectorEntity, TEntity>(
        string searchTerm,
        ICollection<string> keywords,
        DbContext dbContext,
        int take = 10,
        int skip = 0,
        double? threshold = null,
        Expression<Func<TVectorEntity, bool>>? filter = null,
        Expression<Func<TVectorEntity, object?>>? fullTextSearchFiled = null,
        CancellationToken cancellationToken = default
    )
        where TVectorEntity : VectorEntityBase
        where TEntity : Entity;

    Task<VectorSearchResult<TEntity>> SemanticSearchAsync<TVectorEntity, TEntity>(
        string searchTerm,
        DbContext dbContext,
        int take = 10,
        int skip = 0,
        double? threshold = null,
        Expression<Func<TVectorEntity, bool>>? filter = null,
        CancellationToken cancellationToken = default
    )
        where TVectorEntity : VectorEntityBase
        where TEntity : Entity;
}
