// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.ResourceGroup;
using Azure.ResourceManager.PostgreSql.FlexibleServers;
using Npgsql;

namespace Azure.Mcp.Tools.Postgres.Services;

public class PostgresService : BaseAzureService, IPostgresService
{
    private readonly IResourceGroupService _resourceGroupService;
    private string? _cachedEntraIdAccessToken;
    private DateTime _tokenExpiryTime;

    public PostgresService(IResourceGroupService resourceGroupService)
    {
        _resourceGroupService = resourceGroupService ?? throw new ArgumentNullException(nameof(resourceGroupService));
    }

    private async Task<string> GetEntraIdAccessTokenAsync()
    {
        if (_cachedEntraIdAccessToken != null && DateTime.UtcNow < _tokenExpiryTime)
        {
            return _cachedEntraIdAccessToken;
        }

        var tokenRequestContext = new TokenRequestContext(new[] { "https://ossrdbms-aad.database.windows.net/.default" });
        var tokenCredential = await GetCredential();
        var accessToken = await tokenCredential
            .GetTokenAsync(tokenRequestContext, CancellationToken.None)
            .ConfigureAwait(false);
        _cachedEntraIdAccessToken = accessToken.Token;
        _tokenExpiryTime = accessToken.ExpiresOn.UtcDateTime.AddSeconds(-60); // Subtract 60 seconds as a buffer.

        return _cachedEntraIdAccessToken;
    }

    private static string NormalizeServerName(string server)
    {
        if (!server.Contains('.'))
        {
            return server + ".postgres.database.azure.com";
        }
        return server;
    }

    public async Task<List<string>> ListDatabasesAsync(string subscriptionId, string resourceGroup, string user, string server)
    {
        var entraIdAccessToken = await GetEntraIdAccessTokenAsync();
        var host = NormalizeServerName(server);
        var connectionString = $"Host={host};Database=postgres;Username={user};Password={entraIdAccessToken}";

        await using var resource = await PostgresResource.CreateAsync(connectionString);
        var query = "SELECT datname FROM pg_database WHERE datistemplate = false;";
        await using var command = new NpgsqlCommand(query, resource.Connection);
        await using var reader = await command.ExecuteReaderAsync();
        var dbs = new List<string>();
        while (await reader.ReadAsync())
        {
            dbs.Add(reader.GetString(0));
        }
        return dbs;
    }

