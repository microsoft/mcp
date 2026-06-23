// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Postgres.Services;

public interface IPostgresService
{
    Task<List<string>> ListDatabasesAsync(
        string authType,
        string user,
        string? password,
        string server,
        CancellationToken cancellationToken);

    Task<List<string>> ExecuteQueryAsync(
        string authType,
        string user,
        string? password,
        string server,
        string database,
        string query,
        CancellationToken cancellationToken);

    Task<List<string>> ListTablesAsync(
        string authType,
        string user,
        string? password,
        string server,
        string database,
        string schema,
        CancellationToken cancellationToken);

    Task<List<string>> GetTableSchemaAsync(
        string authType,
        string user,
        string? password,
        string server,
        string database,
        string table,
        CancellationToken cancellationToken);

    Task<List<string>> ListServersAsync(
        string subscriptionId,
        string? resourceGroup,
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
