// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.RegularExpressions;
using Azure;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.ResourceGroup;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.MySql.Json;
using Azure.ResourceManager.MySql.FlexibleServers;
using Microsoft.Extensions.Logging;
using MySqlConnector;

namespace Azure.Mcp.Tools.MySql.Services;

public class MySqlService(IResourceGroupService resourceGroupService, ITenantService tenantService, ILogger<MySqlService> logger) : BaseAzureService(tenantService), IMySqlService
{
    private readonly IResourceGroupService _resourceGroupService = resourceGroupService ?? throw new ArgumentNullException(nameof(resourceGroupService));
    private readonly ILogger<MySqlService> _logger = logger;
    private string? _cachedEntraIdAccessToken;
    private DateTime _tokenExpiryTime;
    private readonly object _tokenLock = new object();

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
        "SHOW MASTER", "SHOW SLAVE", "SHOW BINARY", "SHOW BINLOG",
        // System operations
        "LOAD DATA", "OUTFILE", "DUMPFILE", "LOAD_FILE", "INTO OUTFILE",
        // User/privilege management
        "CREATE USER", "DROP USER", "ALTER USER", "RENAME USER",
        // Database structure changes
        "CREATE DATABASE", "DROP DATABASE", "CREATE SCHEMA", "DROP SCHEMA",
        // Stored procedures and functions
        "CREATE PROCEDURE", "DROP PROCEDURE", "CREATE FUNCTION", "DROP FUNCTION",
        // Triggers and events
        "CREATE TRIGGER", "DROP TRIGGER", "CREATE EVENT", "DROP EVENT",
        // Views that could modify data
        "CREATE VIEW", "DROP VIEW",
        // Index operations
        "CREATE INDEX", "DROP INDEX",
        // Table operations
        "CREATE TABLE", "DROP TABLE", "RENAME TABLE",
        // Lock operations
        "LOCK TABLES", "UNLOCK TABLES",
        // Transaction control in unsafe contexts
        "START TRANSACTION", "BEGIN", "COMMIT", "ROLLBACK",
        // System variables
        "SET GLOBAL", "SET SESSION", "SET SQL_MODE"
    ];

    private static readonly string[] ObfuscationFunctions =
    [
        "CHAR(", "CHR(", "ASCII(", "ORD(", "HEX(", "UNHEX(", "CONV(",
        "CONVERT(", "CAST(", "BINARY(", "CONCAT_WS(", "MAKE_SET(",
        "ELT(", "FIELD(", "FIND_IN_SET(", "EXPORT_SET(", "LOAD_FILE(",
        "FROM_BASE64(", "TO_BASE64(", "COMPRESS(", "UNCOMPRESS(",
        "AES_ENCRYPT(", "AES_DECRYPT(", "DES_ENCRYPT(", "DES_DECRYPT(",
        "ENCODE(", "DECODE(", "PASSWORD(", "OLD_PASSWORD("
    ];

    private async Task<string> GetEntraIdAccessTokenAsync()
    {
        lock (_tokenLock)
        {
            if (_cachedEntraIdAccessToken != null && DateTime.UtcNow < _tokenExpiryTime)
            {
                return _cachedEntraIdAccessToken;
            }
        }

        var tokenRequestContext = new TokenRequestContext(new[] { "https://ossrdbms-aad.database.windows.net/.default" });
        var tokenCredential = await GetCredential();
        var accessToken = await tokenCredential
            .GetTokenAsync(tokenRequestContext, CancellationToken.None)
            .ConfigureAwait(false);

        lock (_tokenLock)
        {
            _cachedEntraIdAccessToken = accessToken.Token;
            _tokenExpiryTime = accessToken.ExpiresOn.UtcDateTime.AddSeconds(-60); // Subtract 60 seconds as a buffer.
        }

        return _cachedEntraIdAccessToken;
    }

    private static string NormalizeServerName(string server)
    {
        if (!server.Contains('.'))
        {
            return server + ".mysql.database.azure.com";
        }
        return server;
    }

    private async Task<string> BuildConnectionStringAsync(string server, string user, string database)
    {
        var entraIdAccessToken = await GetEntraIdAccessTokenAsync();
        var host = NormalizeServerName(server);
        return $"Server={host};Database={database};User ID={user};Password={entraIdAccessToken};SSL Mode=Required;";
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

        // Remove hash comments (# comment)
        cleanedQuery = Regex.Replace(cleanedQuery, @"#.*?$", "", RegexOptions.Multiline);

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

        // Check for character conversion functions that may be used for obfuscation
        foreach (var func in ObfuscationFunctions)
        {
            if (queryUpper.Contains(func))
            {
                throw new InvalidOperationException($"Character conversion and obfuscation functions like '{func.TrimEnd('(')}' are not allowed for security reasons.");
            }
        }

        // Additional validation: Only allow SELECT statements
        var trimmedQuery = queryUpper.Trim();
        var allowedStartPatterns = new[]
        {
            "SELECT"
        };

        bool isAllowed = allowedStartPatterns.Any(pattern => trimmedQuery.StartsWith(pattern));

        if (!isAllowed)
        {
            throw new InvalidOperationException("Only SELECT statements are allowed for security reasons.");
        }
    }

    public async Task<List<string>> ListDatabasesAsync(string subscriptionId, string resourceGroup, string user, string server)
    {
        try
        {
            var connectionString = await BuildConnectionStringAsync(server, user, "mysql");

            await using var resource = await MySqlResource.CreateAsync(connectionString);
            var query = "SHOW DATABASES;";
            await using var command = new MySqlCommand(query, resource.Connection);
            await using var reader = await command.ExecuteReaderAsync();
            var dbs = new List<string>();
            var dbCount = 0;
            while (await reader.ReadAsync() && dbCount < MaxResultLimit)
            {
                var dbName = reader.GetString(0);
                // Filter out system databases
                if (dbName != "information_schema" && dbName != "mysql" && dbName != "performance_schema" && dbName != "sys")
                {
                    dbs.Add(dbName);
                    dbCount++;
                }
            }

            if (dbCount >= MaxResultLimit)
            {
                dbs.Add($"... (output limited to {MaxResultLimit:N0} databases for security and performance reasons)");
            }

            return dbs;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error listing MySQL databases. Server: {Server}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
                server, resourceGroup, subscriptionId);
            throw;
        }
    }

    public async Task<List<string>> ExecuteQueryAsync(string subscriptionId, string resourceGroup, string user, string server, string database, string query)
    {
        try
        {
            ValidateQuerySafety(query);

            var connectionString = await BuildConnectionStringAsync(server, user, database);

            await using var resource = await MySqlResource.CreateAsync(connectionString);
            await using var command = new MySqlCommand(query, resource.Connection);
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
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error executing MySQL query. Server: {Server}, Database: {Database}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
                server, database, resourceGroup, subscriptionId);
            throw;
        }
    }

    public async Task<List<string>> GetTableSchemaAsync(string subscriptionId, string resourceGroup, string user, string server, string database, string table)
    {
        try
        {
            var connectionString = await BuildConnectionStringAsync(server, user, database);

            await using var resource = await MySqlResource.CreateAsync(connectionString);
            var query = "SELECT column_name, data_type FROM information_schema.columns WHERE table_schema = DATABASE() AND table_name = @table;";
            await using var command = new MySqlCommand(query, resource.Connection);
            command.Parameters.AddWithValue("@table", table);
            await using var reader = await command.ExecuteReaderAsync();
            var schema = new List<string>();
            while (await reader.ReadAsync())
            {
                schema.Add($"{reader.GetString(0)}: {reader.GetString(1)}");
            }
            return schema;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting MySQL table schema. Server: {Server}, Database: {Database}, Table: {Table}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
                server, database, table, resourceGroup, subscriptionId);
            throw;
        }
    }

    public async Task<List<string>> ListServersAsync(string subscriptionId, string resourceGroup, string user)
    {
        try
        {
            var rg = await _resourceGroupService.GetResourceGroupResource(subscriptionId, resourceGroup);
            if (rg == null)
            {
                throw new Exception($"Resource group '{resourceGroup}' not found.");
            }
            var serverList = new List<string>();
            await foreach (MySqlFlexibleServerResource server in rg.GetMySqlFlexibleServers().GetAllAsync())
            {
                serverList.Add(server.Data.Name);
            }
            return serverList;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error listing MySQL servers. ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
                resourceGroup, subscriptionId);
            throw;
        }
    }

    public async Task<List<string>> GetTablesAsync(string subscriptionId, string resourceGroup, string user, string server, string database)
    {
        try
        {
            var connectionString = await BuildConnectionStringAsync(server, user, database);

            await using var resource = await MySqlResource.CreateAsync(connectionString);
            var query = "SELECT table_name FROM information_schema.tables WHERE table_schema = DATABASE();";
            await using var command = new MySqlCommand(query, resource.Connection);
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
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error listing MySQL tables. Server: {Server}, Database: {Database}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
                server, database, resourceGroup, subscriptionId);
            throw;
        }
    }

    public async Task<string> GetServerConfigAsync(string subscriptionId, string resourceGroup, string user, string server)
    {
        try
        {
            var rg = await _resourceGroupService.GetResourceGroupResource(subscriptionId, resourceGroup);
            if (rg == null)
            {
                throw new Exception($"Resource group '{resourceGroup}' not found.");
            }
            var mysqlServer = await rg.GetMySqlFlexibleServerAsync(server);
            var mysqlServerData = mysqlServer.Value.Data;
            var config = new ServerConfigGetResult
            {
                ServerName = mysqlServerData.Name,
                Location = mysqlServerData.Location.ToString(),
                Version = mysqlServerData.Version?.ToString(),
                SKU = mysqlServerData.Sku?.Name,
                StorageSizeGB = mysqlServerData.Storage?.StorageSizeInGB,
                BackupRetentionDays = mysqlServerData.Backup?.BackupRetentionDays,
                GeoRedundantBackup = mysqlServerData.Backup?.GeoRedundantBackup?.ToString()
            };
            return System.Text.Json.JsonSerializer.Serialize(config, MySqlJsonContext.Default.ServerConfigGetResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting MySQL server configuration. Server: {Server}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
                server, resourceGroup, subscriptionId);
            throw;
        }
    }

    public async Task<string> GetServerParameterAsync(string subscriptionId, string resourceGroup, string user, string server, string param)
    {
        try
        {
            var rg = await _resourceGroupService.GetResourceGroupResource(subscriptionId, resourceGroup);
            if (rg == null)
            {
                throw new Exception($"Resource group '{resourceGroup}' not found.");
            }
            var mysqlServer = await rg.GetMySqlFlexibleServerAsync(server);

            var configResponse = await mysqlServer.Value.GetMySqlFlexibleServerConfigurationAsync(param);
            if (configResponse?.Value?.Data == null)
            {
                throw new Exception($"Parameter '{param}' not found.");
            }
            return configResponse.Value.Data.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting MySQL server parameter. Server: {Server}, Parameter: {Parameter}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
                server, param, resourceGroup, subscriptionId);
            throw;
        }
    }

    public async Task<string> SetServerParameterAsync(string subscriptionId, string resourceGroup, string user, string server, string param, string value)
    {
        try
        {
            var rg = await _resourceGroupService.GetResourceGroupResource(subscriptionId, resourceGroup);
            if (rg == null)
            {
                throw new Exception($"Resource group '{resourceGroup}' not found.");
            }
            var mysqlServer = await rg.GetMySqlFlexibleServerAsync(server);

            var configuration = await mysqlServer.Value.GetMySqlFlexibleServerConfigurationAsync(param);
            if (configuration?.Value?.Data == null)
            {
                throw new Exception($"Parameter '{param}' not found.");
            }

            var configData = configuration.Value.Data;
            configData.Value = value;

            var updateOperation = await mysqlServer.Value.GetMySqlFlexibleServerConfigurations().CreateOrUpdateAsync(WaitUntil.Completed, param, configData);
            return updateOperation.Value.Data.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error setting MySQL server parameter. Server: {Server}, Parameter: {Parameter}, Value: {Value}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
                server, param, value, resourceGroup, subscriptionId);
            throw;
        }
    }

    private sealed class MySqlResource : IAsyncDisposable
    {
        public MySqlConnection Connection { get; }

        public static async Task<MySqlResource> CreateAsync(string connectionString)
        {
            var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();
            return new MySqlResource(connection);
        }

        public async ValueTask DisposeAsync()
        {
            await Connection.DisposeAsync();
        }

        private MySqlResource(MySqlConnection connection)
        {
            Connection = connection;
        }
    }

    public class ServerConfigGetResult
    {
        public string? ServerName { get; set; }
        public string? Location { get; set; }
        public string? Version { get; set; }
        public string? SKU { get; set; }
        public int? StorageSizeGB { get; set; }
        public int? BackupRetentionDays { get; set; }
        public string? GeoRedundantBackup { get; set; }
    }
}
