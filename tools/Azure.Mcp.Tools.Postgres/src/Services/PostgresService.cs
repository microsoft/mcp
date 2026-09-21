// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Data;
using System.Data.Common;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Postgres.Options;
using Azure.Mcp.Tools.Postgres.Providers;
using Azure.ResourceManager;
using Azure.ResourceManager.PostgreSql.FlexibleServers;
using Azure.ResourceManager.Resources;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Helpers;
using Npgsql;

namespace Azure.Mcp.Tools.Postgres.Services;

public class PostgresService(IAzureService azureService, IEntraTokenProvider entraTokenAuth, IDbProvider dbProvider)
    : BaseAzureService(azureService), IPostgresService
{
    private readonly IEntraTokenProvider _entraTokenAuth = entraTokenAuth;
    private readonly IDbProvider _dbProvider = dbProvider;

    internal const int MaxRowCount = 10_000;

    private async Task<string> GetEntraIdAccessTokenAsync(CancellationToken cancellationToken)
    {
        var tokenCredential = await GetCredential(null, cancellationToken);
        var accessToken = await _entraTokenAuth.GetEntraToken(tokenCredential, cancellationToken);

        return accessToken.Token;
    }

    /// <summary>
    /// Returns the full hostname to a Postgres server to be used for
    /// connection string construction.
    /// </summary>
    /// <param name="serverNameOrFullHostname">
    /// A <see cref="string"/> that is either (1) a server name, e.g., mydb or (2) a full DNS
    /// hostname to a server, e.g., mydb.postgres.database.azure.com in the public cloud.
    /// </param>
    /// <returns>
    /// A server hostname to be used for connection string construction based on the current
    /// cloud configuration of the application's runtime.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="serverNameOrFullHostname"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="serverNameOrFullHostname"/> is empty or whitespace; is not a bare
    /// DNS server name or hostname; identifies only the PostgreSQL domain suffix without a server
    /// label; the configured Azure cloud is not supported for PostgreSQL connections; or the
    /// PostgreSQL endpoint allow-list is not configured.
    /// </exception>
    /// <exception cref="SecurityException">
    /// Thrown when SSRF protections are enabled and the completed endpoint is not a valid absolute
    /// HTTPS URI or its host is not an allowed PostgreSQL domain for the configured Azure cloud.
    /// </exception>
    private string CreateAndValidateServerHostname(string serverNameOrFullHostname)
    {
        // GENERAL REMARKS:
        // This method is building the hostname used in a connection string, NOT an HTTPS URI
        // to be used by an HttpClient or the like. As such, there are different tests within
        // this method that EndpointValidator.ValidateAzureServiceEndpoint isn't presently
        // prepared to handle as it's focused on HTTPS URI validation. To authors: you must
        // document each step that is unique in this method for knowledge sharing, historical
        // archiving, and evaluation of correctness.
        ArgumentException.ThrowIfNullOrWhiteSpace(serverNameOrFullHostname);

        ArmEnvironment armEnvironment = AzureService.CloudConfiguration.ArmEnvironment;
        string postgresDnsSuffix = GetPostgresDnsSuffix(armEnvironment);

        // Short Azure PostgreSQL server names have no domain component, so a dot signals that the
        // caller supplied a URI-ready, fully qualified host candidate. Preserve that candidate so
        // validation evaluates the supplied authority; complete only short names with the cloud suffix.
        string host = serverNameOrFullHostname.Contains('.')
            ? serverNameOrFullHostname
            : serverNameOrFullHostname + postgresDnsSuffix;

        // EndpointValidator authorizes the parsed URI host, while Npgsql receives this raw host string.
        // Require DNS-only syntax so user-info, ports, paths, or multi-host values cannot make those differ.
        if (Uri.CheckHostName(host) != UriHostNameType.Dns)
        {
            throw new ArgumentException(
                "The server value must be either a short Azure Database for PostgreSQL server name or a fully " +
                    "qualified Azure Database for PostgreSQL hostname. Do not include a URL scheme, port, path, " +
                    "query, fragment, user information, or multiple hosts.",
                nameof(serverNameOrFullHostname));
        }

        // We prefix with `https://` even though it's not used for a connection string.
        // The method has some helpful error messages and other logic that we won't re-implement here.
        EndpointValidator.ValidateAzureServiceEndpoint(
            endpoint: $"https://{host}",
            serviceType: "postgres",
            armEnvironment: armEnvironment,
            executingToolNamespaceName: "postgres");

        // EndpointValidator permits the allow-listed suffix root, but PostgreSQL connection endpoints
        // require a resource-specific serverNameOrFullHostname label before that suffix.
        if (host.Equals(postgresDnsSuffix.TrimStart('.'), StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "The server name is not a valid Azure Database for PostgreSQL hostname because it does not include a server label.",
                nameof(serverNameOrFullHostname));
        }

        // We do NOT return a value with `https://` prefixed because that's not valid for
        // a connection string.
        return host;
    }

    /// <summary>
    /// Gets the appropriate DNS suffix for a Postgres endpoint in the given Azure cloud.
    /// </summary>
    /// <param name="armEnvironment">The Azure cloud of interest.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">
    /// Given cloud is not valid or supported.
    /// </exception>
    private static string GetPostgresDnsSuffix(ArmEnvironment armEnvironment) =>
        //EndpointValidator.AllowLists.cs also has a copy of this list. Keep them in sync.
        ArmEnvironment.AzurePublicCloud.Equals(armEnvironment) ? ".postgres.database.azure.com" :
        ArmEnvironment.AzureChina.Equals(armEnvironment) ? ".postgres.database.chinacloudapi.cn" :
        ArmEnvironment.AzureGovernment.Equals(armEnvironment) ? ".postgres.database.usgovcloudapi.net" :
        throw new ArgumentException(
            $"The configured Azure cloud is not supported for PostgreSQL connections. Value given: '{armEnvironment}'.",
            nameof(armEnvironment));

    public async Task<DatabaseListResult> ListDatabasesAsync(
        string authType,
        string user,
        string? password,
        string server,
        CancellationToken cancellationToken)
    {
        string host = CreateAndValidateServerHostname(server);
        string? passwordToUse = await GetPassword(authType, password, cancellationToken);
        var connectionString = BuildConnectionString(host, "postgres", user, passwordToUse);

        var query = "SELECT datname FROM pg_database WHERE datistemplate = false ORDER BY datname LIMIT @maxResults;";
        await using IPostgresResource resource = await _dbProvider.GetPostgresResource(connectionString, authType, cancellationToken);
        await using NpgsqlCommand command = _dbProvider.GetCommand(query, resource);
        // Fetch cap+1 rows so we can detect truncation by observing whether an extra row exists, then trim it.
        command.Parameters.AddWithValue("maxResults", MaxRowCount + 1);
        await using DbDataReader reader = await _dbProvider.ExecuteReaderAsync(command, cancellationToken);
        var dbs = new List<string>();
        while (await reader.ReadAsync(cancellationToken))
        {
            dbs.Add(reader.GetString(0));
        }

        var isTruncated = dbs.Count > MaxRowCount;
        if (isTruncated)
        {
            dbs.RemoveRange(MaxRowCount, dbs.Count - MaxRowCount);
        }

        return new DatabaseListResult(dbs, isTruncated);
    }

    public async Task<List<string>> ExecuteQueryAsync(
        string authType,
        string user,
        string? password,
        string server,
        string database,
        string query,
        CancellationToken cancellationToken)
    {
        string host = CreateAndValidateServerHostname(server);
        string? passwordToUse = await GetPassword(authType, password, cancellationToken);
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

    public async Task<TableListResult> ListTablesAsync(
        string authType,
        string user,
        string? password,
        string server,
        string database,
        string schema,
        CancellationToken cancellationToken)
    {
        string host = CreateAndValidateServerHostname(server);
        string? passwordToUse = await GetPassword(authType, password, cancellationToken);
        var connectionString = BuildConnectionString(host, database, user, passwordToUse);

        var query = "SELECT table_name FROM information_schema.tables WHERE table_schema = @schema ORDER BY table_name LIMIT @maxResults;";
        await using IPostgresResource resource = await _dbProvider.GetPostgresResource(connectionString, authType, cancellationToken);
        await using NpgsqlCommand command = _dbProvider.GetCommand(query, resource);
        command.Parameters.AddWithValue("schema", schema);
        // Fetch cap+1 rows so we can detect truncation by observing whether an extra row exists, then trim it.
        command.Parameters.AddWithValue("maxResults", MaxRowCount + 1);
        await using DbDataReader reader = await _dbProvider.ExecuteReaderAsync(command, cancellationToken);
        var tables = new List<string>();
        while (await reader.ReadAsync(cancellationToken))
        {
            tables.Add(reader.GetString(0));
        }

        var isTruncated = tables.Count > MaxRowCount;
        if (isTruncated)
        {
            tables.RemoveRange(MaxRowCount, tables.Count - MaxRowCount);
        }

        return new TableListResult(tables, isTruncated);
    }

    public async Task<List<string>> GetTableSchemaAsync(
        string authType,
        string user,
        string? password,
        string server,
        string database,
        string table,
        CancellationToken cancellationToken)
    {
        string host = CreateAndValidateServerHostname(server);
        string? passwordToUse = await GetPassword(authType, password, cancellationToken);
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
            var subscription = await AzureService.GetSubscription(subscriptionId, cancellationToken: cancellationToken);
            await foreach (var name in ListSubscriptionServerNamesAsync(subscription, cancellationToken))
                serverList.Add(name);
        }
        else
        {
            // List Flexible Servers scoped to the given resource group
            var rg = await AzureService.GetResourceGroupResource(subscriptionId, resourceGroup, cancellationToken: cancellationToken);
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
        string server,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        var rg = await AzureService.GetResourceGroupResource(subscriptionId, resourceGroup, tenant, cancellationToken: cancellationToken)
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
        string server,
        string param,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        var rg = await AzureService.GetResourceGroupResource(subscriptionId, resourceGroup, tenant, cancellationToken: cancellationToken)
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
        string server,
        string param,
        string value,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        var rg = await AzureService.GetResourceGroupResource(subscriptionId, resourceGroup, tenant, cancellationToken: cancellationToken)
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
            Password = password,
            SslMode = SslMode.Require
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
                throw new CommandValidationException($"Password must be provided for '{AuthTypes.PostgreSQL}' authentication.");
            }
            return password;
        }

        throw new CommandValidationException($"Unsupported authentication type. Please use '{AuthTypes.MicrosoftEntra}' or '{AuthTypes.PostgreSQL}'");
    }
}
