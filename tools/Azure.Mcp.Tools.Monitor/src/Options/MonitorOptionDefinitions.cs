// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Monitor.Options;

internal static class MonitorOptionDefinitions
{
    internal const string WorkspaceDescription = "The Log Analytics workspace ID or name. This can be either the unique identifier (GUID) or the display name of your workspace.";
    internal const string TableDescription = "The name of the table to query. This is the specific table within the workspace.";
    internal const string QueryDescription = "The KQL query to execute against the Log Analytics workspace. You can use predefined queries by name:\n" +
                                              "- 'recent': Shows most recent logs ordered by TimeGenerated\n" +
                                              "- 'errors': Shows error-level logs ordered by TimeGenerated\n" +
                                              "Otherwise, provide a custom KQL query.";
    internal const string HoursDescription = "The number of hours to query back from now. Defaults to 24 hours.";
    internal const string LimitDescription = "The maximum number of results to return. Defaults to 20.";
    internal const string SessionIdDescription = "The workspace path returned as sessionId from orchestrator-start.";

    // Metrics related descriptions
    internal const string MetricNamespaceDescription = "The metric namespace to query. Obtain this value from the azmcp-monitor-metrics-definitions command.";

    // Web Test related descriptions
    internal const string WebtestResourceDescription = "The name of the Web Test resource to operate on.";
}
