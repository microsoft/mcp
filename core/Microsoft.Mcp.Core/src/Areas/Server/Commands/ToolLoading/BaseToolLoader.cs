// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace Microsoft.Mcp.Core.Areas.Server.Commands.ToolLoading;

/// <summary>
/// Base class for tool loaders that provides common functionality including disposal patterns.
/// </summary>
/// <param name="logger">Logger instance for this tool loader.</param>
public abstract class BaseToolLoader(ILogger logger) : IToolLoader
{
    /// <summary>
    /// Logger instance for this tool loader.
    /// </summary>
    protected readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Cached empty JSON object to avoid repeated parsing.
    /// </summary>
    protected static readonly JsonElement EmptyJsonObject;

    static BaseToolLoader()
    {
        using (var jsonDoc = JsonDocument.Parse("{}"))
        {
            EmptyJsonObject = jsonDoc.RootElement.Clone();
        }
    }

    private bool _disposed = false;

    /// <summary>
    /// Handles requests to list all tools available in the MCP server.
    /// </summary>
    /// <param name="request">The request context containing metadata and parameters.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A result containing the list of available tools.</returns>
    public abstract ValueTask<ListToolsResult> ListToolsHandler(RequestContext<ListToolsRequestParams> request, CancellationToken cancellationToken);

    /// <summary>
    /// Handles requests to call a specific tool with the provided parameters.
    /// </summary>
    /// <param name="request">The request context containing the tool name and parameters.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A result containing the output of the tool invocation.</returns>
    public abstract ValueTask<CallToolResult> CallToolHandler(RequestContext<CallToolRequestParams> request, CancellationToken cancellationToken);

    /// <summary>
    /// Extracts the "parameters" JsonElement from the tool call request arguments.
    /// </summary>
    /// <param name="request">The request context containing the tool call parameters.</param>
    /// <returns>
    /// The "parameters" JsonElement if it exists and is a valid JSON object;
    /// otherwise, returns an empty JSON object.
    /// </returns>
    protected static JsonElement GetParametersJsonElement(RequestContext<CallToolRequestParams> request)
    {
        IDictionary<string, JsonElement>? args = request.Params?.Arguments;
        if (args != null && args.TryGetValue("parameters", out var parametersElem) && parametersElem.ValueKind == JsonValueKind.Object)
        {
            return parametersElem;
        }

        return EmptyJsonObject;
    }

    /// <summary>
    /// Extracts the "parameters" object from the tool call request arguments and converts it to a dictionary.
    /// </summary>
    /// <param name="request">The request context containing the tool call parameters.</param>
    /// <returns>
    /// A dictionary containing the parameter names and values if the "parameters" object exists and is valid;
    /// otherwise, returns an empty dictionary.
    /// </returns>
    protected static Dictionary<string, object?> GetParametersDictionary(RequestContext<CallToolRequestParams> request)
    {
        JsonElement parametersElem = GetParametersJsonElement(request);
        return parametersElem.EnumerateObject().ToDictionary(prop => prop.Name, prop => (object?)prop.Value);
    }

