// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Data;
using System.Data.Common;
using System.Net;
using Azure.Core;
using Azure.Mcp.Core.Exceptions;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.ResourceGroup;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.Postgres.Auth;
using Azure.Mcp.Tools.Postgres.Options;
using Azure.Mcp.Tools.Postgres.Providers;
using Azure.ResourceManager.PostgreSql.FlexibleServers;
using Npgsql;


namespace Azure.Mcp.Tools.Postgres.Services;

public class PostgresService : BaseAzureService, IPostgresService
{
    private readonly IResourceGroupService _resourceGroupService;
    private readonly IEntraTokenProvider _entraTokenAuth;
    private readonly IDbProvider _dbProvider;

    public PostgresService(
        IResourceGroupService resourceGroupService,
        ITenantService tenantService,
        IEntraTokenProvider entraTokenAuth,
        IDbProvider dbProvider)
        : base(tenantService)
    {
        _resourceGroupService = resourceGroupService ?? throw new ArgumentNullException(nameof(resourceGroupService));
        _entraTokenAuth = entraTokenAuth;
        _dbProvider = dbProvider;
    }

    private async Task<string> GetEntraIdAccessTokenAsync(CancellationToken cancellationToken)
    {
        TokenCredential tokenCredential = await GetCredential(cancellationToken);
        AccessToken accessToken = await _entraTokenAuth.GetEntraToken(tokenCredential, cancellationToken);

        return accessToken.Token;
    }

    private static string NormalizeServerName(string server)
    {
        if (!server.Contains('.'))
        {
            return server + ".postgres.database.azure.com";
        }
        return server;
    }

    public async Task<List<string>> ListDatabasesAsync(
        string subscriptionId,
        string resourceGroup,
        string authType,
        string user,
        string? password,
        string server,
        CancellationToken cancellationToken)
    {
        string? passwordToUse = await GetPassword(authType, password, cancellationToken);
        var host = NormalizeServerName(server);
        var connectionString = $"Host={host};Database=postgres;Username={user};Password={passwordToUse}";

        var query = "SELECT datname FROM pg_database WHERE datistemplate = false;";
        await using IPostgresResource resource = await _dbProvider.GetPostgresResource(connectionString, authType, cancellationToken);
        await using NpgsqlCommand command = _dbProvider.GetCommand(query, resource);
        await using DbDataReader reader = await _dbProvider.ExecuteReaderAsync(command, cancellationToken);
        var dbs = new List<string>();
        while (await reader.ReadAsync(cancellationToken))
        {
            dbs.Add(reader.GetString(0));
        }
        return dbs;
    }

