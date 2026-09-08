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
using Microsoft.Mcp.Core.Areas.Server.Models;
using Microsoft.Mcp.Core.Areas.Server.Options;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Helpers;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Models.Command;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace Microsoft.Mcp.Core.Areas.Server.Commands.ToolLoading;

/// <summary>
/// A tool loader that exposes command groups as hierarchical namespace tools with direct in-process execution.
/// Provides the same functionality as <see cref="ServerToolLoader"/> but without spawning child azmcp processes.
/// Supports learn functionality for progressive discovery of commands within each namespace.
/// </summary>
public sealed class NamespaceToolLoader(
    ICommandFactory commandFactory,
    IOptions<ServerRuntimeConfiguration> configuration,
    ILogger<NamespaceToolLoader> logger,
    bool applyFilter = true) : BaseToolLoader(logger)
{
    private readonly ICommandFactory _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
    private readonly IOptions<ServerRuntimeConfiguration> _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

    private readonly Lazy<IReadOnlyList<string>> _availableNamespaces = new(() =>
    {
        IEnumerable<CommandGroup> allSubGroups = commandFactory.RootGroup.SubGroup;

        if (applyFilter)
        {
            allSubGroups = allSubGroups
                .Where(group => !DiscoveryConstants.IgnoredCommandGroups.Contains(group.Name, StringComparer.OrdinalIgnoreCase))
                .Where(group => configuration.Value.Namespace == null ||
                               configuration.Value.Namespace.Length == 0 ||
                               configuration.Value.Namespace.Contains(group.Name, StringComparer.OrdinalIgnoreCase));
        }

        return [.. allSubGroups.Select(group => group.Name)];
    });

    private readonly Dictionary<string, List<Tool>> _cachedToolLists = new(StringComparer.OrdinalIgnoreCase);
    private ListToolsResult? _cachedListToolsResult;
    private bool StructuredOutputEnabled => _configuration.Value.StructuredOutputMode != null;

    private const string ToolCallProxyInputSchema = """
        {
          "type": "object",
          "properties": {
            "command": {
              "type": "string",
              "description": "The name of the command to call."
            },
            "parameters": {
              "type": "object",
              "description": "A key/value pair of parameters names and values to pass to the command."
            }
          },
          "additionalProperties": false
        }
        """;

    private static readonly HashSet<string> s_metaKeys = new(StringComparer.OrdinalIgnoreCase) { "intent", "command", "learn", "parameters" };

    private static readonly JsonElement s_toolSchema = JsonSerializer.Deserialize("""
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

    public override ValueTask<ListToolsResult> ListToolsHandler(RequestContext<ListToolsRequestParams> request, CancellationToken cancellationToken)
    {
        if (_cachedListToolsResult != null)
        {
            return ValueTask.FromResult(_cachedListToolsResult);
        }

        var namespaces = _availableNamespaces.Value;
        var allToolsResponse = new ListToolsResult
        {
            Tools = []
        };

        foreach (var namespaceName in namespaces)
        {
            var group = _commandFactory.RootGroup.SubGroup
                .First(g => string.Equals(g.Name, namespaceName, StringComparison.OrdinalIgnoreCase));

            if (_configuration.Value.ReadOnly && AllToolsInGroupMatch(meta => !meta.ReadOnly, group))
            {
                // If ReadOnly mode is enabled and all commands in the group are not read-only, skip exposing this namespace as a tool.
                continue;
            }

            if (_configuration.Value.IsHttpMode && AllToolsInGroupMatch(meta => meta.LocalRequired, group))
            {
                // If HTTP mode is enabled and all commands in the group are local-required, skip exposing this namespace as a tool.
                continue;
            }

            var tool = new Tool
            {
                Name = namespaceName,
                Description = group.Description + Environment.NewLine + Environment.NewLine + """
                    This tool is a hierarchical MCP command router.
                    To invoke a command, set "command" and wrap its args in "parameters".
                    Set "learn=true" to discover available sub commands.
                    """,
                InputSchema = s_toolSchema,
                OutputSchema = StructuredOutputEnabled ? AggregateStructuredOutput.NamespaceOutputSchema : null,
                Annotations = new ToolAnnotations()
                {
                    Title = group.Title ?? namespaceName,
                    DestructiveHint = group.ToolMetadata?.Destructive,
                    IdempotentHint = group.ToolMetadata?.Idempotent,
                    OpenWorldHint = group.ToolMetadata?.OpenWorld,
                    ReadOnlyHint = group.ToolMetadata?.ReadOnly,
                },
            };

            allToolsResponse.Tools.Add(tool);
        }

        // Cache the result
        _cachedListToolsResult = allToolsResponse;
        return ValueTask.FromResult(allToolsResponse);
    }

    private static bool AllToolsInGroupMatch(Predicate<ToolMetadata> predicate, CommandGroup group)
    {
        foreach (var command in group.Commands)
        {
            if (!predicate(command.Value.Metadata))
            {
                return false;
            }
        }

        foreach (var subGroup in group.SubGroup)
        {
            if (!AllToolsInGroupMatch(predicate, subGroup))
            {
                return false;
            }
        }

        return true;
    }

    public override async ValueTask<CallToolResult> CallToolHandler(RequestContext<CallToolRequestParams> request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Params?.Name))
        {
            throw new ArgumentNullException(nameof(request.Params.Name), "Tool name cannot be null or empty.");
        }

        string tool = request.Params.Name;
        var args = request.Params.Arguments;
        string? intent = null;
        string? command = null;
        bool learn = false;

        // In namespace mode, the name of the tool is also its IAreaSetup name.
        Activity.Current?.SetTag(TagName.IsServerCommandInvoked, false)
            // At this point the tool parameters is the namespace tool schema
            .SetTag(TagName.ToolParameters, McpHelper.CreateToolParametersTelemetry(request.Params.Arguments?.Keys))
            .SetTag(TagName.ToolArea, tool);

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
            var activity = Activity.Current;

            if (learn)
            {
                return await InvokeToolLearn(request, intent ?? "", tool, cancellationToken);
            }
            else if (!string.IsNullOrEmpty(tool) && !string.IsNullOrEmpty(command))
            {
                // We no longer spawn new processes to handle child tool invocations.
                // So, we have to update ToolName to represent the namespace's child tool name
                // rather than what is exposed to the user. The following inputs would
                // be routed here.  In both examples, the end-user's MCP client sees that we expose
                // a tool called "storage" and would invoke our "storage" tool.
                //
                // A) {
                //       "intent": "List storage blobs.",
                //       "command": "blob_list",
                //       "parameters": [ "--name", "foo", "--subscription-id", "bar" ]
                //    }
                //
                // This is the case where the LLM knows what tool should be executed, so it passes
                // in all the parameters required to execute the underlying Storage tool.
                //
                // B) {
                //       "intent": "List storage blobs.",
                //       "command": "blob_list",
                //       "parameters": []
                //       "learn": true
                //    }
                //
                // This command attempts to learn what the command "blob_list" entails by
                // invoking it with no parameters and "learn" == "true".  The command will
                // generally fail, providing the LLM with extra information it needs to pass
                // in for the command to succeed the next time.
                activity?.SetTag(TagName.ToolName, command);

                var toolParams = GetParametersFromArgs(args);
                return await InvokeChildToolAsync(
                    request,
                    intent ?? "",
                    tool,
                    command,
                    toolParams,
                    cancellationToken);
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

        const string helpMessage = """
            The "command" parameter is required when not learning.
            Run again with the "learn" argument to get a list of available tools and their parameters.
            To learn about a specific tool, use the "command" argument with the name of the tool.
            """;
        return StructuredOutputHelper.CreateCallToolResult(
            _configuration.Value.StructuredOutputMode,
            () => helpMessage,
            () => AggregateStructuredOutput.CreateMessage(helpMessage),
            isError: false);
    }

    private async Task<CallToolResult> InvokeChildToolAsync(
        RequestContext<CallToolRequestParams> request,
        string? intent,
        string namespaceName,
        string command,
        IDictionary<string, JsonElement> parameters,
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

            // Here the parameters are now those for the tool call, instead of being the namespace parameters.
            Activity.Current?.SetTag(TagName.ToolParameters, McpHelper.CreateToolParametersTelemetry(parameters.Keys));

            await NotifyProgressAsync(request, $"Calling {namespaceName} {command}...", cancellationToken);

            if (!namespaceCommands.TryGetValue(command, out var cmd))
            {
                _logger.LogError("Command {Command} found in tools but missing from namespace {Namespace} commands.", command, namespaceName);
                return await InvokeToolLearn(request, intent, namespaceName, cancellationToken);
            }

            Activity.Current?.SetTag(TagName.ToolAnnotations, McpHelper.CreateToolAnnotationTelemetry(cmd));

            // Enforce read-only mode at execution time
            if (_configuration.Value.ReadOnly && !cmd.Metadata.ReadOnly)
            {
                return new CallToolResult
                {
                    Content =
                    [
                        new TextContentBlock
                        {
                            Text = $"Tool '{namespaceName} {command}' is not available. This server is configured in read-only mode and this tool is not a read-only tool.",
                        }
                    ],
                    IsError = true,
                    Meta = new([new(McpHelper.ToolIdMetaKey, cmd.Id)])
                };
            }

            // Enforce HTTP mode restrictions at execution time
            if (_configuration.Value.IsHttpMode && cmd.Metadata.LocalRequired)
            {
                return new CallToolResult
                {
                    Content =
                    [
                        new TextContentBlock
                        {
                            Text = $"Tool '{namespaceName} {command}' is not available. This server is running in HTTP mode and this tool requires local execution.",
                        }
                    ],
                    IsError = true,
                    Meta = new([new(McpHelper.ToolIdMetaKey, cmd.Id)])
                };
            }

            // Check if this tool requires elicitation for sensitive or destructive operations
            var elicitationResult = await HandleElicitationAsync(
                request,
                $"{namespaceName} {command}",
                cmd,
                _configuration.Value.DangerouslyDisableElicitation,
                _logger,
                cancellationToken);

            if (elicitationResult != null)
            {
                return elicitationResult;
            }

            var currentActivity = Activity.Current;
            var commandContext = new CommandContext(currentActivity)
            {
                McpServer = request.Server,
                ProgressToken = request.Params?.ProgressToken
            };
            var realCommand = cmd.GetCommand();

            ParseResult? commandOptions;
            var effectiveOptions = realCommand.Options
                .Where(o => !CommandFactory.IsLearnOption(o))
                .ToList();

            if (effectiveOptions.Count == 1 && IsRawMcpToolInputOption(effectiveOptions[0]))
            {
                commandOptions = realCommand.ParseFromRawMcpToolInput(parameters);
            }
            else
            {
                if (!realCommand.TryParseFromDictionary(parameters, out commandOptions, out var parseErrors))
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
                        Meta = new([new(McpHelper.ToolIdMetaKey, cmd.Id)])
                    };
                }
            }

            _logger.LogTrace("Executing namespace command '{Namespace} {Command}'", namespaceName, command);

            // It is possible that the command provided by the LLM is not one that exists, such as "blob-list".
            // The logic above performs sampling to try and get a correct command name.  "blob_get" in
            // this case, which will be executed.
            currentActivity?.SetTag(TagName.ToolName, command)
                .SetTag(TagName.ToolId, cmd.Id)
                .SetTag(TagName.ToolSource, "internal")
                .SetTag(TagName.IsServerCommandInvoked, true);

            var commandResponse = await cmd.ExecuteAsync(commandContext, commandOptions!, cancellationToken);
            var isError = commandResponse.Status < HttpStatusCode.OK || commandResponse.Status >= HttpStatusCode.Ambiguous;

            if (commandResponse.Message.Contains("Missing required options", StringComparison.OrdinalIgnoreCase))
            {
                var jsonResponse = JsonSerializer.Serialize(commandResponse, ModelsJsonContext.Default.CommandResponse);
                var childTool = GetChildToolList(request, namespaceName)
                    .First(t => string.Equals(t.Name, command, StringComparison.OrdinalIgnoreCase));
                var childToolSpecJson = JsonSerializer.Serialize(new ToolCommandInfo(childTool), ServerJsonContext.Default.ToolCommandInfo);

                _logger.LogWarning("Namespace {Namespace} command {Command} requires additional parameters.", namespaceName, command);

                // Extract the specific error message from the response
                var errorMessage = string.IsNullOrEmpty(commandResponse.Message)
                    ? $"The '{command}' command is missing required parameters."
                    : commandResponse.Message;

                return new CallToolResult
                {
                    Content =
                    [
                        new TextContentBlock
                        {
                            Text = $"""
                                {errorMessage}

                                - Review the following command spec and identify the required arguments from the input schema.
                                - Omit any arguments that are not required or do not apply to your use case.
                                - Wrap all command arguments into the root "parameters" argument.
                                - If required data is missing infer the data from your context or prompt the user as needed.
                                - Run the tool again with the "command" and root "parameters" object.

                                Command Spec:
                                {childToolSpecJson}
                                """
                        },
                        // Add original response content
                        new TextContentBlock { Text = jsonResponse }
                    ],
                    IsError = true,
                    Meta = new([new(McpHelper.ToolIdMetaKey, cmd.Id)])
                };
            }

            var result = StructuredOutputHelper.CreateCallToolResult(
                _configuration.Value.StructuredOutputMode,
                () => JsonSerializer.Serialize(commandResponse, ModelsJsonContext.Default.CommandResponse),
                () => AggregateStructuredOutput.CreateToolResult(command, commandResponse),
                isError);
            return McpHelper.InjectToolIdMetadata(result, cmd.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception thrown while calling namespace: {Namespace}, command: {Command}", namespaceName, command);
            return new CallToolResult
            {
                Content =
                [
                    new TextContentBlock
                    {
                        Text = $"""
                            There was an error finding or calling tool and command.
                            Failed to call namespace: {namespaceName}, command: {command}
                            Error: {ex.Message}

                            Run again with the "learn=true" to get a list of available commands and their parameters.
                            """
                    }
                ],
                IsError = true
            };
        }
    }

    private async Task<CallToolResult> InvokeToolLearn(
        RequestContext<CallToolRequestParams> request,
        string? intent,
        string namespaceName,
        CancellationToken cancellationToken)
    {
        Activity.Current?.SetTag(TagName.IsServerCommandInvoked, false)
            .SetTag(TagName.IsLearn, true);
        var learnTools = GetChildToolList(request, namespaceName).Select(t => new ToolCommandInfo(t));
        var learnToolsJson = JsonSerializer.Serialize(learnTools, ServerJsonContext.Default.IEnumerableToolCommandInfo);
        var contentText = $"""
            Here are the available commands and their input schema for '{namespaceName}' tool.
            If you do not find a suitable "command", run again with the "learn=true" to get a list of available commands and their parameters.
            Next, identify the command you want to execute and run again with the "command" and "parameters" arguments, respecting "required" parameters if present.

            {learnToolsJson}
            """;
        var learnResponse = StructuredOutputHelper.CreateCallToolResult(
            _configuration.Value.StructuredOutputMode,
            () => contentText,
            () => AggregateStructuredOutput.CreateToolList(learnToolsJson),
            isError: false);
        var response = learnResponse;
        if (SupportsSampling(request.Server) && !string.IsNullOrWhiteSpace(intent))
        {
            var availableTools = GetChildToolList(request, namespaceName);
            (string? commandName, IDictionary<string, JsonElement> parameters) = await GetCommandAndParametersFromIntentAsync(request, intent, namespaceName, availableTools, cancellationToken);
            if (commandName != null)
            {
                response = await InvokeChildToolAsync(
                    request,
                    intent,
                    namespaceName,
                    commandName,
                    parameters,
                    cancellationToken);
            }
        }
        return response;
    }

    /// <summary>
    /// Gets the available tools from the namespace commands and caches the result for subsequent requests.
    /// </summary>
    internal List<Tool> GetChildToolList(RequestContext<CallToolRequestParams> request, string namespaceName)
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
            .Where(kvp => !_configuration.Value.ReadOnly || kvp.Value.Metadata.ReadOnly)
            .Where(kvp => !_configuration.Value.IsHttpMode || !kvp.Value.Metadata.LocalRequired)
            .Select(kvp => CreateToolFromCommand(kvp.Key, kvp.Value))
            .ToList();

        // Cache for subsequent requests
        _cachedToolLists[namespaceName] = list;

        return list;
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
            var arguments = JsonNode.Parse(options[0].Description ?? "{}") as JsonObject ?? new JsonObject();
            tool.InputSchema = JsonSerializer.SerializeToElement(arguments, ServerJsonContext.Default.JsonObject);
            return tool;
        }

        var schema = OptionSchemaGenerator.CreateInputSchema(options);
        tool.InputSchema = JsonSerializer.SerializeToElement(schema, ServerJsonContext.Default.JsonObject);
        return tool;
    }

    internal static Dictionary<string, JsonElement> GetParametersFromArgs(IDictionary<string, JsonElement>? args)
    {
        if (args == null)
        {
            return [];
        }

        // Primary: extract from nested "parameters" key (case-insensitive)
        var parametersKey = args.Keys.FirstOrDefault(k => string.Equals(k, "parameters", StringComparison.OrdinalIgnoreCase));
        if (parametersKey != null && args.TryGetValue(parametersKey, out var paramsElem) && paramsElem.ValueKind == JsonValueKind.Object)
        {
            return paramsElem.EnumerateObject()
                .ToDictionary(prop => prop.Name, prop => prop.Value);
        }

        // Fallback: treat all non-meta args as parameters (Codex compatibility)
        var flatParams = args
            .Where(kvp => !s_metaKeys.Contains(kvp.Key))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        return flatParams;
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
            }, cancellationToken: cancellationToken);
    }

    private async Task<(string? commandName, Dictionary<string, JsonElement> parameters)> GetCommandAndParametersFromIntentAsync(
        RequestContext<CallToolRequestParams> request,
        string intent,
        string namespaceName,
        List<Tool> availableTools,
        CancellationToken cancellationToken)
    {
#pragma warning disable MCP9005 // Sampling APIs remain for backward compatibility during migration.
        await NotifyProgressAsync(request, $"Learning about {namespaceName} capabilities...", cancellationToken);

        var availableToolsJson = JsonSerializer.Serialize(availableTools.Select(t => new ToolCommandInfo(t)), ServerJsonContext.Default.IEnumerableToolCommandInfo);

        var samplingRequest = new CreateMessageRequestParams
        {
            MaxTokens = 1000, // arbitrary limit that can be changed in the future as needed; MCP requires a max be specified
            Messages = [
                new SamplingMessage
                {
                    Role = Role.Assistant,
                    Content = [new TextContentBlock
                    {
                        Text = $"""
                            Your task:
                            - Select the single command that best matches the user's intent.
                            - Return a valid JSON object that matches the provided result schema.
                            - Map the user's intent and known parameters to the command's input schema, ensuring parameter names and types match the schema exactly (no extra or missing parameters).
                            - Only include parameters that are defined in the selected command's input schema.
                            - Do not guess or invent parameters.
                            - If no command matches, return JSON schema with "Unknown" command name.

                            Result Schema:
                            {ToolCallProxyInputSchema}

                            Intent:
                            {intent ?? "No specific intent provided"}

                            Known Parameters:
                            {GetParametersJsonElement(request).GetRawText()}

                            Available Commands:
                            {availableToolsJson}
                            """
                    }]
                }
            ],
        };
        try
        {
            var samplingResponse = await request.Server.SampleAsync(samplingRequest, cancellationToken);
            var samplingContent = samplingResponse.Content is { Count: > 0 } ? samplingResponse.Content[0] as TextContentBlock : null;
            var commandCallJson = samplingContent?.Text?.Trim();
            string? commandName = null;
            var parameters = new Dictionary<string, JsonElement>();

            if (!string.IsNullOrEmpty(commandCallJson))
            {
                using var jsonDoc = JsonDocument.Parse(commandCallJson);
                var root = jsonDoc.RootElement;
                if (root.TryGetProperty("command", out var commandProp) && commandProp.ValueKind == JsonValueKind.String)
                {
                    commandName = commandProp.GetString();
                }
                if (root.TryGetProperty("parameters", out var parametersElem) && parametersElem.ValueKind == JsonValueKind.Object)
                {
                    parameters = parametersElem.EnumerateObject().ToDictionary(prop => prop.Name, prop => prop.Value.Clone()) ?? [];
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
#pragma warning restore MCP9005
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
