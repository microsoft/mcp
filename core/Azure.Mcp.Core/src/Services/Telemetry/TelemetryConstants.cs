// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Core.Services.Telemetry;

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
        public const string DevDeviceId = "DevDeviceId";
        public const string ErrorDetails = "ErrorDetails";
        public const string EventId = "EventId";
        public const string MacAddressHash = "MacAddressHash";
        public const string ResourceHash = "AzResourceHash";
        public const string SubscriptionGuid = "AzSubscriptionGuid";
        public const string ToolName = "ToolName";
        public const string ToolArea = "ToolArea";
        public const string ServerMode = "ServerMode";
        public const string IsServerCommandInvoked = "IsServerCommandInvoked";
    }

    internal class ActivityName
    {
        public const string CommandExecuted = "CommandExecuted";
        public const string ListToolsHandler = "ListToolsHandler";
        public const string ToolExecuted = "ToolExecuted";
    }
}
