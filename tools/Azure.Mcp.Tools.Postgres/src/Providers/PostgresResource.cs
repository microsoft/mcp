using System.Net;
using Azure.Mcp.Core.Exceptions;
using Azure.Mcp.Tools.Postgres.Options;
using Npgsql;

namespace Azure.Mcp.Tools.Postgres.Providers
{
    internal class PostgresResource : IPostgresResource
    {
        public NpgsqlConnection Connection { get; }
        private readonly NpgsqlDataSource _dataSource;

        public static async Task<PostgresResource> CreateAsync(string connectionString, string authType, CancellationToken cancellationToken)
        {
            // Configure SSL settings for secure connection
            var connectionBuilder = new NpgsqlConnectionStringBuilder(connectionString)
            {
                SslMode = SslMode.VerifyFull // See: https://www.npgsql.org/doc/security.html?tabs=tabid-1#encryption-ssltls
            };

            var dataSource = new NpgsqlSlimDataSourceBuilder(connectionBuilder.ConnectionString)
                .EnableTransportSecurity()
                .Build();

            NpgsqlConnection connection;
            try
            {
                connection = await dataSource.OpenConnectionAsync(cancellationToken);
            }
            catch (PostgresException e) when (e.Message.Contains("28P01"))
            {
                if (string.IsNullOrEmpty(authType))
                {
                    throw new CommandValidationException($"Authentication failed. No authentication type was provided so '{AuthTypes.MicrosoftEntra}' was used." +
                        $"Please ensure that the user has the necessary permissions or explicitly use another authentication mechanism like '{AuthTypes.PostgreSQL}' providing the user password.", HttpStatusCode.Unauthorized);
                }

                if (AuthTypes.MicrosoftEntra.Equals(authType, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new CommandValidationException($"Authentication failed using the request authentication type '{AuthTypes.MicrosoftEntra}'. " +
                        $"Please ensure that the user has the necessary permissions or explicitly use another authentication mechanism like '{AuthTypes.PostgreSQL}' providing the user password.", HttpStatusCode.Unauthorized);
                }

                if (AuthTypes.PostgreSQL.Equals(authType, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new CommandValidationException($"Authentication failed using the request authentication type '{AuthTypes.PostgreSQL}'. " +
                        $"Please ensure that the user has the necessary permissions or explicitly use another authentication mechanism like '{AuthTypes.MicrosoftEntra}'.", HttpStatusCode.Unauthorized);
                }

                throw;
            }
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
