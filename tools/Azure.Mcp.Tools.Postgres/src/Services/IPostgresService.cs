// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Postgres.Services;

public interface IPostgresService
{
    Task<DatabaseListResult> ListDatabasesAsync(
        string authType,
        string user,
        string? password,
        string server,
        string? tenant,
        CancellationToken cancellationToken);

    Task<List<string>> ExecuteQueryAsync(
        string authType,
        string user,
        string? password,
        string server,
        string database,
        string query,
        string? tenant,
        CancellationToken cancellationToken);

    Task<TableListResult> ListTablesAsync(
        string authType,
        string user,
        string? password,
        string server,
        string database,
        string schema,
        string? tenant,
        CancellationToken cancellationToken);

    Task<List<string>> GetTableSchemaAsync(
        string authType,
        string user,
        string? password,
        string server,
        string database,
        string table,
        string? tenant,
        CancellationToken cancellationToken);

    Task<List<string>> ListServersAsync(
        string subscriptionId,
        string? resourceGroup,
        string? tenant,
        CancellationToken cancellationToken);

    Task<string> GetServerConfigAsync(
        string subscriptionId,
        string resourceGroup,
        string user,
        string server,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<string> GetServerParameterAsync(
        string subscriptionId,
        string resourceGroup,
        string user,
        string server,
        string param,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<string> SetServerParameterAsync(
        string subscription,
        string resourceGroup,
        string user,
        string server,
        string param,
        string value,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);
}
