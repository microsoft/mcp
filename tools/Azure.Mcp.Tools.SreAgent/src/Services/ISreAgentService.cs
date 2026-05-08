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

    #region Incidents + Workflows + Docs + Architecture (sub-agent D)

    Task<string> CallAgentDataPlaneAsync(
        string subscription,
        string agent,
        string? resourceGroup,
        string path,
        HttpMethod method,
        string? jsonBody = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<string> UploadMemoryAsync(
        string subscription,
        string agent,
        string? resourceGroup,
        string fileName,
        string content,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    #endregion
}
