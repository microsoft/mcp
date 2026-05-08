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

    #region Agents + Skills (sub-agent A)

    Task<SreSubAgent> GetSubAgentAsync(string endpoint, string name, string? tenant = null, CancellationToken cancellationToken = default);

    Task<SreSubAgent> CreateSubAgentAsync(string endpoint, SreSubAgentCreateRequest request, string? tenant = null, CancellationToken cancellationToken = default);

    Task<SreAgentDeleteResult> DeleteSubAgentAsync(string endpoint, string name, string? tenant = null, CancellationToken cancellationToken = default);

    Task<SreAgentTool> GetAgentToolAsync(string endpoint, string name, string? tenant = null, CancellationToken cancellationToken = default);

    Task<SreAgentTool> CreateAgentToolAsync(string endpoint, SreAgentToolCreateRequest request, string? tenant = null, CancellationToken cancellationToken = default);

    Task<List<SreAgentTool>> ListAgentToolsAsync(string endpoint, string? tenant = null, CancellationToken cancellationToken = default);

    Task<SreAgentDeleteResult> DeleteAgentToolAsync(string endpoint, string name, string? tenant = null, CancellationToken cancellationToken = default);

    Task<List<SreSkill>> ListSkillsAsync(string endpoint, string? tenant = null, CancellationToken cancellationToken = default);

    Task<SreSkill> CreateSkillAsync(string endpoint, SreSkillCreateRequest request, string? tenant = null, CancellationToken cancellationToken = default);

    #endregion
}
