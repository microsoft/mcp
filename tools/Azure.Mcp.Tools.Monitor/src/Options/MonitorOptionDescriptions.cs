// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Monitor.Options;

internal static class MonitorOptionDescriptions
{
    internal const string Workspace = "The Log Analytics workspace ID or name. This can be either the unique identifier (GUID) or the display name of your workspace.";
    internal const string Table = "The name of the table to query. This is the specific table within the workspace.";
    internal const string Query = "The KQL query to execute against the Log Analytics workspace. You can use predefined queries by name:\n" +
                                              "- 'recent': Shows most recent logs ordered by TimeGenerated\n" +
                                              "- 'errors': Shows error-level logs ordered by TimeGenerated\n" +
                                              "Otherwise, provide a custom KQL query.";
    internal const string Hours = "The number of hours to query back from now. Defaults to 24 hours.";
    internal const string Limit = "The maximum number of results to return. Defaults to 20.";
    internal const string SessionId = "The workspace path returned as sessionId from orchestrator-start.";

    // Metrics related descriptions
    internal const string MetricNamespace = "The metric namespace to query. Obtain this value from the azmcp-monitor-metrics-definitions command.";

    // Web Test related descriptions
    internal const string WebtestResource = "The name of the Web Test resource to operate on.";
}
