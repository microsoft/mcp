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




    #region Threads + ScheduledTasks (sub-agent C)

    Task<List<SreAgentThread>> ListThreadsAsync(string endpoint, string? tenant = null, CancellationToken cancellationToken = default);

    Task<SreAgentThread?> GetThreadAsync(string endpoint, string threadId, string? tenant = null, CancellationToken cancellationToken = default);

    Task<List<SreAgentThreadMessage>> GetThreadMessagesAsync(string endpoint, string threadId, string? tenant = null, CancellationToken cancellationToken = default);

    Task<SreAgentThread?> CreateThreadAsync(string endpoint, SreAgentThreadCreateRequest request, string? tenant = null, CancellationToken cancellationToken = default);

    Task<SreAgentThreadMessage?> SendThreadMessageAsync(string endpoint, string threadId, SreAgentThreadMessageRequest request, string? tenant = null, CancellationToken cancellationToken = default);

    Task DeleteThreadAsync(string endpoint, string threadId, string? tenant = null, CancellationToken cancellationToken = default);

    Task ApproveApprovalAsync(string endpoint, string approvalId, SreAgentApprovalRequest request, string? tenant = null, CancellationToken cancellationToken = default);

    Task<List<SreAgentScheduledTask>> ListScheduledTasksAsync(string endpoint, string? tenant = null, CancellationToken cancellationToken = default);

    Task<SreAgentScheduledTask?> GetScheduledTaskAsync(string endpoint, string taskId, string? tenant = null, CancellationToken cancellationToken = default);

    Task<SreAgentScheduledTask?> CreateScheduledTaskAsync(string endpoint, SreAgentScheduledTaskCreateRequest request, string? tenant = null, CancellationToken cancellationToken = default);

    Task DeleteScheduledTaskAsync(string endpoint, string taskId, string? tenant = null, CancellationToken cancellationToken = default);

    Task PauseScheduledTaskAsync(string endpoint, string taskId, string? tenant = null, CancellationToken cancellationToken = default);

    Task ResumeScheduledTaskAsync(string endpoint, string taskId, string? tenant = null, CancellationToken cancellationToken = default);

    #endregion




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
