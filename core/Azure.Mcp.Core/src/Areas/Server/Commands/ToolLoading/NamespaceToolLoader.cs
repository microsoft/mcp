// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Text.Json.Nodes;
using Azure.Mcp.Core.Areas.Server.Models;
using Azure.Mcp.Core.Areas.Server.Options;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;

namespace Azure.Mcp.Core.Areas.Server.Commands.ToolLoading;

/// <summary>
/// A tool loader that exposes Azure command groups as hierarchical namespace tools with direct in-process tool execution.
/// Provides the same functionality as <see cref="ServerToolLoader"/> but without spawning child azmcp processes.
/// Supports learn functionality for progressive discovery of commands within each namespace.
/// </summary>
public sealed class NamespaceToolLoader : BaseToolLoader
{
    private readonly CommandFactory _commandFactory;
    private readonly IOptions<ServiceStartOptions> _options;
    private readonly IServiceProvider _serviceProvider;
    private static readonly List<string> IgnoreCommandGroups = ["extension", "server", "tools"];

    private readonly Lazy<List<Tool>> _cachedNamespaceTools;
    private readonly IReadOnlyList<string> _namespaceNames;
    private readonly ConcurrentDictionary<string, IReadOnlyDictionary<string, IBaseCommand>> _commandsByNamespace = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, List<Tool>> _cachedLearnToolsByNamespace = new(StringComparer.OrdinalIgnoreCase);

    private const string ToolCallProxySchema = """
        {
          "type": "object",
          "properties": {
            "tool": {
              "type": "string",
              "description": "The name of the tool to call."
            },
            "parameters": {
              "type": "object",
              "description": "A key/value pair of parameters names and values to pass to the tool call command."
            }
          },
          "additionalProperties": false
        }
        """;

