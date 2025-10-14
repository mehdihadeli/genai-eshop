using System.Data.Common;

namespace BuildingBlocks.EF;

public interface IConnectionFactory : IDisposable
{
    Task<DbConnection> GetOrCreateConnectionAsync();
}
