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
        public const string ToolId = "ToolId";
        public const string ToolName = "ToolName";
        public const string ToolArea = "ToolArea";
        public const string ServerMode = "ServerMode";
        public const string IsServerCommandInvoked = "IsServerCommandInvoked";
        public const string Transport = "Transport";
        public const string IsReadOnly = "IsReadOnly";
        public const string Namespace = "Namespace";
        public const string ToolCount = "ToolCount";
        public const string InsecureDisableElicitation = "InsecureDisableElicitation";
        public const string IsDebug = "IsDebug";
        public const string EnableInsecureTransports = "EnableInsecureTransports";
        public const string Tool = "Tool";
    }

    internal class ActivityName
    {
        public const string CommandExecuted = "CommandExecuted";
        public const string ListToolsHandler = "ListToolsHandler";
        public const string ToolExecuted = "ToolExecuted";
        public const string ServerStarted = "ServerStarted";
    }
}
