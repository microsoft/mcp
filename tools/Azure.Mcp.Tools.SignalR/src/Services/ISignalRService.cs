// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.SignalR.Models;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.SignalR.Services;

/// <summary>
/// Service interface for Azure SignalR operations.
/// </summary>
public interface ISignalRService
{
    Task<IEnumerable<Runtime>> GetRuntimeAsync(
        string subscription,
        string? resourceGroup,
        string? signalRName,
        string? tenant = null,
        AuthMethod? authMethod = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);
}
