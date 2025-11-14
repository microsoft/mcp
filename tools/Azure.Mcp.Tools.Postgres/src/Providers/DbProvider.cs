using System.Data.Common;
using Npgsql;

namespace Azure.Mcp.Tools.Postgres.Providers
{
    internal class DbProvider : IDbProvider
    {
        public async Task<IPostgresResource> GetPostgresResource(string connectionString, string authType)
        {
            return await PostgresResource.CreateAsync(connectionString, authType);
        }

        public NpgsqlCommand GetCommand(string query, IPostgresResource postgresResource)
        {
            return new NpgsqlCommand(query, postgresResource.Connection);
        }

        public async Task<DbDataReader> ExecuteReaderAsync(NpgsqlCommand command)
        {
            return await command.ExecuteReaderAsync();
        }
    }
}