    /// <summary>
    /// Disposes resources owned by this tool loader with double disposal protection.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        try
        {
            await DisposeAsyncCore();
        }
        catch (Exception ex)
        {
            // Log disposal failures but don't throw - we want to ensure cleanup continues
            // Individual disposal errors shouldn't stop the overall disposal process
            _logger.LogError(ex, "Error occurred while disposing tool loader {LoaderType}. Some resources may not have been properly disposed.", GetType().Name);
        }
        finally
        {
            _disposed = true;
        }
    }

    /// <summary>
    /// Override this method in derived classes to implement disposal logic.
    /// This method is called exactly once during disposal.
    /// </summary>
    /// <returns>A task representing the asynchronous disposal operation.</returns>
    protected virtual ValueTask DisposeAsyncCore()
    {
        // Default implementation does nothing
        return ValueTask.CompletedTask;
    }

    protected McpClientOptions CreateClientOptions(McpServer server)
    {
        McpClientHandlers handlers = new();

        if (server.ClientCapabilities?.Sampling != null)
        {
            handlers.SamplingHandler = (request, progress, token) =>
            {
                ArgumentNullException.ThrowIfNull(request);
                return server.SampleAsync(request, token);
            };
        }

        if (server.ClientCapabilities?.Elicitation != null)
        {
            handlers.ElicitationHandler = (request, token) =>
            {
                ArgumentNullException.ThrowIfNull(request);
                return server.ElicitAsync(request, token);
            };
        }

        var clientOptions = new McpClientOptions
        {
            ClientInfo = server.ClientInfo,
            Handlers = handlers
        };

        return clientOptions;
    }

    /// <summary>
    /// Handles elicitation for commands that are sensitive, destructive, or both.
    /// Builds a single consolidated prompt based on which metadata flags are set.
    /// If elicitation is disabled or not supported, returns appropriate error result.
    /// </summary>
    /// <param name="request">The request context containing the MCP server.</param>
    /// <param name="toolName">The name of the tool being invoked.</param>
    /// <param name="metadata">The tool metadata containing Secret and Destructive flags.</param>
    /// <param name="dangerouslyDisableElicitation">Whether elicitation has been disabled via dangerous option.</param>
    /// <param name="logger">Logger instance for recording elicitation events.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>
    /// Null if elicitation was accepted or bypassed (operation should proceed).
    /// A CallToolResult with IsError=true if elicitation was rejected or failed (operation should not proceed).
    /// </returns>
    protected static async Task<CallToolResult?> HandleElicitationAsync(
        RequestContext<CallToolRequestParams> request,
        string toolName,
        ToolMetadata metadata,
        bool dangerouslyDisableElicitation,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        bool isSecret = metadata.Secret;
        bool isDestructive = metadata.Destructive;

        if (!isSecret && !isDestructive)
        {
            return null;
        }

        string reason = (isSecret, isDestructive) switch
        {
            (true, true) => "handles sensitive data and may perform destructive operations",
            (true, false) => "handles sensitive data",
            (false, true) => "may perform destructive operations",
            _ => string.Empty
        };

        if (dangerouslyDisableElicitation)
        {
            logger.LogWarning("Tool '{Tool}' {Reason} but elicitation is disabled via --dangerously-disable-elicitation. Proceeding without user consent.", toolName, reason);
            return null;
        }

        if (!request.Server.SupportsElicitation())
        {
            logger.LogWarning("Tool '{Tool}' {Reason} but client does not support elicitation. Operation rejected.", toolName, reason);
            return new CallToolResult
            {
                Content = [new TextContentBlock { Text = $"This tool {reason} and requires user consent, but the client does not support elicitation. Operation rejected for security." }],
                IsError = true
            };
        }

        try
        {
            logger.LogInformation("Tool '{Tool}' {Reason}. Requesting user confirmation via elicitation.", toolName, reason);

            string message = (isSecret, isDestructive) switch
            {
                (true, true) =>
                    $"⚠️ WARNING: The tool '{toolName}' may expose secrets or sensitive information AND may delete or modify existing resources.\n\n" +
                    "This operation could reveal confidential data such as passwords, API keys, or certificates, " +
                    "and could permanently alter or remove Azure resources, configurations, or data.\n\n" +
                    "Do you want to continue?",
                (true, false) =>
                    $"⚠️ SECURITY WARNING: The tool '{toolName}' may expose secrets or sensitive information.\n\n" +
                    "This operation could reveal confidential data such as passwords, API keys, certificates, or other sensitive values.\n\n" +
                    "Do you want to continue with this potentially sensitive operation?",
                _ =>
                    $"⚠️ DESTRUCTIVE OPERATION WARNING: The tool '{toolName}' may delete or modify existing resources.\n\n" +
                    "This operation could permanently alter or remove Azure resources, configurations, or data.\n\n" +
                    "Do you want to continue with this potentially destructive operation?"
            };

            // Create the elicitation request with a single-select enum for approve/reject
            var protocolRequest = new ElicitRequestParams
            {
                Message = message,
                RequestedSchema = new()
                {
                    Properties = new Dictionary<string, ElicitRequestParams.PrimitiveSchemaDefinition>
                    {
                        ["decision"] = new ElicitRequestParams.TitledSingleSelectEnumSchema
                        {
                            Title = "Decision",
                            Description = "Approve or reject this sensitive operation.",
                            OneOf = new List<ElicitRequestParams.EnumSchemaOption>
                            {
                                new() { Title = "Approve", Const = "accept" },
                                new() { Title = "Reject", Const = "reject" }
                            }
                        }
                    },
                    Required = ["decision"]
                }
            };

            var protocolResponse = await request.Server.ElicitAsync(protocolRequest, cancellationToken);

            if (protocolResponse.Action != "accept")
            {
                logger.LogInformation("User {Action} the elicitation for tool '{Tool}'. Operation not executed.",
                    protocolResponse.Action, toolName);
                return new CallToolResult
                {
                    Content = [new TextContentBlock { Text = $"Operation cancelled by user ({protocolResponse.Action})." }],
                    IsError = true
                };
            }

            logger.LogInformation("User accepted elicitation for tool '{Tool}'. Proceeding with execution.", toolName);
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during elicitation for tool '{Tool}': {Error}", toolName, ex.Message);
            return new CallToolResult
            {
                Content = [new TextContentBlock { Text = $"Elicitation failed for tool '{toolName}': {ex.Message}. Operation not executed for security." }],
                IsError = true
            };
        }
    }
}
