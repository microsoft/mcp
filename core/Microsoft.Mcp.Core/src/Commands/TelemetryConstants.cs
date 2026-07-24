// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Core.Commands;

/// <summary>
/// Name of tags published.
/// </summary>
public class TagName
{
    /// <summary>
    /// Name of the MCP server.
    /// </summary>
    public const string McpServerName = "McpServerNameV2";

    /// <summary>
    /// Version of the MCP server.
    /// </summary>
    public const string McpServerVersion = "Version";

    /// <summary>
    /// Name of the MCP client.
    /// </summary>
    public const string ClientName = "ClientName";

    /// <summary>
    /// Version of the MCP client.
    /// </summary>
    public const string ClientVersion = "ClientVersion";

    /// <summary>
    /// Unique identifier of the device hosting the MCP server.
    /// </summary>
    public const string DevDeviceId = "DevDeviceId";

    /// <summary>
    /// The message of the exception, if any, that was thrown during execution of a request.
    /// </summary>
    public const string ExceptionMessage = "exception.message";

    /// <summary>
    /// The type of the exception, if any, that was thrown during execution of a request.
    /// </summary>
    public const string ExceptionType = "exception.type";

    /// <summary>
    /// The stacktrace of the exception, if any, that was thrown during execution of a request.
    /// </summary>
    public const string ExceptionStackTrace = "exception.stacktrace";

    /// <summary>
    /// Unique identifier of the event that was published to telemetry.
    /// </summary>
    public const string EventId = "EventId";

    /// <summary>
    /// MAC address of the device hosting the MCP server.
    /// </summary>
    public const string MacAddressHash = "MacAddressHash";

    /// <summary>
    /// Unique identifier of the MCP tool that was executed. Only available for tools loaded through <see cref="CommandFactory"/>.
    /// </summary>
    public const string ToolId = "ToolId";

    /// <summary>
    /// Name of the MCP tool that was executed.
    /// </summary>
    public const string ToolName = "ToolName";

    /// <summary>
    /// Name of the area of the tool that was executed.
    /// </summary>
    public const string ToolArea = "ToolArea";

    /// <summary>
    /// Name of the source of the tool that was executed.
    /// </summary>
    public const string ToolSource = "ToolSource";

    /// <summary>
    /// Names of the parameters passed to the tool that was executed.
    /// </summary>
    public const string ToolParameters = "ToolParameters";

    /// <summary>
    /// The mode of the MCP server.
    /// </summary>
    public const string ServerMode = "ServerMode";

    /// <summary>
    /// Whether the tool was executed by the server. Both IsLearn and IsServerCommandInvoked can be true at the same time,
    /// if the tool learning resulted in an attempt to execute the tool.
    /// </summary>
    public const string IsServerCommandInvoked = "IsServerCommandInvoked";

    /// <summary>
    /// Whether the tool was executed in learn mode. Both IsLearn and IsServerCommandInvoked can be true at the same time,
    /// if the tool learning resulted in an attempt to execute the tool.
    /// </summary>
    public const string IsLearn = "IsLearn";

    /// <summary>
    /// The transport mode of the MCP server.
    /// </summary>
    public const string Transport = "Transport";

    /// <summary>
    /// Whether the MCP server is running in read-only mode.
    /// </summary>
    public const string IsReadOnly = "IsReadOnly";

    /// <summary>
    /// A list of tool areas, if any, that the MCP server is configured to allow.
    /// </summary>
    public const string Namespace = "Namespace";

    /// <summary>
    /// The number of tools that the MCP server is configured to allow.
    /// </summary>
    public const string ToolCount = "ToolCount";

    /// <summary>
    /// Whether the MCP server is configured to automatically approve / ignore tool elicitation requirements.
    /// </summary>
    public const string DangerouslyDisableElicitation = "DangerouslyDisableElicitation";

    /// <summary>
    /// Whether the MCP server is running in debug mode.
    /// </summary>
    public const string IsDebug = "IsDebug";

    /// <summary>
    /// Whether the MCP server is configured to disable authentication for incoming HTTP requests.
    /// </summary>
    public const string DangerouslyDisableHttpIncomingAuth = "DangerouslyDisableHttpIncomingAuth";

    /// <summary>
    /// A list of the tools that the MCP server is configured to allow.
    /// </summary>
    public const string Tool = "Tool";

    /// <summary>
    /// The unique identifier of the conversation in VS Code that initiated the request.
    /// </summary>
    public const string VSCodeConversationId = "VSCodeConversationId";

    /// <summary>
    /// The unique identifier of the request in VS Code that initiated the request.
    /// </summary>
    public const string VSCodeRequestId = "VSCodeRequestId";

    /// <summary>
    /// The operating system of the device hosting the MCP server.
    /// </summary>
    public const string Host = "Host";

    /// <summary>
    /// The architecture of the processor of the device hosting the MCP server.
    /// </summary>
    public const string ProcessorArchitecture = "ProcessorArchitecture";

    /// <summary>
    /// The Azure cloud type that the MCP is configured to use.
    /// </summary>
    public const string Cloud = "Cloud";

    /// <summary>
    /// The W3C traceparent header value for the request.
    /// </summary>
    public const string TraceParent = "w3c.traceparent";

    /// <summary>
    /// The W3C tracestate header value for the request.
    /// </summary>
    public const string TraceState = "w3c.tracestate";
}

public class ActivityName
{
    public const string ListToolsHandler = "ListToolsHandler";
    public const string ToolExecuted = "ToolExecuted";
    public const string ServerStarted = "ServerStarted";
    public const string PluginExecuted = "PluginExecuted";
}

public class AppInsightsInstanceType
{
    public const string Microsoft = "Microsoft";
    public const string UserProvided = "UserProvided";
}
