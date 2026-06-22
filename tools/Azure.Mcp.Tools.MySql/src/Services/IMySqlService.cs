// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.MySql.Services;

public interface IMySqlService
{
    Task<List<string>> ListDatabasesAsync(string subscriptionId, string resourceGroup, string user, string server, CancellationToken cancellationToken);
    Task<List<string>> ExecuteQueryAsync(string subscriptionId, string resourceGroup, string user, string server, string database, string query, CancellationToken cancellationToken);

    Task<TableListResult> GetTablesAsync(string subscriptionId, string resourceGroup, string user, string server, string database, CancellationToken cancellationToken);
    Task<List<string>> GetTableSchemaAsync(string subscriptionId, string resourceGroup, string user, string server, string database, string table, CancellationToken cancellationToken);

    Task<List<string>> ListServersAsync(string subscriptionId, string resourceGroup, CancellationToken cancellationToken);
    Task<List<string>> ListServersInSubscriptionAsync(string subscriptionId, CancellationToken cancellationToken);
    Task<string> GetServerConfigAsync(string subscriptionId, string resourceGroup, string server, CancellationToken cancellationToken);
    Task<string> GetServerParameterAsync(string subscriptionId, string resourceGroup, string server, string param, CancellationToken cancellationToken);
    Task<string> SetServerParameterAsync(string subscription, string resourceGroup, string server, string param, string value, CancellationToken cancellationToken);
}