    public async Task<List<string>> ExecuteQueryAsync(
        string subscriptionId,
        string resourceGroup,
        string authType,
        string user,
        string? password,
        string server,
        string database,
        string query,
        CancellationToken cancellationToken)
    {
        string? passwordToUse = await GetPassword(authType, password, cancellationToken);
        var host = NormalizeServerName(server);
        var connectionString = $"Host={host};Database={database};Username={user};Password={passwordToUse}";

        await using IPostgresResource resource = await _dbProvider.GetPostgresResource(connectionString, authType, cancellationToken);
        await using NpgsqlCommand command = _dbProvider.GetCommand(query, resource);
        await using DbDataReader reader = await _dbProvider.ExecuteReaderAsync(command, cancellationToken);

        var rows = new List<string>();

        var columnNames = Enumerable.Range(0, reader.FieldCount)
                               .Select(reader.GetName)
                               .ToArray();
        rows.Add(string.Join(", ", columnNames));
        while (await reader.ReadAsync(cancellationToken))
        {
            var row = new List<string>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                try
                {
                    row.Add(reader[i]?.ToString() ?? "NULL");
                }
                catch (InvalidCastException)
                {
                    throw new CommandValidationException($"E_QUERY_UNSUPPORTED_COMPLEX_TYPES. The PostgreSQL query failed because it returned one or more columns with non-standard data types (extension or user-defined) unsupported by the MCP agent.\nColumn that failed: '{columnNames[i]}'.\n" +
                        $"Action required:\n" +
                        $"1. Obtain the exact schema for all the tables involved in the query.\n" +
                        $"2. Identify which columns have non-standard data types.\n" +
                        $"3. Modify the query to convert them to a supported type (e.g. using CAST or converting to text, integer, or the appropriate standard type).\n" +
                        $"4. Re-execute the modified query.\n" +
                        $"Please perform steps 1-4 now and re-execute.", HttpStatusCode.BadRequest);
                }
            }
            rows.Add(string.Join(", ", row));
        }
        return rows;
    }

    public async Task<List<string>> ListTablesAsync(
        string subscriptionId,
        string resourceGroup,
        string authType,
        string user,
        string? password,
        string server,
        string database,
        CancellationToken cancellationToken)
    {
        string? passwordToUse = await GetPassword(authType, password, cancellationToken);
        var host = NormalizeServerName(server);
        var connectionString = $"Host={host};Database={database};Username={user};Password={passwordToUse}";

        var query = "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public';";
        await using IPostgresResource resource = await _dbProvider.GetPostgresResource(connectionString, authType, cancellationToken);
        await using NpgsqlCommand command = _dbProvider.GetCommand(query, resource);
        await using DbDataReader reader = await _dbProvider.ExecuteReaderAsync(command, cancellationToken);
        var tables = new List<string>();
        while (await reader.ReadAsync(cancellationToken))
        {
            tables.Add(reader.GetString(0));
        }
        return tables;
    }

    public async Task<List<string>> GetTableSchemaAsync(
        string subscriptionId,
        string resourceGroup,
        string authType,
        string user,
        string? password,
        string server,
        string database,
        string table,
        CancellationToken cancellationToken)
    {
        string? passwordToUse = await GetPassword(authType, password, cancellationToken);
        var host = NormalizeServerName(server);
        var connectionString = $"Host={host};Database={database};Username={user};Password={passwordToUse}";

        var query = $"SELECT column_name, data_type FROM information_schema.columns WHERE table_name = @tableName;";
        await using IPostgresResource resource = await _dbProvider.GetPostgresResource(connectionString, authType, cancellationToken);
        await using NpgsqlCommand command = _dbProvider.GetCommand(query, resource);
        command.Parameters.AddWithValue("tableName", table);
        await using DbDataReader reader = await _dbProvider.ExecuteReaderAsync(command, cancellationToken);
        var schema = new List<string>();
        while (await reader.ReadAsync(cancellationToken))
        {
            schema.Add($"{reader.GetString(0)}: {reader.GetString(1)}");
        }
        return schema;
    }

    public async Task<List<string>> ListServersAsync(
        string subscriptionId,
        string resourceGroup,
        string user,
        CancellationToken cancellationToken)
    {
        var rg = await _resourceGroupService.GetResourceGroupResource(subscriptionId, resourceGroup, cancellationToken: cancellationToken);
        if (rg == null)
        {
            throw new Exception($"Resource group '{resourceGroup}' not found.");
        }
        var serverList = new List<string>();
        await foreach (PostgreSqlFlexibleServerResource server in rg.GetPostgreSqlFlexibleServers().GetAllAsync(cancellationToken))
        {
            serverList.Add(server.Data.Name);
        }
        return serverList;
    }

    public async Task<string> GetServerConfigAsync(
        string subscriptionId,
        string resourceGroup,
        string user,
        string server,
        CancellationToken cancellationToken)
    {
        var rg = await _resourceGroupService.GetResourceGroupResource(subscriptionId, resourceGroup, cancellationToken: cancellationToken);
        if (rg == null)
        {
            throw new Exception($"Resource group '{resourceGroup}' not found.");
        }
        var pgServer = await rg.GetPostgreSqlFlexibleServerAsync(server, cancellationToken);
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

    public async Task<string> GetServerParameterAsync(
        string subscriptionId,
        string resourceGroup,
        string user,
        string server,
        string param,
        CancellationToken cancellationToken)
    {
        var rg = await _resourceGroupService.GetResourceGroupResource(subscriptionId, resourceGroup, cancellationToken: cancellationToken);
        if (rg == null)
        {
            throw new Exception($"Resource group '{resourceGroup}' not found.");
        }
        var pgServer = await rg.GetPostgreSqlFlexibleServerAsync(server, cancellationToken);

        var configResponse = await pgServer.Value.GetPostgreSqlFlexibleServerConfigurationAsync(param, cancellationToken);
        if (configResponse?.Value?.Data == null)
        {
            throw new Exception($"Parameter '{param}' not found.");
        }
        return configResponse.Value.Data.Value;
    }

    public async Task<string> SetServerParameterAsync(
        string subscriptionId,
        string resourceGroup,
        string user,
        string server,
        string param,
        string value,
        CancellationToken cancellationToken)
    {
        var rg = await _resourceGroupService.GetResourceGroupResource(subscriptionId, resourceGroup, cancellationToken: cancellationToken);
        if (rg == null)
        {
            throw new Exception($"Resource group '{resourceGroup}' not found.");
        }
        var pgServer = await rg.GetPostgreSqlFlexibleServerAsync(server, cancellationToken);

        var configResponse = await pgServer.Value.GetPostgreSqlFlexibleServerConfigurationAsync(param, cancellationToken);
        if (configResponse?.Value?.Data == null)
        {
            throw new Exception($"Parameter '{param}' not found.");
        }

        var configData = new PostgreSqlFlexibleServerConfigurationData
        {
            Value = value,
            Source = "user-override"
        };

        var updateOperation = await configResponse.Value.UpdateAsync(WaitUntil.Completed, configData, cancellationToken);
        if (updateOperation.HasCompleted && updateOperation.HasValue)
        {
            return $"Parameter '{param}' updated successfully to '{value}'.";
        }
        else
        {
            throw new Exception($"Failed to update parameter '{param}' to value '{value}'.");
        }
    }

    private async Task<string> GetPassword(string authType, string? password, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(authType) || AuthTypes.MicrosoftEntra.Equals(authType, StringComparison.InvariantCultureIgnoreCase))
        {
            return await GetEntraIdAccessTokenAsync(cancellationToken);
        }

        if (AuthTypes.PostgreSQL.Equals(authType, StringComparison.InvariantCultureIgnoreCase))
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new CommandValidationException($"Password must be provided for '{AuthTypes.PostgreSQL}' authentication.", HttpStatusCode.BadRequest);
            }
            return password;
        }

        throw new CommandValidationException($"Unsupported authentication type. Please use '{AuthTypes.MicrosoftEntra}' or '{AuthTypes.PostgreSQL}'", HttpStatusCode.BadRequest);
    }
}
