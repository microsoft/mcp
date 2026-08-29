// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Diagnostics;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Mcp.Core.Areas.Server.Options;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Helpers;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Models.Command;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace Microsoft.Mcp.Core.Areas.Server.Commands.ToolLoading;

/// <summary>
/// A tool loader that creates MCP tools from the registered command factory.
/// Exposes MCP commands as MCP tools that can be invoked through the MCP protocol.
/// </summary>
public sealed class CommandFactoryToolLoader(
    ICommandFactory commandFactory,
    IOptions<ServerRuntimeConfiguration> configuration,
    ILogger<CommandFactoryToolLoader> logger) : BaseToolLoader(logger)
{
    private readonly ICommandFactory _commandFactory = commandFactory;
    private readonly IOptions<ServerRuntimeConfiguration> _configuration = configuration;
    private bool StructuredOutputEnabled => _configuration.Value.StructuredOutputMode != null;
    private IReadOnlyDictionary<string, IBaseCommand> _toolCommands =
        (configuration.Value.Namespace == null || configuration.Value.Namespace.Length == 0)
            ? commandFactory.AllCommands
            : commandFactory.GroupCommands(configuration.Value.Namespace);

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
        if (_configuration.Value.Tool != null && _configuration.Value.Tool.Length > 0)
        {
            visibleCommands = visibleCommands.Where(kvp =>
            {
                var toolKey = kvp.Key;
                return _configuration.Value.Tool.Any(tool => tool.Contains(toolKey, StringComparison.OrdinalIgnoreCase));
            });
        }

        var tools = visibleCommands
            .Where(kvp => !_configuration.Value.ReadOnly || kvp.Value.Metadata.ReadOnly)
            .Where(kvp => !_configuration.Value.IsHttpMode || !kvp.Value.Metadata.LocalRequired)
            .Select(kvp => GetTool(kvp.Key, kvp.Value, StructuredOutputEnabled))
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

        Activity.Current?.SetTag(TagName.IsServerCommandInvoked, false)
            .SetTag(TagName.ToolParameters, McpHelper.CreateToolParametersTelemetry(request.Params.Arguments?.Keys));

        var toolName = request.Params.Name;

        // Check if tool filtering is enabled and validate the requested tool
        if (_configuration.Value.Tool != null && _configuration.Value.Tool.Length > 0)
        {
            if (!_configuration.Value.Tool.Any(tool => tool.Contains(toolName, StringComparison.OrdinalIgnoreCase)))
            {
                var content = new TextContentBlock
                {
                    Text = $"Tool '{toolName}' is not available. This server is configured to only expose the tools: {string.Join(", ", _configuration.Value.Tool.Select(t => $"'{t}'"))}",
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
        activity?.SetTag(TagName.ToolId, command.Id)
            .SetTag(TagName.ToolSource, "internal")
            .SetTag(TagName.ToolAnnotations, McpHelper.CreateToolAnnotationTelemetry(command));

        // Enforce read-only mode at execution time
        if (_configuration.Value.ReadOnly && !command.Metadata.ReadOnly)
        {
            var content = new TextContentBlock
            {
                Text = $"Tool '{toolName}' is not available. This server is configured in read-only mode and this tool is not a read-only tool.",
            };

            return new CallToolResult
            {
                Content = [content],
                IsError = true,
                Meta = new([new(McpHelper.ToolIdMetaKey, command.Id)])
            };
        }

        // Enforce HTTP mode restrictions at execution time
        if (_configuration.Value.IsHttpMode && command.Metadata.LocalRequired)
        {
            var content = new TextContentBlock
            {
                Text = $"Tool '{toolName}' is not available. This server is running in HTTP mode and this tool requires local execution.",
            };

            return new CallToolResult
            {
                Content = [content],
                IsError = true,
                Meta = new([new(McpHelper.ToolIdMetaKey, command.Id)])
            };
        }

        var commandContext = new CommandContext(activity)
        {
            McpServer = request.Server,
            ProgressToken = request.Params.ProgressToken
        };

        // Check if this tool requires elicitation for sensitive or destructive operations
        var elicitationResult = await HandleElicitationAsync(
            request,
            toolName,
            command,
            _configuration.Value.DangerouslyDisableElicitation,
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
            commandOptions = realCommand.ParseFromRawMcpToolInput(request.Params.Arguments);
        }
        else
        {
            if (!realCommand.TryParseFromDictionary(request.Params.Arguments, out commandOptions, out var parseErrors))
            {
                return new CallToolResult
                {
                    Content =
                    [
                        new TextContentBlock
                        {
                            Text = parseErrors!,
                        }
                    ],
                    IsError = true,
                    Meta = new([new(McpHelper.ToolIdMetaKey, command.Id)])
                };
            }
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
            var commandResponse = await command.ExecuteAsync(commandContext, commandOptions!, cancellationToken);
            var jsonResponse = JsonSerializer.Serialize(commandResponse, ModelsJsonContext.Default.CommandResponse);
            var isError = commandResponse.Status < HttpStatusCode.OK || commandResponse.Status >= HttpStatusCode.Ambiguous;

            var structuredContent = !isError && StructuredOutputEnabled && command.ResultTypeInfo != null
                ? StructuredOutputHelper.TryBuildStructuredContent(jsonResponse)
                : null;
            var contentText = structuredContent != null && StructuredOutputMode.Compact == _configuration.Value.StructuredOutputMode
                ? StructuredOutputHelper.CompactContentMessage
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
    private static Tool GetTool(
        string fullName,
        IBaseCommand command,
        bool structuredOutputEnabled)
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

        var resultTypeInfo = structuredOutputEnabled
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

        tool.InputSchema = JsonSerializer.SerializeToElement(inputSchema, ServerJsonContext.Default.JsonObject);

        return tool;
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
