// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Core.Services.Telemetry;

internal static class TelemetryConstants
{
    /// <summary>
    /// Name of tags published.
    /// </summary>
    internal class TagName
    {
        public const string AzureMcpVersion = "Version";
        public const string ClientName = "ClientName";
        public const string ClientVersion = "ClientVersion";
        public const string ErrorDetails = "ErrorDetails";
        public const string EventId = "EventId";
        public const string ToolName = "ToolName";
        public const string ToolArea = "ToolArea";
    }

    internal class ActivityName
    {
        public const string ListToolsHandler = "ListToolsHandler";
        public const string ToolExecuted = "ToolExecuted";
    }
}
