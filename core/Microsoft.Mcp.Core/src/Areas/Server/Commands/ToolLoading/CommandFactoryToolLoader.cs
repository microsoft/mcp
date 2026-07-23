// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Helpers;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Models.Command;
using ModelContextProtocol.Protocol;

namespace Microsoft.Mcp.Core.Areas.Server.Commands.ToolLoading;

/// <summary>
/// A tool loader that creates MCP tools from the registered command factory.
/// Exposes AzureMcp commands as MCP tools that can be invoked through the MCP protocol.
/// </summary>
public sealed class CommandFactoryToolLoader(
    IServiceProvider serviceProvider,
    ICommandFactory commandFactory,
    IOptions<ToolLoaderOptions> options,
    ILogger<CommandFactoryToolLoader> logger) : BaseToolLoader(logger)
{
    private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    private readonly ICommandFactory _commandFactory = commandFactory;
    private readonly IOptions<ToolLoaderOptions> _options = options;
    private IReadOnlyDictionary<string, IBaseCommand> _toolCommands =
        (options.Value.Namespace == null || options.Value.Namespace.Length == 0)
            ? commandFactory.AllCommands
            : commandFactory.GroupCommands(options.Value.Namespace);

    /// <summary>
    /// Lists all tools available from the command factory.
    /// </summary>
    /// <param name="request">The request context containing parameters and metadata.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A result containing the list of available tools.</returns>
    public override ValueTask<ListToolsResult> ListToolsHandler(RequestContext<ListToolsRequestParams> request, CancellationToken cancellationToken)
    {
        var visibleCommands = CommandFactory.GetVisibleCommands(_toolCommands);

        // Filter by specific tools if provided
        if (_options.Value.Tool != null && _options.Value.Tool.Length > 0)
        {
            visibleCommands = visibleCommands.Where(kvp =>
            {
                var toolKey = kvp.Key;
                return _options.Value.Tool.Any(tool => tool.Contains(toolKey, StringComparison.OrdinalIgnoreCase));
            });
        }

        // outputSchema only became part of the MCP specification in 2025-06-18, so only advertise it to
        // clients that negotiated that protocol version or newer. Older clients get the legacy shape.
        var supportsStructuredOutput = SupportsStructuredOutput(request.Server.NegotiatedProtocolVersion);

        var tools = visibleCommands
            .Where(kvp => !_options.Value.ReadOnly || kvp.Value.Metadata.ReadOnly)
            .Where(kvp => !_options.Value.IsHttpMode || !kvp.Value.Metadata.LocalRequired)
            .Select(kvp => GetTool(kvp.Key, kvp.Value, supportsStructuredOutput))
            .ToList();

        var listToolsResult = new ListToolsResult { Tools = tools };

        _logger.LogInformation("Listing {NumberOfTools} tools.", tools.Count);

        return ValueTask.FromResult(listToolsResult);
    }

    /// <summary>
    /// Handles tool calls by executing the corresponding command from the command factory.
    /// </summary>
    /// <param name="request">The request context containing parameters and metadata.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The result of the tool call operation.</returns>
    public override async ValueTask<CallToolResult> CallToolHandler(RequestContext<CallToolRequestParams> request, CancellationToken cancellationToken)
    {
        Activity.Current?.SetTag(TagName.IsServerCommandInvoked, false);
        if (request.Params == null)
        {
            var content = new TextContentBlock
            {
                Text = "Cannot call tools with null parameters.",
            };

            return new CallToolResult
            {
                Content = [content],
                IsError = true,
            };
        }

        var toolName = request.Params.Name;

        // Check if tool filtering is enabled and validate the requested tool
        if (_options.Value.Tool != null && _options.Value.Tool.Length > 0)
        {
            if (!_options.Value.Tool.Any(tool => tool.Contains(toolName, StringComparison.OrdinalIgnoreCase)))
            {
                var content = new TextContentBlock
                {
                    Text = $"Tool '{toolName}' is not available. This server is configured to only expose the tools: {string.Join(", ", _options.Value.Tool.Select(t => $"'{t}'"))}",
                };

                return new CallToolResult
                {
                    Content = [content],
                    IsError = true,
                };
            }
        }

        var activity = Activity.Current?.SetTag(TagName.ToolName, toolName);

        var command = _toolCommands.GetValueOrDefault(toolName);
        if (command == null)
        {
            var content = new TextContentBlock
            {
                Text = $"Could not find command: {toolName}",
            };

            return new CallToolResult
            {
                Content = [content],
                IsError = true,
            };
        }
        activity?.SetTag(TagName.ToolId, command.Id);

        // Enforce read-only mode at execution time
        if (_options.Value.ReadOnly && !command.Metadata.ReadOnly)
        {
            var content = new TextContentBlock
            {
                Text = $"Tool '{toolName}' is not available. This server is configured in read-only mode and this tool is not a read-only tool.",
            };

            return McpHelper.InjectToolIdMetadata(new CallToolResult
            {
                Content = [content],
                IsError = true,
            }, command.Id);
        }

        // Enforce HTTP mode restrictions at execution time
        if (_options.Value.IsHttpMode && command.Metadata.LocalRequired)
        {
            var content = new TextContentBlock
            {
                Text = $"Tool '{toolName}' is not available. This server is running in HTTP mode and this tool requires local execution.",
            };

            return McpHelper.InjectToolIdMetadata(new CallToolResult
            {
                Content = [content],
                IsError = true,
            }, command.Id);
        }

        var supportsStructuredOutput = command.ResultTypeInfo != null
            && SupportsStructuredOutput(request.Server.NegotiatedProtocolVersion);
        var commandArguments = request.Params.Arguments;
        var legacyContent = false;
        if (supportsStructuredOutput
            && !TryExtractLegacyContentArgument(request.Params.Arguments, out commandArguments, out legacyContent))
        {
            return McpHelper.InjectToolIdMetadata(new CallToolResult
            {
                Content = [new TextContentBlock
                {
                    Text = $"The '{LegacyContentArgumentName}' argument must be a Boolean."
                }],
                IsError = true
            }, command.Id);
        }

        var commandContext = new CommandContext(_serviceProvider, activity)
        {
            McpServer = request.Server,
            ProgressToken = request.Params.ProgressToken
        };

        // Check if this tool requires elicitation for sensitive or destructive operations
        var elicitationResult = await HandleElicitationAsync(
            request,
            toolName,
            command,
            _options.Value.DangerouslyDisableElicitation,
            _logger,
            cancellationToken);

        if (elicitationResult != null)
        {
            return elicitationResult;
        }

        var realCommand = command.GetCommand();
        ParseResult? commandOptions = null;

        var effectiveOptions = realCommand.Options
            .Where(o => !CommandFactory.IsLearnOption(o))
            .ToList();

        if (effectiveOptions.Count == 1 && IsRawMcpToolInputOption(effectiveOptions[0]))
        {
            commandOptions = realCommand.ParseFromRawMcpToolInput(commandArguments);
        }
        else
        {
            commandOptions = realCommand.ParseFromDictionary(commandArguments);
        }

        _logger.LogTrace("Invoking '{Tool}'.", realCommand.Name);

        if (commandContext.Activity != null)
        {
            var serviceArea = _commandFactory.GetServiceArea(toolName);
            commandContext.Activity.SetTag(TagName.ToolArea, serviceArea);
        }

        try
        {
            activity?.SetTag(TagName.IsServerCommandInvoked, true);
            var commandResponse = await command.ExecuteAsync(commandContext, commandOptions, cancellationToken);
            var jsonResponse = JsonSerializer.Serialize(commandResponse, ModelsJsonContext.Default.CommandResponse);
            var isError = commandResponse.Status < HttpStatusCode.OK || commandResponse.Status >= HttpStatusCode.Ambiguous;

            // Successful structured responses use a compact text block by default. The complete historical
            // response remains available on request for clients that do not expose structuredContent.
            var structuredContent = !isError && supportsStructuredOutput
                ? TryBuildStructuredContent(jsonResponse)
                : null;
            var contentText = structuredContent != null && !legacyContent
                ? CompactStructuredContentMessage
                : jsonResponse;

            var callToolResult = new CallToolResult
            {
                Content = [
                    new TextContentBlock {
                        Text = contentText
                    }
                ],
                StructuredContent = structuredContent,
                IsError = isError
            };

            return McpHelper.InjectToolIdMetadata(callToolResult, command.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred running '{Tool}'. ", realCommand.Name);
            throw;
        }
        finally
        {
            _logger.LogTrace("Finished executing '{Tool}'.", realCommand.Name);
        }
    }

    /// <summary>
    /// Converts a command to an MCP tool definition.
    /// </summary>
    /// <param name="fullName">The full name of the command.</param>
    /// <param name="command">The command to convert.</param>
    /// <returns>An MCP tool definition.</returns>
    private static Tool GetTool(string fullName, IBaseCommand command, bool supportsStructuredOutput)
    {
        var underlyingCommand = command.GetCommand();
        var tool = new Tool
        {
            Name = fullName,
            Description = underlyingCommand.Description,
        };

        // Get tool metadata from the command's Metadata property
        var metadata = command.Metadata;
        tool.Annotations = new ToolAnnotations()
        {
            DestructiveHint = metadata.Destructive,
            IdempotentHint = metadata.Idempotent,
            OpenWorldHint = metadata.OpenWorld,
            ReadOnlyHint = metadata.ReadOnly,
            Title = command.Title,
        };

        JsonObject meta = [new(McpHelper.ToolIdMetaKey, command.Id)];
        // Add Secret metadata to tool.Meta if the property exists
        if (metadata.Secret)
        {
            meta[McpHelper.SecretHintMetaKey] = metadata.Secret;
        }
        // Add LocalRequired metadata to tool.Meta if the property exists
        if (metadata.LocalRequired)
        {
            meta[McpHelper.LocalRequiredHintMetaKey] = metadata.LocalRequired;
        }
        tool.Meta = meta;

        var resultTypeInfo = supportsStructuredOutput
            ? command.ResultTypeInfo
            : null;
        if (resultTypeInfo != null)
        {
            var outputSchema = OptionSchemaGenerator.CreateOutputSchema(resultTypeInfo);
            tool.OutputSchema = JsonSerializer.SerializeToElement(outputSchema, ServerJsonContext.Default.JsonObject);
        }

        var options = command.GetCommand().Options
            .Where(o => !CommandFactory.IsLearnOption(o))
            .ToList();

        var inputSchema = options.Count == 1 && IsRawMcpToolInputOption(options[0])
            ? JsonNode.Parse(options[0].Description ?? "{}") as JsonObject ?? []
            : OptionSchemaGenerator.CreateInputSchema(options);

        if (resultTypeInfo != null)
        {
            AddLegacyContentArgument(inputSchema);
        }

        tool.InputSchema = JsonSerializer.SerializeToElement(inputSchema, ServerJsonContext.Default.JsonObject);

        return tool;
    }

    /// <summary>
    /// The first MCP protocol version that includes <c>outputSchema</c> and <c>structuredContent</c> in
    /// the specification. Clients that negotiated an older version receive the legacy content-only shape.
    /// </summary>
    internal const string StructuredContentMinProtocolVersion = "2025-06-18";

    internal const string LegacyContentArgumentName = "legacy-content";
    internal const string CompactStructuredContentMessage =
        "Response successful. See structuredContent for details. If structuredContent is unavailable, retry with \"legacy-content\": true.";
    private const string LegacyContentArgumentDescription =
        "Return the complete response in content for clients that do not expose structuredContent.";
    private static readonly DateOnly StructuredContentMinProtocolDate = new(2025, 6, 18);

    /// <summary>
    /// Determines whether a client that negotiated <paramref name="negotiatedProtocolVersion"/> can be sent
    /// <c>outputSchema</c> and <c>structuredContent</c>. MCP protocol versions must be valid
    /// <c>YYYY-MM-DD</c> dates. A null, empty, malformed, or older value is treated as unsupported so the
    /// server falls back to the legacy shape.
    /// </summary>
    /// <remarks><see langword="internal"/> (rather than <see langword="private"/>) so the version gate can be
    /// unit tested directly without exercising the full tool-loading pipeline.</remarks>
    internal static bool SupportsStructuredOutput(string? negotiatedProtocolVersion) =>
        DateOnly.TryParseExact(
            negotiatedProtocolVersion,
            "yyyy-MM-dd",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var protocolDate)
        && protocolDate >= StructuredContentMinProtocolDate;

    private static void AddLegacyContentArgument(JsonObject inputSchema)
    {
        if (inputSchema["properties"] is not JsonObject properties)
        {
            properties = [];
            inputSchema["properties"] = properties;
        }

        if (properties.Any(property =>
            string.Equals(property.Key, LegacyContentArgumentName, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException(
                $"Command input schema already defines the reserved '{LegacyContentArgumentName}' argument.");
        }

        properties[LegacyContentArgumentName] = new JsonObject
        {
            ["type"] = "boolean",
            ["description"] = LegacyContentArgumentDescription,
            ["default"] = false
        };
    }

    private static bool TryExtractLegacyContentArgument(
        IDictionary<string, JsonElement>? arguments,
        out IDictionary<string, JsonElement>? commandArguments,
        out bool legacyContent)
    {
        commandArguments = arguments;
        legacyContent = false;

        if (arguments is null)
        {
            return true;
        }

        string? argumentKey = null;
        foreach (var key in arguments.Keys)
        {
            if (!string.Equals(key, LegacyContentArgumentName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (argumentKey is not null)
            {
                return false;
            }

            argumentKey = key;
        }

        if (argumentKey is null)
        {
            return true;
        }

        var value = arguments[argumentKey];
        if (value.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
        {
            return false;
        }

        legacyContent = value.GetBoolean();
        var filteredArguments = new Dictionary<string, JsonElement>(arguments);
        filteredArguments.Remove(argumentKey);
        commandArguments = filteredArguments;
        return true;
    }

    /// <summary>
    /// Extracts the command result payload from a serialized <see cref="CommandResponse"/> and shapes it
    /// into the <c>structuredContent</c> value. Object payloads are used as-is; array or scalar payloads
    /// are wrapped under a single <c>value</c> property to match the wrapping applied by
    /// <see cref="OptionSchemaGenerator.CreateOutputSchema"/>. Returns <see langword="null"/> when there
    /// is no result payload.
    /// </summary>
    /// <remarks><see langword="internal"/> (rather than <see langword="private"/>) so the payload-shaping
    /// contract can be unit tested directly without exercising the full call-tool pipeline.</remarks>
    internal static JsonElement? TryBuildStructuredContent(string jsonResponse)
    {
        using var document = JsonDocument.Parse(jsonResponse);

        if (!document.RootElement.TryGetProperty("results", out var results))
        {
            return null;
        }

        switch (results.ValueKind)
        {
            case JsonValueKind.Object:
                return results.Clone();
            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
                return null;
            default:
                var wrapper = new JsonObject { ["value"] = JsonNode.Parse(results.GetRawText()) };
                return JsonSerializer.SerializeToElement(wrapper, ServerJsonContext.Default.JsonObject);
        }
    }

    /// <summary>
    /// Disposes resources owned by this tool loader.
    /// CommandFactoryToolLoader doesn't own external resources that need disposal.
    /// </summary>
    protected override ValueTask DisposeAsyncCore()
    {
        // CommandFactoryToolLoader doesn't create or manage disposable resources
        return ValueTask.CompletedTask;
    }
}
