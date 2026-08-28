// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Diagnostics;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Mcp.Core.Areas.Server.Commands.Discovery;
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
public sealed partial class CommandFactoryToolLoader(
    ICommandFactory commandFactory,
    IOptions<ServerRuntimeConfiguration> configuration,
    ILogger<CommandFactoryToolLoader> logger,
    IConsolidatedToolDefinitionProvider? consolidatedToolDefinitionProvider = null) : BaseToolLoader(logger)
{
    internal static readonly string[] UtilityNamespaces = ["subscription", "group"];
    private static readonly string[] s_ignoredCommandGroups = ["extension", "server", "tools", .. UtilityNamespaces];

    private readonly ICommandFactory _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
    private readonly IOptions<ServerRuntimeConfiguration> _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    private readonly IReadOnlyDictionary<string, IBaseCommand> _toolCommands =
        (configuration.Value.Mode == ModeTypes.ConsolidatedProxy ||
         configuration.Value.Namespace == null ||
         configuration.Value.Namespace.Length == 0)
            ? commandFactory.AllCommands
            : commandFactory.GroupCommands(configuration.Value.Namespace);

    private readonly Lazy<IReadOnlyList<CommandGroup>> _commandGroups = new(() =>
    {
        if (configuration.Value.Mode == ModeTypes.ConsolidatedProxy)
        {
            return CreateConsolidatedCommandGroups(
                commandFactory,
                consolidatedToolDefinitionProvider ?? throw new InvalidOperationException("A consolidated tool definition provider is required in consolidated mode."),
                configuration.Value,
                logger);
        }

        IEnumerable<CommandGroup> commandGroups = commandFactory.RootGroup.SubGroup;

        commandGroups = commandGroups
            .Where(group => !s_ignoredCommandGroups.Contains(group.Name, StringComparer.OrdinalIgnoreCase))
            .Where(group => configuration.Value.Namespace == null ||
                           configuration.Value.Namespace.Length == 0 ||
                           configuration.Value.Namespace.Contains(group.Name, StringComparer.OrdinalIgnoreCase));

        return [.. commandGroups];
    });

    private readonly Lazy<IReadOnlyDictionary<string, IBaseCommand>> _utilityCommands = new(() =>
    {
        var utilityNamespaces = new List<string>(UtilityNamespaces);
        if (configuration.Value.Namespace == null || configuration.Value.Namespace.Length == 0)
        {
            utilityNamespaces.Add("extension");
        }

        return commandFactory.GroupCommands([.. utilityNamespaces]);
    });

    private ListToolsResult? _cachedListToolsResult;

    private static readonly JsonElement s_namespaceToolSchema = JsonSerializer.Deserialize("""
                {
                    "type": "object",
                    "properties": {
                        "intent": {
                            "type": "string",
                            "description": "The intent of the operation to perform."
                        },
                        "command": {
                            "type": "string",
                            "description": "The command to execute against the specified tool."
                        },
                        "parameters": {
                            "type": "object",
                            "description": "The parameters to pass to the tool command."
                        },
                        "learn": {
                            "type": "boolean",
                            "description": "To learn about the tool and its supported child tools and parameters.",
                            "default": false
                        }
                    },
                    "required": ["intent"],
                    "additionalProperties": false
                }
                """, ServerJsonContext.Default.JsonElement);

    /// <summary>
    /// Lists all tools available from the command factory.
    /// </summary>
    /// <param name="request">The request context containing parameters and metadata.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A result containing the list of available tools.</returns>
    public override ValueTask<ListToolsResult> ListToolsHandler(RequestContext<ListToolsRequestParams> request, CancellationToken cancellationToken)
    {
        if (_configuration.Value.Mode != ModeTypes.All)
        {
            return ValueTask.FromResult(ListCommandGroups());
        }

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
            .Select(kvp => GetTool(kvp.Key, kvp.Value))
            .ToList();

        var listToolsResult = new ListToolsResult { Tools = tools };

        _logger.LogInformation("Listing {NumberOfTools} tools.", tools.Count);

        return ValueTask.FromResult(listToolsResult);
    }

    private ListToolsResult ListCommandGroups()
    {
        if (_cachedListToolsResult != null)
        {
            return _cachedListToolsResult;
        }

        var tools = _commandGroups.Value
            .Where(group => !_configuration.Value.ReadOnly || !AllToolsInGroupMatch(metadata => !metadata.ReadOnly, group))
            .Where(group => !_configuration.Value.IsHttpMode || !AllToolsInGroupMatch(metadata => metadata.LocalRequired, group))
            .Select(CreateCommandGroupTool)
            .ToList();

        if (_configuration.Value.Mode == ModeTypes.NamespaceProxy)
        {
            var utilityCommands = CommandFactory.GetVisibleCommands(_utilityCommands.Value)
                .Where(command => IsToolIncluded(command.Key))
                .Where(command => !_configuration.Value.ReadOnly || command.Value.Metadata.ReadOnly)
                .Where(command => !_configuration.Value.IsHttpMode || !command.Value.Metadata.LocalRequired)
                .Select(command => GetTool(command.Key, command.Value));

            tools.AddRange(utilityCommands);
        }

        _cachedListToolsResult = new ListToolsResult { Tools = tools };
        _logger.LogInformation("Listing {NumberOfTools} tools.", tools.Count);
        return _cachedListToolsResult;
    }

    private static Tool CreateCommandGroupTool(CommandGroup group) => new()
    {
        Name = group.Name,
        Description = group.Description + """
            This tool is a hierarchical MCP command router.
            Sub commands require specific fields inside the "parameters" object.
            To invoke a command, set "command" and wrap its args in "parameters".
            Set "learn=true" to discover available sub commands.
            """,
        InputSchema = s_namespaceToolSchema,
        Annotations = new ToolAnnotations
        {
            Title = group.Title ?? group.Name,
            DestructiveHint = group.ToolMetadata?.Destructive,
            IdempotentHint = group.ToolMetadata?.Idempotent,
            OpenWorldHint = group.ToolMetadata?.OpenWorld,
            ReadOnlyHint = group.ToolMetadata?.ReadOnly,
        },
    };

    private bool IsToolIncluded(string toolName) =>
        _configuration.Value.Tool == null ||
        _configuration.Value.Tool.Length == 0 ||
        _configuration.Value.Tool.Any(tool => tool.Contains(toolName, StringComparison.OrdinalIgnoreCase));

    private static bool AllToolsInGroupMatch(Predicate<ToolMetadata> predicate, CommandGroup group)
    {
        foreach (var command in group.Commands.Values)
        {
            if (!predicate(command.Metadata))
            {
                return false;
            }
        }

        return group.SubGroup.All(subGroup => AllToolsInGroupMatch(predicate, subGroup));
    }

    /// <summary>
    /// Handles tool calls by executing the corresponding command from the command factory.
    /// </summary>
    /// <param name="request">The request context containing parameters and metadata.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The result of the tool call operation.</returns>
    public override async ValueTask<CallToolResult> CallToolHandler(RequestContext<CallToolRequestParams> request, CancellationToken cancellationToken)
    {
        if (_configuration.Value.Mode != ModeTypes.All && IsCommandGroupTool(request.Params?.Name))
        {
            return await CallCommandGroupToolHandler(request, cancellationToken);
        }

        if (_configuration.Value.Mode != ModeTypes.All &&
            (_configuration.Value.Mode != ModeTypes.NamespaceProxy ||
             request.Params == null ||
             !_utilityCommands.Value.ContainsKey(request.Params.Name)))
        {
            return ToolNotFound(request.Params?.Name);
        }

        return await CallFlatToolHandler(request, cancellationToken);
    }

    private async ValueTask<CallToolResult> CallFlatToolHandler(RequestContext<CallToolRequestParams> request, CancellationToken cancellationToken)
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

        var commands = _configuration.Value.Mode == ModeTypes.NamespaceProxy
            ? _utilityCommands.Value
            : _toolCommands;
        var command = commands.GetValueOrDefault(toolName);
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

        return await ExecuteCommandAsync(
            request,
            toolName,
            toolName,
            command,
            request.Params.Arguments,
            _commandFactory.GetServiceArea(toolName),
            cancellationToken);
    }

    private async ValueTask<CallToolResult> ExecuteCommandAsync(
        RequestContext<CallToolRequestParams> request,
        string toolName,
        string displayName,
        IBaseCommand command,
        IDictionary<string, JsonElement>? arguments,
        string? serviceArea,
        CancellationToken cancellationToken)
    {
        var activity = Activity.Current?.SetTag(TagName.ToolName, toolName);

        activity?.SetTag(TagName.ToolId, command.Id)
            .SetTag(TagName.ToolSource, "internal")
            .SetTag(TagName.ToolAnnotations, McpHelper.CreateToolAnnotationTelemetry(command));

        // Enforce read-only mode at execution time
        if (_configuration.Value.ReadOnly && !command.Metadata.ReadOnly)
        {
            var content = new TextContentBlock
            {
                Text = $"Tool '{displayName}' is not available. This server is configured in read-only mode and this tool is not a read-only tool.",
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
                Text = $"Tool '{displayName}' is not available. This server is running in HTTP mode and this tool requires local execution.",
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
            displayName,
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
            commandOptions = realCommand.ParseFromRawMcpToolInput(arguments);
        }
        else
        {
            if (!realCommand.TryParseFromDictionary(arguments, out commandOptions, out var parseErrors))
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
            commandContext.Activity.SetTag(TagName.ToolArea, serviceArea);
        }

        try
        {
            activity?.SetTag(TagName.IsServerCommandInvoked, true);
            var commandResponse = await command.ExecuteAsync(commandContext, commandOptions!, cancellationToken);
            var jsonResponse = JsonSerializer.Serialize(commandResponse, ModelsJsonContext.Default.CommandResponse);
            var isError = commandResponse.Status < HttpStatusCode.OK || commandResponse.Status >= HttpStatusCode.Ambiguous;

            return new CallToolResult
            {
                Content = [
                    new TextContentBlock
                    {
                        Text = jsonResponse
                    }
                ],
                IsError = isError,
                Meta = new([new(McpHelper.ToolIdMetaKey, command.Id)])
            };
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

    private bool IsCommandGroupTool(string? toolName) =>
        !string.IsNullOrWhiteSpace(toolName) &&
        _commandGroups.Value.Any(group => string.Equals(group.Name, toolName, StringComparison.OrdinalIgnoreCase));

    internal bool ContainsCommandGroup(string toolName) => IsCommandGroupTool(toolName);

    internal IList<Tool> GetCommandGroupTools() => ListCommandGroups().Tools;

    private static CallToolResult ToolNotFound(string? toolName) => new()
    {
        Content = [new TextContentBlock { Text = $"Could not find command: {toolName}" }],
        IsError = true,
    };

    /// <summary>
    /// Converts a command to an MCP tool definition.
    /// </summary>
    /// <param name="fullName">The full name of the command.</param>
    /// <param name="command">The command to convert.</param>
    /// <returns>An MCP tool definition.</returns>
    private static Tool GetTool(string fullName, IBaseCommand command)
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

        var options = command.GetCommand().Options
            .Where(o => !CommandFactory.IsLearnOption(o))
            .ToList();

        if (options.Count == 1 && IsRawMcpToolInputOption(options[0]))
        {
            var arguments = JsonNode.Parse(options[0].Description ?? "{}") as JsonObject ?? [];
            tool.InputSchema = JsonSerializer.SerializeToElement(arguments, ServerJsonContext.Default.JsonObject);
            return tool;
        }

        var schema = OptionSchemaGenerator.CreateInputSchema(options);
        tool.InputSchema = JsonSerializer.SerializeToElement(schema, ServerJsonContext.Default.JsonObject);

        return tool;
    }

    /// <summary>
    /// Disposes resources owned by this tool loader.
    /// CommandFactoryToolLoader doesn't own external resources that need disposal.
    /// </summary>
    protected override ValueTask DisposeAsyncCore()
    {
        _cachedCommandGroupTools.Clear();
        _cachedListToolsResult = null;
        return ValueTask.CompletedTask;
    }
}
