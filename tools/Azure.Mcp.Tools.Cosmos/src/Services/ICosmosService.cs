// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Cosmos.Services;

public interface ICosmosService : IDisposable
{
    Task<List<string>> GetCosmosAccounts(
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);

    Task<List<string>> ListDatabases(
        string accountName,
        string subscription,
        AuthMethod authMethod = AuthMethod.Credential,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);

    Task<List<string>> ListContainers(
        string accountName,
        string databaseName,
        string subscription,
        AuthMethod authMethod = AuthMethod.Credential,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);

    Task<List<JsonElement>> QueryItems(
        string accountName,
        string databaseName,
        string containerName,
        string? query,
        string subscription,
        AuthMethod authMethod = AuthMethod.Credential,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);
}
