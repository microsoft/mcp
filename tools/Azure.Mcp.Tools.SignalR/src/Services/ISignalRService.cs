// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.SignalR.Models;

namespace Azure.Mcp.Tools.SignalR.Services;

/// <summary>
/// Service interface for Azure SignalR operations.
/// </summary>
public interface ISignalRService
{
    Task<IEnumerable<Runtime>> ListRuntimesAsync(
        string subscription,
        string? tenant = null,
        AuthMethod? authMethod = null,
        RetryPolicyOptions? retryPolicy = null);

    Task<Runtime?> GetRuntimeAsync(
        string subscription,
        string resourceGroup,
        string signalRName,
        string? tenant = null,
        AuthMethod? authMethod = null,
        RetryPolicyOptions? retryPolicy = null);

    /// <summary>
    /// Lists keys for a SignalR service.
    /// </summary>
    Task<Key?> ListKeysAsync(
        string subscription,
        string resourceGroup,
        string signalRName,
        string? tenant = null,
        AuthMethod? authMethod = null,
        RetryPolicyOptions? retryPolicy = null);

    /// <summary>
    /// Gets identity configuration for a SignalR service.
    /// </summary>
    Task<SignalR.Models.Identity?> GetSignalRIdentityAsync(
        string subscription,
        string resourceGroup,
        string signalRName,
        string? tenant = null,
        AuthMethod? authMethod = null,
        RetryPolicyOptions? retryPolicy = null);

    /// <summary>
    /// Lists network ACL rules for a SignalR service.
    /// </summary>
    Task<NetworkRule?> GetNetworkRulesAsync(
        string subscription,
        string resourceGroup,
        string signalRName,
        string? tenant = null,
        AuthMethod? authMethod = null,
        RetryPolicyOptions? retryPolicy = null);
}
