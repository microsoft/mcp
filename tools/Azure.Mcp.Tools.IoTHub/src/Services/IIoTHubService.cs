// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.IoTHub.Models;
using Azure.Mcp.Core.Services.Azure;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.IoTHub.Services;

public interface IIoTHubService
{
    Task<ResourceQueryResults<IoTHubDescription>> GetIoTHub(
        string? name,
        string? resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);
}
