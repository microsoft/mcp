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
}
