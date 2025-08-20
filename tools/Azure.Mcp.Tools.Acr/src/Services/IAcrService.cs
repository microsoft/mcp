// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Acr.Services;

public interface IAcrService
{
    Task<List<Models.AcrRegistryInfo>> ListRegistries(
        string subscription,
        string? resourceGroup = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);
}
