
using System.Data.Common;
using Npgsql;

namespace Azure.Mcp.Tools.Postgres.Providers
{
    public interface IDbProvider
    {
        Task<IPostgresResource> GetPostgresResource(string connectionString);
        NpgsqlCommand GetCommand(string query, IPostgresResource postgresResource);
        Task<DbDataReader> ExecuteReaderAsync(NpgsqlCommand command);
    }
}