    private static readonly JsonElement ToolSchema = JsonSerializer.Deserialize("""
        {
            "type": "object",
            "properties": {
            "intent": {
                "type": "string",
                "description": "The intent of the azure operation to perform."
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

    public NamespaceToolLoader(
        CommandFactory commandFactory,
        IOptions<ServiceStartOptions> options,
        IServiceProvider serviceProvider,
        ILogger<NamespaceToolLoader> logger) : base(logger)
    {
        _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

        _namespaceNames = GetFilteredNamespaceNames();
        _cachedNamespaceTools = new Lazy<List<Tool>>(() =>
            _namespaceNames
                .Select(ns => CreateNamespaceTool(ns))
                .ToList());
    }

    public override ValueTask<ListToolsResult> ListToolsHandler(
        RequestContext<ListToolsRequestParams> request,
        CancellationToken cancellationToken)
    {
        var tools = _cachedNamespaceTools.Value;
        return ValueTask.FromResult(new ListToolsResult { Tools = tools });
    }

    /// <summary>
    /// Handles tool calls for namespace tools. Supports both learn mode (discovery) and
    /// command execution mode (direct command invocation).
    /// </summary>
    public override async ValueTask<CallToolResult> CallToolHandler(
        RequestContext<CallToolRequestParams> request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Params?.Name))
        {
            throw new ArgumentNullException(nameof(request.Params.Name), "Tool name cannot be null or empty.");
        }

        var toolName = request.Params.Name;

        // Validate namespace exists
        if (!_namespaceNames.Contains(toolName, StringComparer.OrdinalIgnoreCase))
        {
            return new CallToolResult
            {
                Content = [new TextContentBlock
                {
                    Text = $"Namespace '{toolName}' not found. Available namespaces: {string.Join(", ", _namespaceNames)}"
                }],
                IsError = true
            };
        }

        var args = request.Params.Arguments;
        var (intent, command, parameters, learn) = ParseHierarchicalCall(args);
        if (!learn && !string.IsNullOrEmpty(intent) && string.IsNullOrEmpty(command))
        {
            // Auto-learn if intent provided but no command specified
            learn = true;
        }

        try
        {
            if (learn && string.IsNullOrEmpty(command))
            {
                // Learn mode: Return available commands for this namespace
                return await HandleLearnRequest(request, intent ?? "", toolName, cancellationToken);
            }
            else if (!string.IsNullOrEmpty(toolName) && !string.IsNullOrEmpty(command))
            {
                // Execution mode: Execute the specified command
                return await ExecuteNamespaceCommand(request, intent ?? "", toolName, command, parameters, cancellationToken);
            }
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogError(ex, "Key not found while calling namespace tool: {Tool}", toolName);

            return new CallToolResult
            {
                Content = [new TextContentBlock
                {
                    Text = $"""
                        The tool '{toolName}.{command}' was not found or does not support the specified command.
                        Please ensure the tool name and command are correct.
                        If you want to learn about available tools, run again with the "learn=true" argument.
                        """
                }],
                IsError = true
            };
        }

        return new CallToolResult
        {
            Content = [new TextContentBlock
            {
                Text = """
                    The "command" parameter is required when not learning.
                    Run again with the "learn" argument to get a list of available tools and their parameters.
                    To learn about a specific tool, use the "tool" argument with the name of the tool.
                    """
            }],
            IsError = false
        };
    }

    /// <summary>
    /// Handles learn requests for a namespace, returning available commands with their schemas.
    /// Uses caching to avoid rebuilding tool definitions on repeated requests.
    /// </summary>
    private async Task<CallToolResult> HandleLearnRequest(
        RequestContext<CallToolRequestParams> request,
        string intent,
        string nameSpace,
        CancellationToken cancellationToken)
    {
        if (_cachedLearnToolsByNamespace.TryGetValue(nameSpace, out var cachedTools))
        {
            var cachedJson = JsonSerializer.Serialize(cachedTools, ServerJsonContext.Default.ListTool);
            return CreateLearnResponse(nameSpace, cachedJson);
        }

        // Build tools for this namespace (lazy load if not cached)
        var namespaceCommands = GetOrLoadNamespaceCommands(nameSpace);
        var tools = namespaceCommands
            .Where(kvp => !(_options.Value.ReadOnly ?? false) || kvp.Value.Metadata.ReadOnly)
            .Select(kvp => CreateToolFromCommand(kvp.Key, kvp.Value))
            .ToList();

        // Cache tools for future learn requests
        _cachedLearnToolsByNamespace[nameSpace] = tools;

        // If client supports sampling and intent is provided, try to infer command
        if (SupportsSampling(request.Server) && !string.IsNullOrWhiteSpace(intent))
        {
            var (commandName, parameters) = await GetCommandAndParametersFromIntentAsync(
                request, intent, nameSpace, tools, cancellationToken);

            if (commandName != null)
            {
                return await ExecuteNamespaceCommand(request, intent, nameSpace, commandName, parameters, cancellationToken);
            }
        }

        var toolsJson = JsonSerializer.Serialize(tools, ServerJsonContext.Default.ListTool);
        return CreateLearnResponse(nameSpace, toolsJson);
    }

    /// <summary>
    /// Executes a command within a namespace.
    /// </summary>
    private async Task<CallToolResult> ExecuteNamespaceCommand(
        RequestContext<CallToolRequestParams> request,
        string intent,
        string nameSpace,
        string command,
        IReadOnlyDictionary<string, JsonElement> parameters,
        CancellationToken cancellationToken)
    {
        var namespaceCommands = GetOrLoadNamespaceCommands(nameSpace);

        // Try to find the command - handle both "command" and "namespace command" formats
        if (!namespaceCommands.TryGetValue(command, out var cmd))
        {
            var fullCommandName = $"{nameSpace} {command}";
            if (!namespaceCommands.TryGetValue(fullCommandName, out cmd))
            {
                _logger.LogWarning("Namespace {Namespace} does not have a command {Command}.", nameSpace, command);

                if (string.IsNullOrWhiteSpace(intent))
                {
                    return await HandleLearnRequest(request, intent, nameSpace, cancellationToken);
                }

                var tools = _cachedLearnToolsByNamespace.GetValueOrDefault(nameSpace)
                    ?? GetToolsForNamespace(nameSpace);

                var samplingResult = await GetCommandAndParametersFromIntentAsync(
                    request, intent, nameSpace, tools, cancellationToken);

                if (string.IsNullOrWhiteSpace(samplingResult.commandName))
                {
                    return await HandleLearnRequest(request, intent, nameSpace, cancellationToken);
                }

                command = samplingResult.commandName;
                parameters = samplingResult.parameters;

                if (!namespaceCommands.TryGetValue(command, out cmd))
                {
                    return await HandleLearnRequest(request, intent, nameSpace, cancellationToken);
                }
            }
        }

        try
        {
            await NotifyProgressAsync(request, $"Calling {nameSpace} {command}...", cancellationToken);

            // Direct execution (same as CommandFactoryToolLoader)
            var commandContext = new CommandContext(_serviceProvider, Activity.Current);
            var realCommand = cmd.GetCommand();

            ParseResult commandOptions;
            if (realCommand.Options.Count == 1 && IsRawMcpToolInputOption(realCommand.Options[0]))
            {
                commandOptions = realCommand.ParseFromRawMcpToolInput(parameters);
            }
            else
            {
                commandOptions = realCommand.ParseFromDictionary(parameters);
            }

            _logger.LogTrace("Executing namespace command '{Namespace} {Command}'", nameSpace, command);

            var commandResponse = await cmd.ExecuteAsync(commandContext, commandOptions);

            // Check if command requires missing parameters
            var jsonResponse = JsonSerializer.Serialize(commandResponse, ModelsJsonContext.Default.CommandResponse);
            var isError = commandResponse.Status < HttpStatusCode.OK || commandResponse.Status >= HttpStatusCode.Ambiguous;

            if (jsonResponse.Contains("Missing required options", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Namespace command '{Namespace} {Command}' requires additional parameters.", nameSpace, command);

                var commandTool = GetCommandTool(nameSpace, command);
                var commandToolJson = JsonSerializer.Serialize(commandTool, ServerJsonContext.Default.Tool);

                return new CallToolResult
                {
                    Content =
                    [
                        new TextContentBlock
                        {
                            Text = $"""
                                The '{command}' command is missing required parameters.

                                - Review the following command spec and identify the required arguments from the input schema.
                                - Omit any arguments that are not required or do not apply to your use case.
                                - Wrap all command arguments into the root "parameters" argument.
                                - If required data is missing infer the data from your context or prompt the user as needed.
                                - Run the tool again with the "command" and root "parameters" object.

                                Command Spec:
                                {commandToolJson}

                                Original Error:
                                {jsonResponse}
                                """
                        }
                    ],
                    IsError = true
                };
            }

            return new CallToolResult
            {
                Content = [new TextContentBlock { Text = jsonResponse }],
                IsError = isError
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception thrown while calling namespace tool: {Namespace}, command: {Command}", nameSpace, command);
            return new CallToolResult
            {
                Content = [new TextContentBlock
                {
                    Text = $"""
                        There was an error finding or calling tool and command.
                        Failed to call namespace: {nameSpace}, command: {command}
                        Error: {ex.Message}

                        Run again with the "learn=true" to get a list of available commands and their parameters.
                        """
                }],
                IsError = true
            };
        }
    }

    /// <summary>
    /// Gets or lazily loads commands for a specific namespace.
    /// Commands are only loaded when first accessed, improving startup time and memory usage.
    /// </summary>
    private IReadOnlyDictionary<string, IBaseCommand> GetOrLoadNamespaceCommands(string nameSpace)
    {
        return _commandsByNamespace.GetOrAdd(nameSpace, ns => _commandFactory.GroupCommands([ns]));
    }

    /// <summary>
    /// Gets filtered namespace names.
    /// </summary>
    private IReadOnlyList<string> GetFilteredNamespaceNames()
    {
        return _commandFactory.RootGroup.SubGroup
            .Where(group => !IgnoreCommandGroups.Contains(group.Name, StringComparer.OrdinalIgnoreCase))
            .Where(group => _options.Value.Namespace == null ||
                           _options.Value.Namespace.Length == 0 ||
                           _options.Value.Namespace.Contains(group.Name, StringComparer.OrdinalIgnoreCase))
            .Select(group => group.Name)
            .ToList();
    }

    /// <summary>
    /// Creates a hierarchical namespace tool with learn capabilities.
    /// </summary>
    private Tool CreateNamespaceTool(string nameSpace)
    {
        var group = _commandFactory.RootGroup.SubGroup
            .First(g => string.Equals(g.Name, nameSpace, StringComparison.OrdinalIgnoreCase));
        var description = group.Description;

        return new Tool
        {
            Name = nameSpace,
            Description = description + """
                This tool is a hierarchical MCP command router.
                Sub commands are routed to MCP servers that require specific fields inside the "parameters" object.
                To invoke a command, set "command" and wrap its args in "parameters".
                Set "learn=true" to discover available sub commands.
                """,
            InputSchema = ToolSchema
        };
    }

    /// <summary>
    /// Creates a tool definition from a command (same logic as CommandFactoryToolLoader).
    /// </summary>
    private static Tool CreateToolFromCommand(string fullName, IBaseCommand command)
    {
        var underlyingCommand = command.GetCommand();
        var tool = new Tool
        {
            Name = fullName,
            Description = underlyingCommand.Description,
        };

        var metadata = command.Metadata;
        tool.Annotations = new ToolAnnotations()
        {
            DestructiveHint = metadata.Destructive,
            IdempotentHint = metadata.Idempotent,
            OpenWorldHint = metadata.OpenWorld,
            ReadOnlyHint = metadata.ReadOnly,
            Title = command.Title,
        };

        if (metadata.Secret)
        {
            tool.Meta = new JsonObject { ["SecretHint"] = metadata.Secret };
        }

        var schema = new ToolInputSchema();
        var options = command.GetCommand().Options;

        if (options?.Count > 0)
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
    /// Parses hierarchical call structure from MCP tool arguments.
    /// </summary>
    private static (string? intent, string? command, IReadOnlyDictionary<string, JsonElement> parameters, bool learn) ParseHierarchicalCall(
        IReadOnlyDictionary<string, JsonElement>? args)
    {
        if (args == null)
        {
            return (null, null, new Dictionary<string, JsonElement>(), false);
        }

        string? intent = null;
        string? command = null;
        bool learn = false;
        IReadOnlyDictionary<string, JsonElement> parameters = new Dictionary<string, JsonElement>();

        if (args.TryGetValue("intent", out var intentElem) && intentElem.ValueKind == JsonValueKind.String)
        {
            intent = intentElem.GetString();
        }

        if (args.TryGetValue("learn", out var learnElem) && learnElem.ValueKind == JsonValueKind.True)
        {
            learn = true;
        }

        if (args.TryGetValue("command", out var commandElem) && commandElem.ValueKind == JsonValueKind.String)
        {
            command = commandElem.GetString();
        }

        if (args.TryGetValue("parameters", out var paramsElem) && paramsElem.ValueKind == JsonValueKind.Object)
        {
            parameters = paramsElem.EnumerateObject()
                .ToDictionary(prop => prop.Name, prop => prop.Value);
        }

        return (intent, command, parameters, learn);
    }

    private static bool IsRawMcpToolInputOption(Option option)
    {
        const string RawMcpToolInputOptionName = "raw-mcp-tool-input";
        if (string.Equals(NameNormalization.NormalizeOptionName(option.Name), RawMcpToolInputOptionName, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return option.Aliases.Any(alias =>
            string.Equals(NameNormalization.NormalizeOptionName(alias), RawMcpToolInputOptionName, StringComparison.OrdinalIgnoreCase));
    }

    private CallToolResult CreateLearnResponse(string nameSpace, string toolsJson)
    {
        return new CallToolResult
        {
            Content = [new TextContentBlock
            {
                Text = $"""
                    Here are the available command and their parameters for '{nameSpace}' tool.
                    If you do not find a suitable command, run again with the "learn=true" to get a list of available commands and their parameters.
                    Next, identify the command you want to execute and run again with the "command" and "parameters" arguments.

                    {toolsJson}
                    """
            }],
            IsError = false
        };
    }

    private List<Tool> GetToolsForNamespace(string nameSpace)
    {
        var namespaceCommands = GetOrLoadNamespaceCommands(nameSpace);
        return namespaceCommands
            .Where(kvp => !(_options.Value.ReadOnly ?? false) || kvp.Value.Metadata.ReadOnly)
            .Select(kvp => CreateToolFromCommand(kvp.Key, kvp.Value))
            .ToList();
    }

    private Tool GetCommandTool(string nameSpace, string commandName)
    {
        var tools = _cachedLearnToolsByNamespace.GetValueOrDefault(nameSpace)
            ?? GetToolsForNamespace(nameSpace);

        return tools.First(t => string.Equals(t.Name, commandName, StringComparison.OrdinalIgnoreCase));
    }

    private static bool SupportsSampling(McpServer server)
    {
        return server?.ClientCapabilities?.Sampling != null;
    }

    private static async Task NotifyProgressAsync(RequestContext<CallToolRequestParams> request, string message, CancellationToken cancellationToken)
    {
        var progressToken = request.Params?.ProgressToken;
        if (progressToken == null)
        {
            return;
        }

        await request.Server.NotifyProgressAsync(progressToken.Value,
            new ProgressNotificationValue
            {
                Progress = 0f,
                Message = message,
            }, cancellationToken);
    }

    private async Task<(string? commandName, IReadOnlyDictionary<string, JsonElement> parameters)> GetCommandAndParametersFromIntentAsync(
        RequestContext<CallToolRequestParams> request,
        string intent,
        string nameSpace,
        List<Tool> availableTools,
        CancellationToken cancellationToken)
    {
        await NotifyProgressAsync(request, $"Learning about {nameSpace} capabilities...", cancellationToken);

        JsonElement toolParams = GetParametersJsonElement(request);
        var toolParamsJson = toolParams.GetRawText();
        var availableToolsJson = JsonSerializer.Serialize(availableTools, ServerJsonContext.Default.ListTool);

        var samplingRequest = new CreateMessageRequestParams
        {
            Messages = [
                new SamplingMessage
                {
                    Role = Role.Assistant,
                    Content = new TextContentBlock
                    {
                        Text = $"""
                            This is a list of available commands for the {nameSpace} server.

                            Your task:
                            - Select the single command that best matches the user's intent.
                            - Return a valid JSON object that matches the provided result schema.
                            - Map the user's intent and known parameters to the command's input schema, ensuring parameter names and types match the schema exactly (no extra or missing parameters).
                            - Only include parameters that are defined in the selected command's input schema.
                            - Do not guess or invent parameters.
                            - If no command matches, return JSON schema with "Unknown" tool name.

                            Result Schema:
                            {ToolCallProxySchema}

                            Intent:
                            {intent ?? "No specific intent provided"}

                            Known Parameters:
                            {toolParamsJson}

                            Available Commands:
                            {availableToolsJson}
                            """
                    }
                }
            ],
        };

        try
        {
            var samplingResponse = await request.Server.SampleAsync(samplingRequest, cancellationToken);
            var samplingContent = samplingResponse.Content as TextContentBlock;
            var toolCallJson = samplingContent?.Text?.Trim();
            string? commandName = null;
            IReadOnlyDictionary<string, JsonElement> parameters = new Dictionary<string, JsonElement>();

            if (!string.IsNullOrEmpty(toolCallJson))
            {
                var doc = JsonDocument.Parse(toolCallJson);
                var root = doc.RootElement;
                if (root.TryGetProperty("tool", out var toolProp) && toolProp.ValueKind == JsonValueKind.String)
                {
                    commandName = toolProp.GetString();
                }
                if (root.TryGetProperty("parameters", out var parametersElem) && parametersElem.ValueKind == JsonValueKind.Object)
                {
                    parameters = parametersElem.EnumerateObject().ToDictionary(prop => prop.Name, prop => prop.Value) ?? new Dictionary<string, JsonElement>();
                }
            }

            if (commandName != null && commandName != "Unknown")
            {
                return (commandName, parameters);
            }
        }
        catch
        {
            _logger.LogError("Failed to get command and parameters from intent: {Intent} for namespace: {Namespace}", intent, nameSpace);
        }

        return (null, new Dictionary<string, JsonElement>());
    }

    /// <summary>
    /// Disposes resources owned by this tool loader.
    /// Clears the cached tool lists dictionary.
    /// </summary>
    protected override async ValueTask DisposeAsyncCore()
    {
        _cachedLearnToolsByNamespace.Clear();
        await ValueTask.CompletedTask;
    }
}
