// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Data;
using System.Data.Common;
using System.Net;
using System.Runtime.CompilerServices;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.ResourceGroup;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.Postgres.Auth;
using Azure.Mcp.Tools.Postgres.Options;
using Azure.Mcp.Tools.Postgres.Providers;
using Azure.ResourceManager.PostgreSql.FlexibleServers;
using Azure.ResourceManager.Resources;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Helpers;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using Npgsql;


namespace Azure.Mcp.Tools.Postgres.Services;

public class PostgresService(
    IResourceGroupService resourceGroupService,
    ISubscriptionService subscriptionService,
    ITenantService tenantService,
    IEntraTokenProvider entraTokenAuth,
    IDbProvider dbProvider) : BaseAzureService(tenantService), IPostgresService
{
    private readonly IResourceGroupService _resourceGroupService = resourceGroupService ?? throw new ArgumentNullException(nameof(resourceGroupService));
    private readonly ISubscriptionService _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
    private readonly ITenantService _tenantService = tenantService ?? throw new ArgumentNullException(nameof(tenantService));
    private readonly IEntraTokenProvider _entraTokenAuth = entraTokenAuth;
    private readonly IDbProvider _dbProvider = dbProvider;

    private async Task<string> GetEntraIdAccessTokenAsync(CancellationToken cancellationToken)
    {
        var tokenCredential = await GetCredential(cancellationToken);
        var accessToken = await _entraTokenAuth.GetEntraToken(tokenCredential, cancellationToken);

        return accessToken.Token;
    }

    private static readonly string[] AllowedPostgresSuffixes =
    [
        ".postgres.database.azure.com",
        ".postgres.database.usgovcloudapi.net",
        ".postgres.database.chinacloudapi.cn",
    ];

    private string NormalizeServerName(string server)
    {
        if (!server.Contains('.'))
        {
            return _tenantService.CloudConfiguration.CloudType switch
            {
                AzureCloudConfiguration.AzureCloud.AzurePublicCloud =>
                    server + ".postgres.database.azure.com",
                AzureCloudConfiguration.AzureCloud.AzureUSGovernmentCloud =>
                    server + ".postgres.database.usgovcloudapi.net",
                AzureCloudConfiguration.AzureCloud.AzureChinaCloud =>
                    server + ".postgres.database.chinacloudapi.cn",
                _ =>
                    server + ".postgres.database.azure.com"
            };
        }

        if (!Array.Exists(AllowedPostgresSuffixes, suffix => server.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)))
        {
            throw new ArgumentException(
                $"The server name '{server}' is not a valid Azure Database for PostgreSQL hostname. " +
                $"Fully qualified server names must end with one of: {string.Join(", ", AllowedPostgresSuffixes)}.");
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
        var connectionString = BuildConnectionString(host, "postgres", user, passwordToUse);

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
        var connectionString = BuildConnectionString(host, database, user, passwordToUse);

        var (parameterizedQuery, queryParameters) = ParameterizeStringLiterals(query);

        await using IPostgresResource resource = await _dbProvider.GetPostgresResource(connectionString, authType, cancellationToken);
        await using NpgsqlCommand command = _dbProvider.GetCommand(parameterizedQuery, resource);

        foreach (var (name, value) in queryParameters)
        {
            command.Parameters.AddWithValue(name, value);
        }

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
        var connectionString = BuildConnectionString(host, database, user, passwordToUse);

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
        var connectionString = BuildConnectionString(host, database, user, passwordToUse);

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
        string? resourceGroup,
        CancellationToken cancellationToken)
    {
        var serverList = new List<string>();

        if (string.IsNullOrEmpty(resourceGroup))
        {
            // List all Flexible Servers across the entire subscription
            var subscription = await _subscriptionService.GetSubscription(subscriptionId, cancellationToken: cancellationToken);
            await foreach (var name in ListSubscriptionServerNamesAsync(subscription, cancellationToken))
                serverList.Add(name);
        }
        else
        {
            // List Flexible Servers scoped to the given resource group
            var rg = await _resourceGroupService.GetResourceGroupResource(subscriptionId, resourceGroup, cancellationToken: cancellationToken);
            if (rg == null)
                throw new Exception($"Resource group '{resourceGroup}' not found.");
            await foreach (var name in ListResourceGroupServerNamesAsync(rg, cancellationToken))
                serverList.Add(name);
        }

        return serverList;
    }

    // Virtual so tests can override and avoid calling the un-mockable ARM SDK extension methods.
    protected virtual async IAsyncEnumerable<string> ListSubscriptionServerNamesAsync(
        SubscriptionResource subscription,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (PostgreSqlFlexibleServerResource server in subscription.GetPostgreSqlFlexibleServersAsync(cancellationToken))
            yield return server.Data.Name;
    }

    protected virtual async IAsyncEnumerable<string> ListResourceGroupServerNamesAsync(
        ResourceGroupResource resourceGroup,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (PostgreSqlFlexibleServerResource server in resourceGroup.GetPostgreSqlFlexibleServers().GetAllAsync(cancellationToken))
            yield return server.Data.Name;
    }

    public async Task<string> GetServerConfigAsync(
        string subscriptionId,
        string resourceGroup,
        string user,
        string server,
        CancellationToken cancellationToken)
    {
        var rg = await _resourceGroupService.GetResourceGroupResource(subscriptionId, resourceGroup, cancellationToken: cancellationToken)
            ?? throw new Exception($"Resource group '{resourceGroup}' not found.");

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
        var rg = await _resourceGroupService.GetResourceGroupResource(subscriptionId, resourceGroup, cancellationToken: cancellationToken)
            ?? throw new Exception($"Resource group '{resourceGroup}' not found.");

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
        var rg = await _resourceGroupService.GetResourceGroupResource(subscriptionId, resourceGroup, cancellationToken: cancellationToken)
            ?? throw new Exception($"Resource group '{resourceGroup}' not found.");

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

        var updateOperation = await configResponse.Value.UpdateAsync(WaitUntil.Started, configData, cancellationToken);
        await WaitForLroCompletionAsync(updateOperation, cancellationToken);
        if (updateOperation.HasCompleted && updateOperation.HasValue)
        {
            return $"Parameter '{param}' updated successfully to '{value}'.";
        }
        else
        {
            throw new Exception($"Failed to update parameter '{param}' to value '{value}'.");
        }
    }

    private static string BuildConnectionString(string host, string database, string user, string password)
    {
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = host,
            Database = database,
            Username = user,
            Password = password
        };
        return builder.ConnectionString;
    }

    internal static (string Query, List<(string Name, string Value)> Parameters) ParameterizeStringLiterals(string query) =>
        SqlQueryParameterizer.Parameterize(query, SqlQueryParameterizer.SqlDialect.Standard);

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