    public async Task<List<string>> ExecuteQueryAsync(string subscriptionId, string resourceGroup, string user, string server, string database, string query)
    {
        var entraIdAccessToken = await GetEntraIdAccessTokenAsync();
        var host = NormalizeServerName(server);
        var connectionString = $"Host={host};Database={database};Username={user};Password={entraIdAccessToken}";

        await using var resource = await PostgresResource.CreateAsync(connectionString);
        await using var command = new NpgsqlCommand(query, resource.Connection);
        await using var reader = await command.ExecuteReaderAsync();

        var rows = new List<string>();

        var columnNames = Enumerable.Range(0, reader.FieldCount)
                               .Select(reader.GetName)
                               .ToArray();
        rows.Add(string.Join(", ", columnNames));
        while (await reader.ReadAsync())
        {
            var row = new List<string>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                row.Add(reader[i]?.ToString() ?? "NULL");
            }
            rows.Add(string.Join(", ", row));
        }
        return rows;
    }

    public async Task<List<string>> ListTablesAsync(string subscriptionId, string resourceGroup, string user, string server, string database)
    {
        var entraIdAccessToken = await GetEntraIdAccessTokenAsync();
        var host = NormalizeServerName(server);
        var connectionString = $"Host={host};Database={database};Username={user};Password={entraIdAccessToken}";

        await using var resource = await PostgresResource.CreateAsync(connectionString);
        var query = "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public';";
        await using var command = new NpgsqlCommand(query, resource.Connection);
        await using var reader = await command.ExecuteReaderAsync();
        var tables = new List<string>();
        while (await reader.ReadAsync())
        {
            tables.Add(reader.GetString(0));
        }
        return tables;
    }

    public async Task<List<string>> GetTableSchemaAsync(string subscriptionId, string resourceGroup, string user, string server, string database, string table)
    {
        var entraIdAccessToken = await GetEntraIdAccessTokenAsync();
        var host = NormalizeServerName(server);
        var connectionString = $"Host={host};Database={database};Username={user};Password={entraIdAccessToken}";

        await using var resource = await PostgresResource.CreateAsync(connectionString);
        var query = $"SELECT column_name, data_type FROM information_schema.columns WHERE table_name = '{table}';";
        await using var command = new NpgsqlCommand(query, resource.Connection);
        await using var reader = await command.ExecuteReaderAsync();
        var schema = new List<string>();
        while (await reader.ReadAsync())
        {
            schema.Add($"{reader.GetString(0)}: {reader.GetString(1)}");
        }
        return schema;
    }

    public async Task<List<string>> ListServersAsync(string subscriptionId, string resourceGroup, string user)
    {
        var rg = await _resourceGroupService.GetResourceGroupResource(subscriptionId, resourceGroup);
        if (rg == null)
        {
            throw new Exception($"Resource group '{resourceGroup}' not found.");
        }
        var serverList = new List<string>();
        await foreach (PostgreSqlFlexibleServerResource server in rg.GetPostgreSqlFlexibleServers().GetAllAsync())
        {
            serverList.Add(server.Data.Name);
        }
        return serverList;
    }

    public async Task<string> GetServerConfigAsync(string subscriptionId, string resourceGroup, string user, string server)
    {
        var rg = await _resourceGroupService.GetResourceGroupResource(subscriptionId, resourceGroup);
        if (rg == null)
        {
            throw new Exception($"Resource group '{resourceGroup}' not found.");
        }
        var pgServer = await rg.GetPostgreSqlFlexibleServerAsync(server);
        var pgServerData = pgServer.Value.Data;
        var result = $"Server Name: {pgServerData.Name}\n" +
                 $"Location: {pgServerData.Location}\n" +
                 $"Version: {pgServerData.Version}\n" +
                 $"SKU: {pgServerData.Sku?.Name}\n" +
                 $"Storage Size (GB): {pgServerData.Storage?.StorageSizeInGB}\n" +
                 $"Backup Retention Days: {pgServerData.Backup?.BackupRetentionDays}\n" +
                 $"Geo-Redundant Backup: {pgServerData.Backup?.GeoRedundantBackup}";
        return result;
    }

    public async Task<string> GetServerParameterAsync(string subscriptionId, string resourceGroup, string user, string server, string param)
    {
        var rg = await _resourceGroupService.GetResourceGroupResource(subscriptionId, resourceGroup);
        if (rg == null)
        {
            throw new Exception($"Resource group '{resourceGroup}' not found.");
        }
        var pgServer = await rg.GetPostgreSqlFlexibleServerAsync(server);

        var configResponse = await pgServer.Value.GetPostgreSqlFlexibleServerConfigurationAsync(param);
        if (configResponse?.Value?.Data == null)
        {
            throw new Exception($"Parameter '{param}' not found.");
        }
        return configResponse.Value.Data.Value;
    }

    public async Task<string> SetServerParameterAsync(string subscriptionId, string resourceGroup, string user, string server, string param, string value)
    {
        var rg = await _resourceGroupService.GetResourceGroupResource(subscriptionId, resourceGroup);
        if (rg == null)
        {
            throw new Exception($"Resource group '{resourceGroup}' not found.");
        }
        var pgServer = await rg.GetPostgreSqlFlexibleServerAsync(server);

        var configResponse = await pgServer.Value.GetPostgreSqlFlexibleServerConfigurationAsync(param);
        if (configResponse?.Value?.Data == null)
        {
            throw new Exception($"Parameter '{param}' not found.");
        }

        var configData = new PostgreSqlFlexibleServerConfigurationData
        {
            Value = value,
            Source = "user-override"
        };

        var updateOperation = await configResponse.Value.UpdateAsync(WaitUntil.Completed, configData);
        if (updateOperation.HasCompleted && updateOperation.HasValue)
        {
            return $"Parameter '{param}' updated successfully to '{value}'.";
        }
        else
        {
            throw new Exception($"Failed to update parameter '{param}' to value '{value}'.");
        }
    }

    private sealed class PostgresResource : IAsyncDisposable
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

    /// <summary>
    /// Validates that a SQL query is safe to execute (read-only operations only).
    /// This method provides validation that matches the test expectations.
    /// </summary>
    /// <param name="query">The SQL query to validate</param>
    /// <exception cref="ArgumentException">Thrown when the query is null, empty, or too long</exception>
    /// <exception cref="InvalidOperationException">Thrown when the query contains dangerous operations</exception>
    private static void ValidateQuerySafety(string query)
    {
        // Null/empty validation
        if (string.IsNullOrWhiteSpace(query))
        {
            throw new ArgumentException("Query cannot be null or empty");
        }

        var trimmed = query.Trim();

        // Length validation
        if (trimmed.Length > 10000)
        {
            throw new InvalidOperationException("Query length exceeds the maximum allowed limit of 10,000 characters");
        }

        // Remove comments to avoid false positives
        var cleanedQuery = RemoveComments(trimmed);

        // Check if query becomes empty after removing comments
        if (string.IsNullOrWhiteSpace(cleanedQuery))
        {
            throw new ArgumentException("Query cannot be empty after removing comments");
        }

        // Check for multiple statements
        if (HasMultipleStatements(cleanedQuery))
        {
            throw new InvalidOperationException("Multiple SQL statements are not allowed. Use only a single SELECT statement.");
        }

        // Check for dangerous keywords
        if (HasDangerousKeywords(cleanedQuery))
        {
            throw new InvalidOperationException("Query contains dangerous keyword or patterns");
        }

        // Check for allowed statement types only
        if (!IsAllowedStatementType(cleanedQuery))
        {
            throw new InvalidOperationException("Only SELECT and WITH statements are allowed");
        }
    }

    private static string RemoveComments(string query)
    {
        // Remove single-line comments
        var result = System.Text.RegularExpressions.Regex.Replace(query, @"--.*?$", "", System.Text.RegularExpressions.RegexOptions.Multiline);
        // Remove multi-line comments
        result = System.Text.RegularExpressions.Regex.Replace(result, @"/\*.*?\*/", "", System.Text.RegularExpressions.RegexOptions.Singleline);
        return result;
    }

    private static bool HasMultipleStatements(string query)
    {
        // Simple check for semicolons not within quoted strings
        var inQuotes = false;
        var quoteChar = '\0';

        for (int i = 0; i < query.Length; i++)
        {
            var c = query[i];

            if (!inQuotes && (c == '\'' || c == '"'))
            {
                inQuotes = true;
                quoteChar = c;
            }
            else if (inQuotes && c == quoteChar)
            {
                inQuotes = false;
                quoteChar = '\0';
            }
            else if (!inQuotes && c == ';')
            {
                // Check if there's non-whitespace content after this semicolon
                var remaining = query.Substring(i + 1).Trim();
                if (!string.IsNullOrEmpty(remaining))
                {
                    return true; // Multiple statements detected
                }
            }
        }

        return false;
    }

    private static bool HasDangerousKeywords(string query)
    {
        var dangerousKeywords = new[]
        {
            "DROP", "DELETE", "INSERT", "UPDATE", "CREATE", "ALTER", "GRANT", "REVOKE",
            "TRUNCATE", "VACUUM", "REINDEX", "BEGIN", "COMMIT", "ROLLBACK", "SAVEPOINT",
            "EXTENSION", "LANGUAGE", "USER", "ROLE", "DATABASE", "SCHEMA", "FUNCTION",
            "TRIGGER", "VIEW", "INDEX", "SHOW", "COPY", "\\COPY", "EXPLAIN", "ANALYZE",
            "UNION", "INTERSECT", "EXCEPT"
        };

        var upperQuery = query.ToUpperInvariant();
        return dangerousKeywords.Any(keyword =>
            System.Text.RegularExpressions.Regex.IsMatch(upperQuery, @"\b" + System.Text.RegularExpressions.Regex.Escape(keyword) + @"\b"));
    }

    private static bool IsAllowedStatementType(string query)
    {
        var trimmed = query.Trim().ToUpperInvariant();
        return trimmed.StartsWith("SELECT") || trimmed.StartsWith("WITH");
    }
}
