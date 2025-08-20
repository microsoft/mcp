// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Kusto.Commands;

namespace Azure.Mcp.Tools.Kusto.Services;

public interface IKustoService
{
    Task<List<string>> ListClusters(
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);

    Task<KustoClusterResourceProxy?> GetCluster(
        string subscription,
        string clusterName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);

    Task<List<string>> ListDatabases(
        string clusterUri,
        string? tenant = null,
        AuthMethod? authMethod = AuthMethod.Credential,
        RetryPolicyOptions? retryPolicy = null);

    Task<List<string>> ListDatabases(
        string subscription,
        string clusterName,
        string? tenant = null,
        AuthMethod? authMethod = AuthMethod.Credential,
        RetryPolicyOptions? retryPolicy = null);

    Task<List<JsonElement>> QueryItems(
        string clusterUri,
        string databaseName,
        string query,
        string? tenant = null,
        AuthMethod? authMethod = AuthMethod.Credential,
        RetryPolicyOptions? retryPolicy = null);

    Task<List<JsonElement>> QueryItems(
        string subscriptionId,
        string clusterName,
        string databaseName,
        string query,
        string? tenant = null,
        AuthMethod? authMethod = AuthMethod.Credential,
        RetryPolicyOptions? retryPolicy = null);

    Task<List<string>> ListTables(
        string clusterUri,
        string databaseName,
        string? tenant = null,
        AuthMethod? authMethod = AuthMethod.Credential,
        RetryPolicyOptions? retryPolicy = null);

    Task<List<string>> ListTables(
        string subscriptionId,
        string clusterName,
        string databaseName,
        string? tenant = null,
        AuthMethod? authMethod = AuthMethod.Credential,
        RetryPolicyOptions? retryPolicy = null);

    Task<string> GetTableSchema(
        string clusterUri,
        string databaseName,
        string tableName,
        string? tenant = null,
        AuthMethod? authMethod = AuthMethod.Credential,
        RetryPolicyOptions? retryPolicy = null);

    Task<string> GetTableSchema(
        string subscriptionId,
        string clusterName,
        string databaseName,
        string tableName,
        string? tenant = null,
        AuthMethod? authMethod = AuthMethod.Credential,
        RetryPolicyOptions? retryPolicy = null);
}
