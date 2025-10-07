// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Net;
using System.Text.Json.Nodes;
using Azure.Mcp.Core.Areas.Server.Commands.Discovery;
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
/// A tool loader that exposes Azure command groups as hierarchical namespace tools with direct in-process execution.
/// Provides the same functionality as <see cref="ServerToolLoader"/> but without spawning child azmcp processes.
/// Supports learn functionality for progressive discovery of commands within each namespace.
/// </summary>
public sealed class NamespaceToolLoader(
    CommandFactory commandFactory,
    IOptions<ServiceStartOptions> options,
    IServiceProvider serviceProvider,
    ILogger<NamespaceToolLoader> logger) : BaseToolLoader(logger)
{
    private readonly CommandFactory _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
    private readonly IOptions<ServiceStartOptions> _options = options ?? throw new ArgumentNullException(nameof(options));
    private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

    private readonly Lazy<IReadOnlyList<string>> _availableNamespaces = new Lazy<IReadOnlyList<string>>(() =>
    {
        return commandFactory.RootGroup.SubGroup
            .Where(group => !DiscoveryConstants.IgnoredCommandGroups.Contains(group.Name, StringComparer.OrdinalIgnoreCase))
            .Where(group => options.Value.Namespace == null ||
                           options.Value.Namespace.Length == 0 ||
                           options.Value.Namespace.Contains(group.Name, StringComparer.OrdinalIgnoreCase))
            .Select(group => group.Name)
            .ToList();
    });

    private readonly Dictionary<string, List<Tool>> _cachedToolLists = new(StringComparer.OrdinalIgnoreCase);
    private ListToolsResult? _cachedListToolsResult;

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

    public override ValueTask<ListToolsResult> ListToolsHandler(RequestContext<ListToolsRequestParams> request, CancellationToken cancellationToken)
    {
        if (_cachedListToolsResult != null)
        {
            return ValueTask.FromResult(_cachedListToolsResult);
        }

        var namespaces = _availableNamespaces.Value;
        var allToolsResponse = new ListToolsResult
        {
            Tools = new List<Tool>()
        };

        foreach (var namespaceName in namespaces)
        {
            var group = _commandFactory.RootGroup.SubGroup
                .First(g => string.Equals(g.Name, namespaceName, StringComparison.OrdinalIgnoreCase));

            var tool = new Tool
            {
                Name = namespaceName,
                Description = group.Description + """
                    This tool is a hierarchical MCP command router.
                    Sub commands are routed to MCP servers that require specific fields inside the "parameters" object.
                    To invoke a command, set "command" and wrap its args in "parameters".
                    Set "learn=true" to discover available sub commands.
                    """,
                InputSchema = ToolSchema,
            };

            allToolsResponse.Tools.Add(tool);
        }

        // Cache the result
        _cachedListToolsResult = allToolsResponse;
        return ValueTask.FromResult(allToolsResponse);
    }

    public override async ValueTask<CallToolResult> CallToolHandler(RequestContext<CallToolRequestParams> request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Params?.Name))
        {
            throw new ArgumentNullException(nameof(request.Params.Name), "Tool name cannot be null or empty.");
        }

        string tool = request.Params.Name;
        var args = request.Params?.Arguments;
        string? intent = null;
        string? command = null;
        bool learn = false;

        if (args != null)
        {
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
        }

        if (!learn && !string.IsNullOrEmpty(intent) && string.IsNullOrEmpty(command))
        {
            learn = true;
        }

        try
        {
            if (learn && string.IsNullOrEmpty(command))
            {
                return await InvokeToolLearn(request, intent ?? "", tool, cancellationToken);
            }
            else if (!string.IsNullOrEmpty(tool) && !string.IsNullOrEmpty(command))
            {
                var toolParams = GetParametersFromArgs(args);
                return await InvokeChildToolAsync(request, intent ?? "", tool, command, toolParams, cancellationToken);
            }
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogError(ex, "Key not found while calling tool: {Tool}", tool);

            return new CallToolResult
            {
                Content =
                [
                    new TextContentBlock {
                        Text = $"""
                            The tool '{tool}.{command}' was not found or does not support the specified command.
                            Please ensure the tool name and command are correct.
                            If you want to learn about available tools, run again with the "learn=true" argument.
                        """
                    }
                ],
                IsError = true
            };
        }

        return new CallToolResult
        {
            Content =
                [
                    new TextContentBlock {
                    Text = """
                        The "command" parameters are required when not learning
                        Run again with the "learn" argument to get a list of available tools and their parameters.
                        To learn about a specific tool, use the "tool" argument with the name of the tool.
                    """
                }
                ],
            IsError = false
        };
    }

    private async Task<CallToolResult> InvokeChildToolAsync(
        RequestContext<CallToolRequestParams> request,
        string? intent,
        string namespaceName,
        string command,
        IReadOnlyDictionary<string, JsonElement> parameters,
        CancellationToken cancellationToken)
    {
        if (request.Params == null)
        {
            var content = new TextContentBlock
            {
                Text = "Cannot call tools with null parameters.",
            };

            _logger.LogWarning(content.Text);

            return new CallToolResult
            {
                Content = [content],
                IsError = true,
            };
        }

        IReadOnlyDictionary<string, IBaseCommand> namespaceCommands;
        try
        {
            namespaceCommands = _commandFactory.GroupCommands([namespaceName]);
            if (namespaceCommands == null || namespaceCommands.Count == 0)
            {
                _logger.LogError("Failed to get commands for namespace: {Namespace}", namespaceName);
                return await InvokeToolLearn(request, intent, namespaceName, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception thrown while getting commands for namespace: {Namespace}", namespaceName);
            return await InvokeToolLearn(request, intent, namespaceName, cancellationToken);
        }

        try
        {
            var availableTools = GetChildToolList(request, namespaceName);

            // When the specified command is not available, we try to learn about the tool's capabilities
            // and infer the command and parameters from the users intent.
            if (!availableTools.Any(t => string.Equals(t.Name, command, StringComparison.OrdinalIgnoreCase)))
            {
                _logger.LogWarning("Namespace {Namespace} does not have a command {Command}.", namespaceName, command);
                if (string.IsNullOrWhiteSpace(intent))
                {
                    return await InvokeToolLearn(request, intent, namespaceName, cancellationToken);
                }

                var samplingResult = await GetCommandAndParametersFromIntentAsync(request, intent, namespaceName, availableTools, cancellationToken);
                if (string.IsNullOrWhiteSpace(samplingResult.commandName))
                {
                    return await InvokeToolLearn(request, intent ?? "", namespaceName, cancellationToken);
                }

                command = samplingResult.commandName;
                parameters = samplingResult.parameters;
            }

            await NotifyProgressAsync(request, $"Calling {namespaceName} {command}...", cancellationToken);

            if (!namespaceCommands.TryGetValue(command, out var cmd))
            {
                _logger.LogError("Command {Command} found in tools but missing from namespace {Namespace} commands.", command, namespaceName);
                return await InvokeToolLearn(request, intent, namespaceName, cancellationToken);
            }

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

            _logger.LogTrace("Executing namespace command '{Namespace} {Command}'", namespaceName, command);

            var commandResponse = await cmd.ExecuteAsync(commandContext, commandOptions);
            var jsonResponse = JsonSerializer.Serialize(commandResponse, ModelsJsonContext.Default.CommandResponse);
            var isError = commandResponse.Status < HttpStatusCode.OK || commandResponse.Status >= HttpStatusCode.Ambiguous;

            if (jsonResponse.Contains("Missing required options", StringComparison.OrdinalIgnoreCase))
            {
                var childToolSpecJson = GetChildToolJson(request, namespaceName, command);

                _logger.LogWarning("Namespace {Namespace} command {Command} requires additional parameters.", namespaceName, command);
                var finalResponse = new CallToolResult
                {
                    Content =
                    [
                        new TextContentBlock {
                                Text = $"""
                                    The '{command}' command is missing required parameters.

                                    - Review the following command spec and identify the required arguments from the input schema.
                                    - Omit any arguments that are not required or do not apply to your use case.
                                    - Wrap all command arguments into the root "parameters" argument.
                                    - If required data is missing infer the data from your context or prompt the user as needed.
                                    - Run the tool again with the "command" and root "parameters" object.

                                    Command Spec:
                                    {childToolSpecJson}
                                    """
                            }
                    ],
                    IsError = true
                };

                // Add original response content
                finalResponse.Content.Add(new TextContentBlock { Text = jsonResponse });
                return finalResponse;
            }

            return new CallToolResult
            {
                Content = [new TextContentBlock { Text = jsonResponse }],
                IsError = isError
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception thrown while calling namespace: {Namespace}, command: {Command}", namespaceName, command);
            return new CallToolResult
            {
                Content =
                [
                    new TextContentBlock {
                        Text = $"""
                            There was an error finding or calling tool and command.
                            Failed to call namespace: {namespaceName}, command: {command}
                            Error: {ex.Message}

                            Run again with the "learn=true" to get a list of available commands and their parameters.
                            """
                    }
                ]
            };
        }
    }

    private async Task<CallToolResult> InvokeToolLearn(RequestContext<CallToolRequestParams> request, string? intent, string namespaceName, CancellationToken cancellationToken)
    {
        var toolsJson = GetChildToolListJson(request, namespaceName);

        var learnResponse = new CallToolResult
        {
            Content =
            [
                new TextContentBlock {
                    Text = $"""
                        Here are the available command and their parameters for '{namespaceName}' tool.
                        If you do not find a suitable command, run again with the "learn=true" to get a list of available commands and their parameters.
                        Next, identify the command you want to execute and run again with the "command" and "parameters" arguments.

                        {toolsJson}
                        """
                }
            ],
            IsError = false
        };
        var response = learnResponse;
        if (SupportsSampling(request.Server) && !string.IsNullOrWhiteSpace(intent))
        {
            var availableTools = GetChildToolList(request, namespaceName);
            (string? commandName, IReadOnlyDictionary<string, JsonElement> parameters) = await GetCommandAndParametersFromIntentAsync(request, intent, namespaceName, availableTools, cancellationToken);
            if (commandName != null)
            {
                response = await InvokeChildToolAsync(request, intent, namespaceName, commandName, parameters, cancellationToken);
            }
        }
        return response;
    }

    /// <summary>
    /// Gets the available tools from the namespace commands and caches the result for subsequent requests.
    /// </summary>
    private List<Tool> GetChildToolList(RequestContext<CallToolRequestParams> request, string namespaceName)
    {
        // Check cache first
        if (_cachedToolLists.TryGetValue(namespaceName, out var cachedList))
        {
            return cachedList;
        }

        if (string.IsNullOrWhiteSpace(request.Params?.Name))
        {
            throw new ArgumentNullException(nameof(request.Params.Name), "Tool name cannot be null or empty.");
        }

        var namespaces = _availableNamespaces.Value;
        if (!namespaces.Any(ns => string.Equals(ns, namespaceName, StringComparison.OrdinalIgnoreCase)))
        {
            var availableList = string.Join(", ", namespaces);
            throw new KeyNotFoundException($"The namespace '{namespaceName}' was not found. Available namespaces: {availableList}");
        }

        var namespaceCommands = _commandFactory.GroupCommands([namespaceName]);
        if (namespaceCommands == null)
        {
            _logger.LogWarning("No commands found for namespace: {Namespace}", namespaceName);
            return [];
        }

        var list = namespaceCommands
            .Where(kvp => !(_options.Value.ReadOnly ?? false) || kvp.Value.Metadata.ReadOnly)
            .Select(kvp => CreateToolFromCommand(kvp.Key, kvp.Value))
            .ToList();

        // Cache for subsequent requests
        _cachedToolLists[namespaceName] = list;

        return list;
    }

    private string GetChildToolListJson(RequestContext<CallToolRequestParams> request, string namespaceName)
    {
        var listTools = GetChildToolList(request, namespaceName);
        return JsonSerializer.Serialize(listTools, ServerJsonContext.Default.ListTool);
    }

    private string GetChildToolJson(RequestContext<CallToolRequestParams> request, string namespaceName, string commandName)
    {
        var tools = GetChildToolList(request, namespaceName);
        var tool = tools.First(t => string.Equals(t.Name, commandName, StringComparison.OrdinalIgnoreCase));
        return JsonSerializer.Serialize(tool, ServerJsonContext.Default.Tool);
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

    private static IReadOnlyDictionary<string, JsonElement> GetParametersFromArgs(IReadOnlyDictionary<string, JsonElement>? args)
    {
        if (args == null || !args.TryGetValue("parameters", out var paramsElem))
        {
            return new Dictionary<string, JsonElement>();
        }

        if (paramsElem.ValueKind == JsonValueKind.Object)
        {
            return paramsElem.EnumerateObject()
                .ToDictionary(prop => prop.Name, prop => prop.Value);
        }

        return new Dictionary<string, JsonElement>();
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
        string namespaceName,
        List<Tool> availableTools,
        CancellationToken cancellationToken)
    {
        await NotifyProgressAsync(request, $"Learning about {namespaceName} capabilities...", cancellationToken);

        JsonElement toolParams = GetParametersJsonElement(request);
        var toolParamsJson = toolParams.GetRawText();
        var availableToolsJson = JsonSerializer.Serialize(availableTools, ServerJsonContext.Default.ListTool);

        var samplingRequest = new CreateMessageRequestParams
        {
            Messages = [
                new SamplingMessage
                {
                    Role = Role.Assistant,
                    Content = new TextContentBlock{
                        Text = $"""
                            This is a list of available commands for the {namespaceName} server.

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
            _logger.LogError("Failed to get command and parameters from intent: {Intent} for namespace: {Namespace}", intent, namespaceName);
        }

        return (null, new Dictionary<string, JsonElement>());
    }

    /// <summary>
    /// Disposes resources owned by this tool loader.
    /// Clears the cached tool lists dictionary.
    /// </summary>
    protected override ValueTask DisposeAsyncCore()
    {
        _cachedToolLists.Clear();
        return ValueTask.CompletedTask;
    }
}
