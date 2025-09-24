// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Mcp.Core.Areas.Server.Commands.ToolLoading.Filters;
using Azure.Mcp.Core.Areas.Server.Models;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Helpers;
using Azure.Mcp.Core.Services.Telemetry;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;

namespace Azure.Mcp.Core.Areas.Server.Commands.ToolLoading;

/// <summary>
/// A configurable tool loader that uses a filter chain to determine which commands
/// should be exposed as MCP tools. Replaces the previous approach of using three
/// separate loaders with a single, composable filter-based architecture.
/// </summary>
/// <param name="serviceProvider">The service provider for resolving dependencies.</param>
/// <param name="commandFactory">The command factory containing all available commands.</param>
/// <param name="filters">The list of filters to apply when loading tools.</param>
/// <param name="logger">The logger for diagnostic information.</param>
public sealed class ConfigurableToolLoader(
    IServiceProvider serviceProvider,
    CommandFactory commandFactory,
    IList<ICommandFilter> filters,
    ILogger<ConfigurableToolLoader> logger) : IToolLoader
{
    private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    private readonly CommandFactory _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
    private readonly IList<ICommandFilter> _filters = filters ?? throw new ArgumentNullException(nameof(filters));
    private readonly ILogger<ConfigurableToolLoader> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly Lazy<IReadOnlyDictionary<string, IBaseCommand>> _toolCommands = new(() => ApplyFilters(commandFactory, filters, logger));

    public const string RawMcpToolInputOptionName = "raw-mcp-tool-input";

    /// <summary>
    /// Handles requests to list all tools available in the MCP server.
    /// </summary>
    /// <param name="request">The request context containing metadata and parameters.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A result containing the list of available tools.</returns>
    public ValueTask<ListToolsResult> ListToolsHandler(RequestContext<ListToolsRequestParams> request, CancellationToken cancellationToken)
    {
        var tools = CommandFactory.GetVisibleCommands(_toolCommands.Value)
            .Select(kvp => GetTool(kvp.Key, kvp.Value))
            .ToList();

        var listToolsResult = new ListToolsResult { Tools = tools };

        _logger.LogInformation("Listing {NumberOfTools} tools using filter chain.", tools.Count);

        return ValueTask.FromResult(listToolsResult);
    }

    /// <summary>
    /// Handles tool calls by executing the corresponding command from the command factory.
    /// </summary>
    /// <param name="request">The request context containing parameters and metadata.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The result of the tool call operation.</returns>
    public async ValueTask<CallToolResult> CallToolHandler(RequestContext<CallToolRequestParams> request, CancellationToken cancellationToken)
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

        var toolName = request.Params.Name;
        var command = _toolCommands.Value.GetValueOrDefault(toolName);
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

        var commandContext = new CommandContext(_serviceProvider, Activity.Current);
        var realCommand = command.GetCommand();
        ParseResult? commandOptions = null;

        if (realCommand.Options.Count == 1 && IsRawMcpToolInputOption(realCommand.Options[0]))
        {
            commandOptions = realCommand.ParseFromRawMcpToolInput(request.Params.Arguments);
        }
        else
        {
            commandOptions = realCommand.ParseFromDictionary(request.Params.Arguments);
        }

        _logger.LogTrace("Invoking '{Tool}'.", realCommand.Name);

        if (commandContext.Activity != null)
        {
            var serviceArea = _commandFactory.GetServiceArea(toolName);
            commandContext.Activity.AddTag(TelemetryConstants.TagName.ToolArea, serviceArea);
        }

        try
        {
            var commandResponse = await command.ExecuteAsync(commandContext, commandOptions);
            var jsonResponse = JsonSerializer.Serialize(commandResponse, ModelsJsonContext.Default.CommandResponse);
            var isError = commandResponse.Status < HttpStatusCode.OK || commandResponse.Status >= HttpStatusCode.Ambiguous;

            var callToolResult = new CallToolResult
            {
                Content = [new TextContentBlock { Text = jsonResponse }],
                IsError = isError,
            };

            _logger.LogTrace("Tool '{Tool}' executed successfully.", realCommand.Name);
            return callToolResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing tool: {ToolName}", toolName);

            var content = new TextContentBlock
            {
                Text = $"Error executing tool '{toolName}': {ex.Message}",
            };

            return new CallToolResult
            {
                Content = [content],
                IsError = true,
            };
        }
    }

    /// <summary>
    /// Disposes of the tool loader resources.
    /// </summary>
    public ValueTask DisposeAsync()
    {
        // No resources to dispose currently
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Gets the configured filters for diagnostics and testing purposes.
    /// </summary>
    public IReadOnlyList<ICommandFilter> Filters => _filters.ToList().AsReadOnly();

    /// <summary>
    /// Gets the command factory being used.
    /// </summary>
    public CommandFactory CommandFactory => _commandFactory;

    /// <summary>
    /// Applies the configured filter chain to determine which commands should be available as tools.
    /// </summary>
    private static IReadOnlyDictionary<string, IBaseCommand> ApplyFilters(
        CommandFactory commandFactory,
        IList<ICommandFilter> filters,
        ILogger<ConfigurableToolLoader> logger)
    {
        try
        {
            logger.LogDebug("Applying {FilterCount} filters to command set", filters.Count);

            // Get all available commands from the factory
            var allCommands = commandFactory.AllCommands;
            logger.LogDebug("Found {CommandCount} total commands", allCommands.Count);

            // Apply filters in priority order
            var orderedFilters = filters.OrderBy(f => f.Priority).ToList();
            logger.LogDebug("Applying filters in order: {FilterNames}",
                string.Join(", ", orderedFilters.Select(f => $"{f.Name}({f.Priority})")));

            var filteredCommands = new Dictionary<string, IBaseCommand>();

            foreach (var kvp in allCommands)
            {
                var commandName = kvp.Key;
                var command = kvp.Value;

                // Apply all filters - command must pass ALL filters to be included
                var shouldInclude = orderedFilters.All(filter =>
                {
                    var result = filter.ShouldIncludeCommand(commandName, command);
                    logger.LogTrace("Filter {FilterName}: {CommandName} -> {Result}",
                        filter.Name, commandName, result);
                    return result;
                });

                if (shouldInclude)
                {
                    filteredCommands[commandName] = command;
                }
            }

            logger.LogInformation("Filter chain produced {FilteredCount} commands from {TotalCount} total commands",
                filteredCommands.Count, allCommands.Count);

            return filteredCommands.AsReadOnly();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error applying filter chain");
            throw;
        }
    }

    /// <summary>
    /// Converts a command to an MCP tool with proper metadata.
    /// </summary>
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

        // Add Secret metadata to tool.Meta if the property exists
        if (metadata.Secret)
        {
            tool.Meta = new JsonObject
            {
                ["SecretHint"] = metadata.Secret
            };
        }

        var options = command.GetCommand().Options;

        var schema = new ToolInputSchema();

        if (options != null && options.Count > 0)
        {
            if (options.Count == 1 && IsRawMcpToolInputOption(options[0]))
            {
                var arguments = JsonNode.Parse(options[0].Description ?? "{}") as JsonObject ?? new JsonObject();
                tool.InputSchema = JsonSerializer.SerializeToElement(arguments, ServerJsonContext.Default.JsonObject);
                return tool;
            }
            else
            {
                foreach (var option in options)
                {
                    // Use the CreatePropertySchema method to properly handle array types with items
                    var propName = NameNormalization.NormalizeOptionName(option.Name);
                    schema.Properties.Add(propName, TypeToJsonTypeMapper.CreatePropertySchema(option.ValueType, option.Description));
                }

                schema.Required = [.. options.Where(p => p.Required).Select(p => NameNormalization.NormalizeOptionName(p.Name))];
            }
        }

        tool.InputSchema = JsonSerializer.SerializeToElement(schema, ServerJsonContext.Default.ToolInputSchema);

        return tool;
    }

    private static bool IsRawMcpToolInputOption(Option option)
    {
        if (string.Equals(NameNormalization.NormalizeOptionName(option.Name), RawMcpToolInputOptionName, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        foreach (var alias in option.Aliases)
        {
            if (string.Equals(NameNormalization.NormalizeOptionName(alias), RawMcpToolInputOptionName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
