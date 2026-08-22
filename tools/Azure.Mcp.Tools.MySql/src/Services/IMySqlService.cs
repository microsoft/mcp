// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.MySql.Services;

public interface IMySqlService
{
    Task<List<string>> ListDatabasesAsync(string subscriptionId, string resourceGroup, string user, string server, string? tenant, CancellationToken cancellationToken);
    Task<List<string>> ExecuteQueryAsync(string subscriptionId, string resourceGroup, string user, string server, string database, string query, string? tenant, CancellationToken cancellationToken);

    Task<TableListResult> GetTablesAsync(string subscriptionId, string resourceGroup, string user, string server, string database, string? tenant, CancellationToken cancellationToken);
    Task<List<string>> GetTableSchemaAsync(string subscriptionId, string resourceGroup, string user, string server, string database, string table, string? tenant, CancellationToken cancellationToken);

    Task<List<string>> ListServersAsync(string subscriptionId, string resourceGroup, string? tenant, CancellationToken cancellationToken);
    Task<List<string>> ListServersInSubscriptionAsync(string subscriptionId, string? tenant, CancellationToken cancellationToken);
    Task<string> GetServerConfigAsync(string subscriptionId, string resourceGroup, string server, string? tenant, CancellationToken cancellationToken);
    Task<string> GetServerParameterAsync(string subscriptionId, string resourceGroup, string server, string param, string? tenant, CancellationToken cancellationToken);
    Task<string> SetServerParameterAsync(string subscription, string resourceGroup, string server, string param, string value, string? tenant, CancellationToken cancellationToken);
}
