// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.SreAgent.Models;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.SreAgent.Services;

public interface ISreAgentService
{
    /// <summary>
    /// Lists Azure SRE Agent resources in the subscription, optionally filtered by resource group.
    /// </summary>
    Task<List<SreAgentResource>> ListAgentsAsync(
        string subscription,
        string? resourceGroup = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);
    #region Connectors + Hooks (sub-agent B)

    Task<List<AgentConnector>> ListConnectorsAsync(
        string endpoint,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    Task<AgentConnector> GetConnectorAsync(
        string endpoint,
        string name,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    Task<AgentConnector> CreateOrUpdateConnectorAsync(
        string endpoint,
        string name,
        AgentConnectorEnvelope connector,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    Task DeleteConnectorAsync(
        string endpoint,
        string name,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    Task<ConnectorTestResult> TestConnectorAsync(
        string endpoint,
        string name,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    Task<List<HookEnvelope>> ListHooksAsync(
        string endpoint,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    Task<HookEnvelope> GetHookAsync(
        string endpoint,
        string name,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    Task DeleteHookAsync(
        string endpoint,
        string name,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    Task<ThreadHooksResponse> ListThreadHooksAsync(
        string endpoint,
        string threadId,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    Task ActivateThreadHookAsync(
        string endpoint,
        string threadId,
        string hookName,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    Task DeactivateThreadHookAsync(
        string endpoint,
        string threadId,
        string hookName,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    #endregion
}
