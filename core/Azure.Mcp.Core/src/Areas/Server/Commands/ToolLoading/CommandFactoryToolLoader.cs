// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Net;
using System.Text.Json.Nodes;
using Azure.Mcp.Core.Areas.Server.Models;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Helpers;
using Azure.Mcp.Core.Models.Elicitation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Protocol;
using static Azure.Mcp.Core.Services.Telemetry.TelemetryConstants;

namespace Azure.Mcp.Core.Areas.Server.Commands.ToolLoading;

/// <summary>
/// A tool loader that creates MCP tools from the registered command factory.
/// Exposes AzureMcp commands as MCP tools that can be invoked through the MCP protocol.
/// </summary>
public sealed class CommandFactoryToolLoader(
    IServiceProvider serviceProvider,
    CommandFactory commandFactory,
    IOptions<ToolLoaderOptions> options,
    ILogger<CommandFactoryToolLoader> logger) : BaseToolLoader(logger)
{
    private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    private readonly CommandFactory _commandFactory = commandFactory;
    private readonly IOptions<ToolLoaderOptions> _options = options;
    private IReadOnlyDictionary<string, IBaseCommand> _toolCommands =
        (options.Value.Namespace == null || options.Value.Namespace.Length == 0)
            ? commandFactory.AllCommands
            : commandFactory.GroupCommands(options.Value.Namespace);

    public const string RawMcpToolInputOptionName = "raw-mcp-tool-input";

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

        var tools = visibleCommands
            .Select(kvp => GetTool(kvp.Key, kvp.Value))
            .Where(tool => !_options.Value.ReadOnly || (tool.Annotations?.ReadOnlyHint == true))
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

        var activity = Activity.Current;

        if (activity != null)
        {
            activity.SetTag(TagName.ToolName, toolName);
        }

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
        var commandContext = new CommandContext(_serviceProvider, activity);

        // Check if this tool requires elicitation for sensitive data
        var metadata = command.Metadata;
        if (metadata.Secret)
        {
            var elicitationResult = await HandleSecretElicitationAsync(
                request,
                toolName,
                _options.Value.InsecureDisableElicitation,
                _logger,
                cancellationToken);

            if (elicitationResult != null)
            {
                return elicitationResult;
            }
        }

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
            commandContext.Activity.SetTag(TagName.ToolArea, serviceArea);
        }

        try
        {
            var commandResponse = await command.ExecuteAsync(commandContext, commandOptions, CancellationToken.None);
            var jsonResponse = JsonSerializer.Serialize(commandResponse, ModelsJsonContext.Default.CommandResponse);
            var isError = commandResponse.Status < HttpStatusCode.OK || commandResponse.Status >= HttpStatusCode.Ambiguous;

            return new CallToolResult
            {
                Content = [
                    new TextContentBlock {
                        Text = jsonResponse
                    }
                ],
                IsError = isError
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
