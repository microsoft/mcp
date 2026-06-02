// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.SreAgent.Models;
using Azure.Mcp.Tools.SreAgent.Options.Threads;
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

    /// <summary>
    /// Gets a single Azure SRE Agent resource by name using a targeted Resource Graph query.
    /// </summary>
    Task<SreAgentResource?> GetAgentAsync(
        string subscription,
        string? resourceGroup = null,
        string agentName = "",
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

    Task<SreAgentDeleteResult> DeleteSkillAsync(string endpoint, string name, string? tenant = null, CancellationToken cancellationToken = default);

    #endregion


    #region Connectors + Hooks (sub-agent B)

    Task<List<AgentConnector>> ListConnectorsAsync(
        string subscription,
        string resourceGroup,
        string agentName,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    Task<AgentConnector> GetConnectorAsync(
        string subscription,
        string resourceGroup,
        string agentName,
        string name,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    Task<AgentConnector> CreateOrUpdateConnectorAsync(
        string subscription,
        string resourceGroup,
        string agentName,
        string name,
        AgentConnectorEnvelope connector,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    Task DeleteConnectorAsync(
        string subscription,
        string resourceGroup,
        string agentName,
        string name,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    Task<string> ResolveAgentResourceGroupAsync(
        string subscription,
        string agentName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
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

    Task<List<SreAgentThreadMessage>> PollThreadForCompletionAsync(string endpoint, string threadId, string? tenant, TimeSpan timeout, bool autoApprove, CancellationToken cancellationToken = default);

    #endregion




    #region Incidents + Workflows + Docs + Architecture (sub-agent D)

    Task<List<ThreadListItem>> ListIncidentThreadsAsync(string endpoint, string? tenant = null, CancellationToken cancellationToken = default);

    Task<IncidentThreadResponse?> CreateIncidentThreadAsync(string endpoint, IncidentThreadCreateRequest request, string? tenant = null, CancellationToken cancellationToken = default);

    Task<List<IncidentFilter>> ListIncidentFiltersAsync(string endpoint, string? tenant = null, CancellationToken cancellationToken = default);

    Task<List<IncidentHandler>> ListIncidentHandlersAsync(string endpoint, string? tenant = null, CancellationToken cancellationToken = default);

    Task CreateOrUpdateIncidentFilterAsync(string endpoint, string filterId, IncidentFilterPayload payload, string? tenant = null, CancellationToken cancellationToken = default);

    Task DeleteIncidentFilterAsync(string endpoint, string filterId, string? tenant = null, CancellationToken cancellationToken = default);

    Task EnableIncidentFilterAsync(string endpoint, string filterId, string? tenant = null, CancellationToken cancellationToken = default);

    Task CreateOrUpdateIncidentHandlerAsync(string endpoint, string handlerId, IncidentHandler payload, string? tenant = null, CancellationToken cancellationToken = default);

    Task ApplyExtendedAgentResourceAsync(string endpoint, string kind, string name, ExtendedAgentResourceEnvelope payload, string? tenant = null, CancellationToken cancellationToken = default);

    Task<List<DocumentInfo>> ListMemoriesAsync(string endpoint, string? tenant = null, CancellationToken cancellationToken = default);

    Task DeleteMemoryAsync(string endpoint, string name, string? tenant = null, CancellationToken cancellationToken = default);

    Task<List<MemorySearchResult>> SearchMemoriesAsync(string endpoint, string query, int k = 10, string? tenant = null, CancellationToken cancellationToken = default);

    Task ReindexMemoriesAsync(string endpoint, string? tenant = null, CancellationToken cancellationToken = default);

    Task UploadMemoryAsync(string endpoint, string fileName, string content, string? tenant = null, CancellationToken cancellationToken = default);

    Task<List<CommonPromptEnvelope>> ListCommonPromptsAsync(string endpoint, string? search = null, string? tenant = null, CancellationToken cancellationToken = default);

    Task<CommonPromptEnvelope?> GetCommonPromptAsync(string endpoint, string name, string? tenant = null, CancellationToken cancellationToken = default);

    Task CreateOrUpdateCommonPromptAsync(string endpoint, string name, string content, string? tenant = null, CancellationToken cancellationToken = default);

    Task DeleteCommonPromptAsync(string endpoint, string name, string? tenant = null, CancellationToken cancellationToken = default);

    #endregion

}
