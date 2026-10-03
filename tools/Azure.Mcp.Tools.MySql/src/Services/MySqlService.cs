// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Security;
using System.Text.RegularExpressions;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.MySql.Commands;
using Azure.ResourceManager;
using Azure.ResourceManager.MySql.FlexibleServers;
using Azure.ResourceManager.MySql.FlexibleServers.Models;
using Microsoft.Mcp.Core.Helpers;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using MySqlConnector;

namespace Azure.Mcp.Tools.MySql.Services;

public sealed class MySqlService(IAzureService azureService)
    : BaseAzureService(azureService), IMySqlService
{
    // Maximum number of rows to return to prevent DoS attacks and performance issues
    private const int MaxRowCount = 10_000;

    // Maximum allowed query length in characters to prevent oversized inputs
    private const int MaxQueryLengthChars = 10_000;

    // Pre-compiled regex used to detect multiple / stacked statements
    private static readonly Regex s_multipleStatementsPattern =
        RegexHelper.CreateRegex(@";\s*\w", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private async Task<string> GetEntraIdAccessTokenAsync(CancellationToken cancellationToken)
    {
        var tokenCredential = await GetCredential(null, cancellationToken);
        var accessToken = await tokenCredential.GetTokenAsync(new([GetOpenSourceRDBMSScope()]), cancellationToken);
        return accessToken.Token;
    }

    private string GetOpenSourceRDBMSScope()
    {
        return AzureService.CloudConfiguration.CloudType switch
        {
            AzureCloudConfiguration.AzureCloud.AzurePublicCloud =>
                "https://ossrdbms-aad.database.windows.net/.default",
            AzureCloudConfiguration.AzureCloud.AzureUSGovernmentCloud =>
                "https://ossrdbms-aad.database.usgovcloudapi.net/.default",
            AzureCloudConfiguration.AzureCloud.AzureChinaCloud =>
                "https://ossrdbms-aad.database.chinacloudapi.cn/.default",
            _ => throw new ArgumentException(
                $"The configured Azure cloud is not supported for MySql connections. Value given: '{AzureService.CloudConfiguration.CloudType}'.",
                nameof(AzureService.CloudConfiguration.CloudType))
        };
    }

    /// <summary>
    /// Gets the appropriate DNS suffix for a MySql endpoint in the given Azure cloud.
    /// </summary>
    /// <param name="armEnvironment">The Azure cloud of interest.</param>
    /// <returns>The DNS suffix for the MySql endpoint in the specified Azure cloud.</returns>
    /// <exception cref="ArgumentException">
    /// Given cloud is not valid or supported.
    /// </exception>
    private static string GetMySqlDnsSuffix(ArmEnvironment armEnvironment) =>
        //EndpointValidator.AllowLists.cs also has a copy of this list. Keep them in sync.
        ArmEnvironment.AzurePublicCloud.Equals(armEnvironment) ? ".mysql.database.azure.com" :
        ArmEnvironment.AzureChina.Equals(armEnvironment) ? ".mysql.database.chinacloudapi.cn" :
        ArmEnvironment.AzureGovernment.Equals(armEnvironment) ? ".mysql.database.usgovcloudapi.net" :
        throw new ArgumentException(
            $"The configured Azure cloud is not supported for MySql connections. Value given: '{armEnvironment}'.",
            nameof(armEnvironment));

    /// <summary>
    /// Returns the full hostname to a MySql server to be used for
    /// connection string construction.
    /// </summary>
    /// <param name="serverNameOrFullHostname">
    /// A <see cref="string"/> that is either (1) a server name, e.g., mydb or (2) a full DNS
    /// hostname to a server, e.g., mydb.mysql.database.azure.com in the public cloud.
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
    /// DNS server name or hostname; identifies only the MySql domain suffix without a server
    /// label; the configured Azure cloud is not supported for MySql connections; or the
    /// MySql endpoint allow-list is not configured or does not allow the hostname.
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
        string mysqlDnsSuffix = GetMySqlDnsSuffix(armEnvironment);

        // Short Azure MySql server names have no domain component, so a dot signals that the
        // caller supplied a URI-ready, fully qualified host candidate. Preserve that candidate so
        // validation evaluates the supplied authority; complete only short names with the cloud suffix.
        string host = serverNameOrFullHostname.Contains('.')
            ? serverNameOrFullHostname
            : serverNameOrFullHostname + mysqlDnsSuffix;

        return ValidateServerHostname(host, armEnvironment);
    }

    internal static string ValidateServerHostname(string hostname, ArmEnvironment armEnvironment)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(hostname);

        // EndpointValidator authorizes the parsed URI host, while MySql receives this raw host string.
        // Require DNS-only syntax so user-info, ports, paths, or multi-host values cannot make those differ.
        if (Uri.CheckHostName(hostname) != UriHostNameType.Dns)
        {
            throw CreateInvalidServerException(hostname);
        }

        var endpoint = new Uri($"https://{hostname}", UriKind.Absolute);
        try
        {
            EndpointValidator.ValidateAzureServiceEndpoint(
                endpoint: endpoint.AbsoluteUri,
                serviceType: "mysql",
                armEnvironment: armEnvironment,
                executingToolNamespaceName: "mysql");
        }
        catch (SecurityException ex)
        {
            throw CreateInvalidServerException(hostname, ex);
        }

        // EndpointValidator permits the allow-listed suffix root, but MySql connection endpoints
        // require a resource-specific server label before that suffix.
        var mysqlDnsSuffix = GetMySqlDnsSuffix(armEnvironment);
        if (endpoint.IdnHost.Equals(mysqlDnsSuffix.TrimStart('.'), StringComparison.OrdinalIgnoreCase))
        {
            throw CreateInvalidServerException(hostname);
        }

        return endpoint.IdnHost;
    }

    private static ArgumentException CreateInvalidServerException(string hostname, Exception? innerException = null) =>
        new(
            $"The server name '{hostname}' is not a valid Azure Database for MySQL hostname for the configured cloud.",
            nameof(hostname),
            innerException);

    private async Task<string> BuildConnectionStringAsync(string server, string user, string database, CancellationToken cancellationToken)
    {
        var host = CreateAndValidateServerHostname(server);
        var entraIdAccessToken = await GetEntraIdAccessTokenAsync(cancellationToken);
        return BuildConnectionString(host, database, user, entraIdAccessToken);
    }

    internal static string BuildConnectionString(string host, string database, string user, string password)
    {
        var builder = new MySqlConnectionStringBuilder
        {
            Server = host,
            Database = database,
            UserID = user,
            Password = password,
            SslMode = MySqlSslMode.Required
        };
        return builder.ConnectionString;
    }

    /// <summary>
    /// Performs lightweight structural validation of a query. This does not restrict which SQL verbs may be
    /// executed; the caller's database permissions are the authority on what is allowed. Validation is limited
    /// to rejecting empty or oversized input, SQL comments, and multiple / stacked statements.
    /// </summary>
    internal static void ValidateQuerySafety(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            throw new ArgumentException("Query cannot be null or empty.", nameof(query));
        }

        // Prevent DoS attacks by limiting query length
        if (query.Length > MaxQueryLengthChars)
        {
            throw new InvalidOperationException($"Query length exceeds the maximum allowed limit of {MaxQueryLengthChars:N0} characters to prevent potential DoS attacks.");
        }

        // Strip string literals before checking for comment markers to avoid
        // false positives (e.g., 'C#Developer' or 'foo--bar' are not comments).
        // The pattern handles both SQL-standard doubled quotes ('') and
        // MySQL's default backslash escaping (\') inside string literals.
        var queryWithoutStrings = Regex.Replace(query, "'([^'\\\\]|\\\\.|'')*'", "'str'", RegexOptions.None, RegexHelper.DefaultRegexTimeout);

        // Reject queries containing SQL comments to prevent bypass attacks
        // (e.g., MySQL version-specific comments /*!50000 ... */ that are executed as code)
        if (queryWithoutStrings.Contains("--", StringComparison.Ordinal) || queryWithoutStrings.Contains("/*", StringComparison.Ordinal) || queryWithoutStrings.Contains("#", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("SQL comments are not allowed for security reasons.");
        }

        // Normalize whitespace and trim for validation
        var cleanedQuery = Regex.Replace(query, @"\s+", " ", RegexOptions.Multiline).Trim();

        // Ensure the cleaned query is not empty
        if (string.IsNullOrWhiteSpace(cleanedQuery))
        {
            throw new ArgumentException("Query cannot be empty after removing comments and whitespace.", nameof(query));
        }

        if (s_multipleStatementsPattern.IsMatch(cleanedQuery))
        {
            throw new InvalidOperationException("Multiple SQL statements are not allowed. Use only a single statement.");
        }
    }

    internal static (string Query, List<(string Name, string Value)> Parameters) ParameterizeStringLiterals(string query) =>
        SqlQueryParameterizer.Parameterize(query, SqlQueryParameterizer.SqlDialect.MySql);

    public async Task<List<string>> ListDatabasesAsync(string subscriptionId, string resourceGroup, string user, string server, CancellationToken cancellationToken)
    {
        var connectionString = await BuildConnectionStringAsync(server, user, "mysql", cancellationToken);

        await using var resource = await MySqlResource.CreateAsync(connectionString, cancellationToken);
        var query = "SHOW DATABASES;";
        await using var command = new MySqlCommand(query, resource.Connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var dbs = new List<string>();
        var dbCount = 0;
        while (await reader.ReadAsync(cancellationToken) && dbCount < MaxRowCount)
        {
            var dbName = reader.GetString(0);
            // Filter out system databases
            if (dbName != "information_schema" && dbName != "mysql" && dbName != "performance_schema" && dbName != "sys")
            {
                dbs.Add(dbName);
                dbCount++;
            }
        }

        if (dbCount >= MaxRowCount)
        {
            dbs.Add($"... (output limited to {MaxRowCount:N0} databases for security and performance reasons)");
        }

        return dbs;
    }

    public async Task<List<string>> ExecuteQueryAsync(string user, string server, string database, string query, CancellationToken cancellationToken)
    {
        ValidateQuerySafety(query);

        var (parameterizedQuery, queryParameters) = ParameterizeStringLiterals(query);

        var connectionString = await BuildConnectionStringAsync(server, user, database, cancellationToken);

        await using var resource = await MySqlResource.CreateAsync(connectionString, cancellationToken);
        await using var command = new MySqlCommand(parameterizedQuery, resource.Connection);

        foreach (var (name, value) in queryParameters)
        {
            command.Parameters.AddWithValue(name, value);
        }

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var rows = new List<string>();

        var columnNames = Enumerable.Range(0, reader.FieldCount)
                                .Select(reader.GetName)
                                .ToArray();
        rows.Add(string.Join(", ", columnNames));

        var rowCount = 0;

        while (await reader.ReadAsync(cancellationToken) && rowCount < MaxRowCount)
        {
            var row = new List<string>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                row.Add(reader[i]?.ToString() ?? "NULL");
            }
            rows.Add(string.Join(", ", row));
            rowCount++;
        }

        if (rowCount >= MaxRowCount)
        {
            rows.Add($"... (output limited to {MaxRowCount:N0} rows for security and performance reasons)");
        }

        return rows;
    }

    public async Task<List<string>> GetTableSchemaAsync(string user, string server, string database, string table, CancellationToken cancellationToken)
    {
        var connectionString = await BuildConnectionStringAsync(server, user, database, cancellationToken);

        await using var resource = await MySqlResource.CreateAsync(connectionString, cancellationToken);
        var query = "SELECT column_name, data_type FROM information_schema.columns WHERE table_schema = DATABASE() AND table_name = @table;";
        await using var command = new MySqlCommand(query, resource.Connection);
        command.Parameters.AddWithValue("@table", table);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var schema = new List<string>();
        while (await reader.ReadAsync(cancellationToken))
        {
            schema.Add($"{reader.GetString(0)}: {reader.GetString(1)}");
        }
        return schema;
    }

    public async Task<List<string>> ListServersAsync(string subscriptionId, string resourceGroup, CancellationToken cancellationToken)
    {
        var rg = await AzureService.GetResourceGroupResource(subscriptionId, resourceGroup, null, cancellationToken)
            ?? throw new KeyNotFoundException($"Resource group '{resourceGroup}' not found.");

        var serverList = new List<string>();
        await foreach (MySqlFlexibleServerResource server in rg.GetMySqlFlexibleServers().GetAllAsync(cancellationToken: cancellationToken))
        {
            serverList.Add(server.Data.Name);
        }
        return serverList;
    }

    public async Task<List<string>> ListServersInSubscriptionAsync(string subscriptionId, CancellationToken cancellationToken)
    {
        var subscriptionResource = await AzureService.GetSubscription(subscriptionId, cancellationToken: cancellationToken);
        var serverList = new List<string>();
        await foreach (MySqlFlexibleServerResource server in subscriptionResource.GetMySqlFlexibleServersAsync(cancellationToken: cancellationToken))
        {
            serverList.Add(server.Data.Name);
        }
        return serverList;
    }

    public async Task<TableListResult> GetTablesAsync(string subscriptionId, string resourceGroup, string user, string server, string database, CancellationToken cancellationToken)
    {
        var connectionString = await BuildConnectionStringAsync(server, user, database, cancellationToken);

        await using var resource = await MySqlResource.CreateAsync(connectionString, cancellationToken);
        var query = "SELECT table_name FROM information_schema.tables WHERE table_schema = DATABASE();";
        await using var command = new MySqlCommand(query, resource.Connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var tables = new List<string>();
        var tableCount = 0;
        while (await reader.ReadAsync(cancellationToken) && tableCount < MaxRowCount)
        {
            tables.Add(reader.GetString(0));
            tableCount++;
        }

        var isTruncated = tableCount >= MaxRowCount && await reader.ReadAsync(cancellationToken);

        return new TableListResult(tables, isTruncated);
    }

    public async Task<string> GetServerConfigAsync(string subscriptionId, string resourceGroup, string server, CancellationToken cancellationToken)
    {
        var rg = await AzureService.GetResourceGroupResource(subscriptionId, resourceGroup, null, cancellationToken)
            ?? throw new KeyNotFoundException($"Resource group '{resourceGroup}' not found.");

        var mysqlServer = await rg.GetMySqlFlexibleServerAsync(server, cancellationToken);
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

    public async Task<string> GetServerParameterAsync(string subscriptionId, string resourceGroup, string server, string param, CancellationToken cancellationToken)
    {
        var rg = await AzureService.GetResourceGroupResource(subscriptionId, resourceGroup, null, cancellationToken)
            ?? throw new KeyNotFoundException($"Resource group '{resourceGroup}' not found.");

        var mysqlServer = await rg.GetMySqlFlexibleServerAsync(server, cancellationToken);

        var configResponse = await mysqlServer.Value.GetMySqlFlexibleServerConfigurationAsync(param, cancellationToken);
        if (configResponse?.Value?.Data == null)
        {
            throw new KeyNotFoundException($"Parameter '{param}' not found on server '{server}'.");
        }
        return configResponse.Value.Data.Value;
    }

    public async Task<string> SetServerParameterAsync(string subscriptionId, string resourceGroup, string server, string param, string value, CancellationToken cancellationToken)
    {
        var rg = await AzureService.GetResourceGroupResource(subscriptionId, resourceGroup, null, cancellationToken)
            ?? throw new KeyNotFoundException($"Resource group '{resourceGroup}' not found.");

        var mysqlServer = await rg.GetMySqlFlexibleServerAsync(server, cancellationToken);

        var configuration = await mysqlServer.Value.GetMySqlFlexibleServerConfigurationAsync(param, cancellationToken);
        if (configuration?.Value?.Data == null)
        {
            throw new KeyNotFoundException($"Parameter '{param}' not found on server '{server}'.");
        }

        var configData = configuration.Value.Data;
        configData.Value = value;
        configData.Source = MySqlFlexibleServerConfigurationSource.UserOverride;

        var updateOperation = await mysqlServer.Value.GetMySqlFlexibleServerConfigurations().CreateOrUpdateAsync(WaitUntil.Started, param, configData, cancellationToken);
        await WaitForLroCompletionAsync(updateOperation, cancellationToken);
        return updateOperation.Value.Data.Value;
    }

    private sealed class MySqlResource : IAsyncDisposable
    {
        public MySqlConnection Connection { get; }

        public static async Task<MySqlResource> CreateAsync(string connectionString, CancellationToken cancellationToken)
        {
            var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);
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
