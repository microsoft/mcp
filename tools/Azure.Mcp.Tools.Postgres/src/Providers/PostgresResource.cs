using Npgsql;

namespace Azure.Mcp.Tools.Postgres.Providers
{
    internal class PostgresResource : IPostgresResource
    {
        public NpgsqlConnection Connection { get; }
        private readonly NpgsqlDataSource _dataSource;

        public static async Task<PostgresResource> CreateAsync(string connectionString)
        {
            var dataSource = new NpgsqlSlimDataSourceBuilder(connectionString)
                .EnableTransportSecurity()
                .Build();
            var connection = await dataSource.OpenConnectionAsync();
            return new PostgresResource(dataSource, connection);
        }

        public async ValueTask DisposeAsync()
        {
            await Connection.DisposeAsync();
            await _dataSource.DisposeAsync();
        }

        private PostgresResource(NpgsqlDataSource dataSource, NpgsqlConnection connection)
        {
            _dataSource = dataSource;
            Connection = connection;
        }
    }
}
