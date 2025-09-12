// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.RegularExpressions;
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

    // Maximum number of items to return to prevent DoS attacks and performance issues
    private const int MaxResultLimit = 10000;

    // Static arrays for security validation - initialized once per class
    private static readonly string[] DangerousKeywords =
    [
        // Data manipulation that could be harmful
        "DROP", "DELETE", "TRUNCATE", "ALTER", "CREATE", "INSERT", "UPDATE",
        // Administrative operations
        "GRANT", "REVOKE", "SET", "RESET", "KILL", "SHUTDOWN", "RESTART",
        // Information disclosure
        "SHOW", "EXPLAIN", "ANALYZE",
        // System operations
        "COPY", "\\COPY", "VACUUM", "REINDEX",
        // User/privilege management
        "CREATE USER", "DROP USER", "ALTER USER", "CREATE ROLE", "DROP ROLE",
        // Database structure changes
        "CREATE DATABASE", "DROP DATABASE", "CREATE SCHEMA", "DROP SCHEMA",
        // Stored procedures and functions
        "CREATE FUNCTION", "DROP FUNCTION", "CREATE PROCEDURE", "DROP PROCEDURE",
        // Triggers and events
        "CREATE TRIGGER", "DROP TRIGGER",
        // Views that could modify data
        "CREATE VIEW", "DROP VIEW",
        // Index operations
        "CREATE INDEX", "DROP INDEX",
        // Transaction control in unsafe contexts
        "BEGIN", "COMMIT", "ROLLBACK", "SAVEPOINT",
        // Extensions and languages
        "CREATE EXTENSION", "DROP EXTENSION", "CREATE LANGUAGE", "DROP LANGUAGE",
        // Union operations that could be used for data exfiltration
        "UNION", "UNION ALL"
    ];

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

    private static void ValidateQuerySafety(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            throw new ArgumentException("Query cannot be null or empty.", nameof(query));
        }

        // Prevent DoS attacks by limiting query length
        if (query.Length > MaxResultLimit)
        {
            throw new InvalidOperationException($"Query length exceeds the maximum allowed limit of {MaxResultLimit:N0} characters to prevent potential DoS attacks.");
        }

        // Clean the query: remove comments, normalize whitespace, and trim
        var cleanedQuery = query;

        // Remove line comments (-- comment)
        cleanedQuery = Regex.Replace(cleanedQuery, @"--.*?$", "", RegexOptions.Multiline);

        // Remove block comments (/* comment */)
        cleanedQuery = Regex.Replace(cleanedQuery, @"/\*.*?\*/", "", RegexOptions.Singleline);

        // Normalize whitespace: replace multiple whitespace characters with single space
        cleanedQuery = Regex.Replace(cleanedQuery, @"\s+", " ", RegexOptions.Multiline);

        // Trim the result
        cleanedQuery = cleanedQuery.Trim();

        // Ensure the cleaned query is not empty
        if (string.IsNullOrWhiteSpace(cleanedQuery))
        {
            throw new ArgumentException("Query cannot be empty after removing comments and whitespace.", nameof(query));
        }

        // Regex pattern to detect multiple SQL statements (semicolon not at end)
        var multipleStatementsPattern = new Regex(
            @";\s*\w",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );

        if (multipleStatementsPattern.IsMatch(cleanedQuery))
        {
            throw new InvalidOperationException("Multiple SQL statements are not allowed. Use only a single SELECT statement.");
        }

        // List of dangerous SQL keywords that should be blocked
        var queryUpper = cleanedQuery.ToUpperInvariant();

        foreach (var keyword in DangerousKeywords)
        {
            if (queryUpper.Contains(keyword))
            {
                throw new InvalidOperationException($"Query contains dangerous keyword '{keyword}' which is not allowed for security reasons.");
            }
        }

        // Additional validation: Only allow SELECT statements
        var trimmedQuery = queryUpper.Trim();
        var allowedStartPatterns = new[]
        {
            "SELECT", "WITH"
        };

        bool isAllowed = allowedStartPatterns.Any(pattern => trimmedQuery.StartsWith(pattern));

        if (!isAllowed)
        {
            throw new InvalidOperationException("Only SELECT and WITH statements are allowed for security reasons.");
        }
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
        var dbCount = 0;
        while (await reader.ReadAsync() && dbCount < MaxResultLimit)
        {
            dbs.Add(reader.GetString(0));
            dbCount++;
        }

        if (dbCount >= MaxResultLimit)
        {
            dbs.Add($"... (output limited to {MaxResultLimit:N0} databases for security and performance reasons)");
        }

        return dbs;
    }

    public async Task<List<string>> ExecuteQueryAsync(string subscriptionId, string resourceGroup, string user, string server, string database, string query)
    {
        ValidateQuerySafety(query);

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

        var rowCount = 0;
        while (await reader.ReadAsync() && rowCount < MaxResultLimit)
        {
            var row = new List<string>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                row.Add(reader[i]?.ToString() ?? "NULL");
            }
            rows.Add(string.Join(", ", row));
            rowCount++;
        }

        if (rowCount >= MaxResultLimit)
        {
            rows.Add($"... (output limited to {MaxResultLimit:N0} rows for security and performance reasons)");
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
        var tableCount = 0;
        while (await reader.ReadAsync() && tableCount < MaxResultLimit)
        {
            tables.Add(reader.GetString(0));
            tableCount++;
        }

        if (tableCount >= MaxResultLimit)
        {
            tables.Add($"... (output limited to {MaxResultLimit:N0} tables for security and performance reasons)");
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
}
