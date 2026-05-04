// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.ContainerApps.Models;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ContainerApps.Services;

public interface IContainerAppsService
{
    Task<ResourceQueryResults<ContainerAppInfo>> ListContainerApps(
        string subscription,
        string? resourceGroup = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);
}
