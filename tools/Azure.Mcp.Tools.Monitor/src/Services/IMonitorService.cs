// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Nodes;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Monitor.Models;
using Azure.Mcp.Tools.Monitor.Models.ActivityLog;

namespace Azure.Mcp.Tools.Monitor.Services;

public interface IMonitorService
{
    Task<List<JsonNode>> QueryResourceLogs(
        string subscription,
        string resourceId,
        string query,
        string table,
        int? hours = 24,
        int? limit = 20,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);

    Task<List<JsonNode>> QueryWorkspace(
        string subscription,
        string workspace,
        string query,
        int timeSpanDays = 1,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);

    Task<List<string>> ListTables(
        string subscription,
        string resourceGroup,
        string workspace, string? tableType = "CustomLog",
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);

    Task<List<WorkspaceInfo>> ListWorkspaces(
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);

    Task<List<JsonNode>> QueryWorkspaceLogs(
        string subscription,
        string workspace,
        string query,
        string table,
        int? hours = 24, int? limit = 20,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);

    Task<List<string>> ListTableTypes(
        string subscription,
        string resourceGroup,
        string workspace,
        string? tenant,
        RetryPolicyOptions? retryPolicy);

    Task<List<ActivityLogEventData>> ListActivityLogs(
        string subscription,
        string resourceName,
        string? resourceGroup = null,
        string? resourceType = null,
        double hours = 24.0,
        ActivityLogEventLevel? eventLevel = null,
        int top = 10,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);
}
