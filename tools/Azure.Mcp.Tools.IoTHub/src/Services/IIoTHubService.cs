// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.IoTHub.Models;
using Azure.Mcp.Core.Services.Azure;

namespace Azure.Mcp.Tools.IoTHub.Services;

public interface IIoTHubService
{
    Task<List<IoTHubDescription>> GetIoTHub(
        string? name,
        string? resourceGroup,
        string subscription,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<List<IoTHubKey>> GetIoTHubKeys(
        string name,
        string resourceGroup,
        string subscription,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);
      }