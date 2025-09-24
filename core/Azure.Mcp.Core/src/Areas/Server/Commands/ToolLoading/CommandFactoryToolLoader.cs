// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Text.Json.Nodes;
using Azure.Mcp.Core.Areas.Server.Models;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Helpers;
using Azure.Mcp.Core.Models.Elicitation;
using Azure.Mcp.Core.Services.Telemetry;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Protocol;

namespace Azure.Mcp.Core.Areas.Server.Commands.ToolLoading;

/// <summary>
/// A configurable tool loader that uses command attributes to determine which commands
/// should be exposed as MCP tools. Uses attributes (Hidden, ReadOnly, Essential, Extension)
/// for declarative configuration.
/// </summary>
/// <param name="serviceProvider">The service provider for resolving dependencies.</param>
/// <param name="commandFactory">The command factory containing all available commands.</param>
/// <param name="options">The tool loader options for configuration.</param>
/// <param name="logger">The logger for diagnostic information.</param>
public sealed class CommandFactoryToolLoader(
    IServiceProvider serviceProvider,
    CommandFactory commandFactory,
    IOptions<ToolLoaderOptions> options,
    ILogger<CommandFactoryToolLoader> logger) : IToolLoader
{
    private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    private readonly CommandFactory _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
    private readonly IOptions<ToolLoaderOptions> _options = options ?? throw new ArgumentNullException(nameof(options));
    private readonly ILogger<CommandFactoryToolLoader> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private IReadOnlyDictionary<string, IBaseCommand>? _toolCommands;

    public const string RawMcpToolInputOptionName = "raw-mcp-tool-input";

    /// <summary>
    /// Handles requests to list all tools available in the MCP server.
    /// </summary>
    /// <param name="request">The request context containing metadata and parameters.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A result containing the list of available tools.</returns>
    public ValueTask<ListToolsResult> ListToolsHandler(RequestContext<ListToolsRequestParams> request, CancellationToken cancellationToken)
    {
        var filteredCommands = GetFilteredCommands();
        var tools = CommandFactory.GetVisibleCommands(filteredCommands)
            .Select(kvp => GetTool(kvp.Key, kvp.Value))
            .ToList();

        var listToolsResult = new ListToolsResult { Tools = tools };

        _logger.LogInformation("Listing {NumberOfTools} tools using attribute filtering.", tools.Count);

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
        var filteredCommands = GetFilteredCommands();
        var command = filteredCommands.GetValueOrDefault(toolName);
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

        // Check if this tool requires elicitation for sensitive data
        var metadata = command.Metadata;
        if (metadata.Secret)
        {
            // Check if elicitation is disabled by insecure option
            if (_options.Value.InsecureDisableElicitation)
            {
                _logger.LogWarning("Tool '{Tool}' handles sensitive data but elicitation is disabled via --insecure-disable-elicitation. Proceeding without user consent (INSECURE).", toolName);
            }
            else
            {
                // If client doesn't support elicitation, treat as rejected and don't execute
                if (!request.Server.SupportsElicitation())
                {
                    _logger.LogWarning("Tool '{Tool}' handles sensitive data but client does not support elicitation. Operation rejected.", toolName);
                    return new CallToolResult
                    {
                        Content = [new TextContentBlock { Text = "This tool handles sensitive data and requires user consent, but the client does not support elicitation. Operation rejected for security." }],
                        IsError = true
                    };
                }

                try
                {
                    _logger.LogInformation("Tool '{Tool}' handles sensitive data. Requesting user confirmation via elicitation.", toolName);

                    // Create the elicitation request using our custom model
                    var elicitationRequest = new ElicitationRequestParams
                    {
                        Message = $"⚠️ SECURITY WARNING: The tool '{toolName}' may expose secrets or sensitive information.\n\nThis operation could reveal confidential data such as passwords, API keys, certificates, or other sensitive values.\n\nDo you want to continue with this potentially sensitive operation?",
                        RequestedSchema = ElicitationSchema.CreateSecretSchema("confirmation", "Confirm Action", "Type 'yes' to confirm you want to proceed with this sensitive operation", true)
                    };

                    // Use our extension method to handle the elicitation
                    var elicitationResponse = await request.Server.RequestElicitationAsync(elicitationRequest, cancellationToken);

                    if (elicitationResponse.Action != ElicitationAction.Accept)
                    {
                        _logger.LogInformation("User {Action} the elicitation for tool '{Tool}'. Operation not executed.",
                            elicitationResponse.Action.ToString().ToLower(), toolName);
                        return new CallToolResult
                        {
                            Content = [new TextContentBlock { Text = $"Operation cancelled by user ({elicitationResponse.Action.ToString().ToLower()})." }],
                            IsError = true
                        };
                    }

                    _logger.LogInformation("User accepted elicitation for tool '{Tool}'. Proceeding with execution.", toolName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during elicitation for tool '{Tool}': {Error}", toolName, ex.Message);
                    return new CallToolResult
                    {
                        Content = [new TextContentBlock { Text = $"Elicitation failed for sensitive tool '{toolName}': {ex.Message}. Operation not executed for security." }],
                        IsError = true
                    };
                }
            }
        }

        var commandContext = new CommandContext(_serviceProvider, Activity.Current);
        var realCommand = command.GetCommand();
        ParseResult? commandOptions = null;

        if (realCommand.Options.Count == 1 && IsRawMcpToolInputOption(realCommand.Options[0]))
        {
            commandOptions = realCommand.ParseFromRawMcpToolInput(request.Params!.Arguments);
        }
        else
        {
            commandOptions = realCommand.ParseFromDictionary(request.Params!.Arguments);
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
    /// Disposes of the tool loader resources.
    /// </summary>
    public ValueTask DisposeAsync()
    {
        // No resources to dispose currently
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Gets the command factory being used.
    /// </summary>
    public CommandFactory CommandFactory => _commandFactory;

    /// <summary>
    /// Gets the filtered commands based on attributes, building them on first access.
    /// </summary>
    private IReadOnlyDictionary<string, IBaseCommand> GetFilteredCommands()
    {
        if (_toolCommands != null)
        {
            return _toolCommands;
        }

        try
        {
            // Always start with ALL commands, then filter based on attributes and namespace logic
            // Essential commands should always be considered regardless of namespace filtering
            var baseCommands = _commandFactory.AllCommands;

            _logger.LogDebug("Filtering {CommandCount} commands using attributes and namespace logic", baseCommands.Count);

            var filteredCommands = new Dictionary<string, IBaseCommand>();

            foreach (var kvp in baseCommands)
            {
                var commandName = kvp.Key;
                var command = kvp.Value;

                if (ShouldIncludeCommand(commandName, command))
                {
                    filteredCommands[commandName] = command;
                }
            }

            _logger.LogInformation("Attribute filtering produced {FilteredCount} commands from {TotalCount} total commands",
                filteredCommands.Count, baseCommands.Count);

            _toolCommands = filteredCommands.AsReadOnly();
            return _toolCommands;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error filtering commands by attributes");
            throw;
        }
    }

    /// <summary>
    /// Determines if a command should be included based on its attributes and current configuration.
    /// </summary>
    private bool ShouldIncludeCommand(string commandName, IBaseCommand command)
    {
        if (string.IsNullOrWhiteSpace(commandName) || command == null)
        {
            return false;
        }

        var commandType = command.GetType();

        // Always include essential commands (check both class and base class)
        if (commandType.GetCustomAttribute<EssentialAttribute>() != null ||
            commandType.BaseType?.GetCustomAttribute<EssentialAttribute>() != null)
        {
            _logger.LogTrace("Including essential command: {CommandName}", commandName);
            return ShouldIncludeBasedOnReadOnly(command);
        }

        // Handle extension commands
        if (commandType.GetCustomAttribute<ExtensionAttribute>() != null)
        {
            var shouldIncludeExtensions = ShouldIncludeExtensions();
            if (!shouldIncludeExtensions)
            {
                _logger.LogTrace("Excluding extension command: {CommandName}", commandName);
                return false;
            }
            _logger.LogTrace("Including extension command: {CommandName}", commandName);
            return ShouldIncludeBasedOnReadOnly(command);
        }

        // For regular service commands, include them based on namespace filtering and ReadOnly mode
        if (ShouldIncludeBasedOnNamespace(commandName))
        {
            _logger.LogTrace("Including service command: {CommandName}", commandName);
            return ShouldIncludeBasedOnReadOnly(command);
        }
        else
        {
            _logger.LogTrace("Excluding service command due to namespace filtering: {CommandName}", commandName);
            return false;
        }
    }

    /// <summary>
    /// Determines if extension commands should be included based on configuration.
    /// </summary>
    private bool ShouldIncludeExtensions()
    {
        // Include extensions if:
        // 1. No namespace specified (all mode)
        // 2. "extension" namespace explicitly requested
        return _options.Value.Namespace == null ||
               _options.Value.Namespace.Contains("extension", StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines if a command should be included based on namespace filtering.
    /// </summary>
    private bool ShouldIncludeBasedOnNamespace(string commandName)
    {
        // If no namespace filtering is applied, include all commands
        if (_options.Value.Namespace == null || _options.Value.Namespace.Length == 0)
        {
            return true;
        }

        // Check if the command belongs to any of the specified namespaces
        // Command names are in format: azmcp_{namespace}_{resource}_{action}
        foreach (var nameSpace in _options.Value.Namespace)
        {
            if (commandName.Contains($"_{nameSpace}_", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Applies ReadOnly mode filtering if enabled.
    /// </summary>
    private bool ShouldIncludeBasedOnReadOnly(IBaseCommand command)
    {
        if (!_options.Value.ReadOnly)
        {
            return true; // Not in ReadOnly mode, include all commands
        }

        // In ReadOnly mode, only include commands marked as ReadOnly
        return command.Metadata.ReadOnly;
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
