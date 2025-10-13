// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using Azure.Mcp.Core.Areas.Server.Commands.Discovery;
using Azure.Mcp.Core.Services.Telemetry;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModelContextProtocol;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace Azure.Mcp.Core.Areas.Server.Commands.ToolLoading;

public sealed class ServerToolLoader(IMcpDiscoveryStrategy serverDiscoveryStrategy, IOptions<ToolLoaderOptions> options, ILogger<ServerToolLoader> logger) : BaseToolLoader(logger)
{
    private readonly IMcpDiscoveryStrategy _serverDiscoveryStrategy = serverDiscoveryStrategy ?? throw new ArgumentNullException(nameof(serverDiscoveryStrategy));
    private readonly Dictionary<string, List<Tool>> _cachedToolLists = new(StringComparer.OrdinalIgnoreCase);

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
              "description": "A key/value pair of parameters names nad values to pass to the tool call command."
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

    public override async ValueTask<ListToolsResult> ListToolsHandler(RequestContext<ListToolsRequestParams> request, CancellationToken cancellationToken)
    {
        var serverList = await _serverDiscoveryStrategy.DiscoverServersAsync();
        var allToolsResponse = new ListToolsResult
        {
            Tools = new List<Tool>()
        };

        foreach (var server in serverList)
        {
            var metadata = server.CreateMetadata();
            var tool = new Tool
            {
                Name = metadata.Name,
                Description = metadata.Description + """
                    This tool is a hierarchical MCP command router.
                    Sub commands are routed to MCP servers that require specific fields inside the "parameters" object.
                    To invoke a command, set "command" and wrap its args in "parameters".
                    Set "learn=true" to discover available sub commands.
                    """,
                InputSchema = ToolSchema,
            };

            allToolsResponse.Tools.Add(tool);
        }

        return allToolsResponse;
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
                Activity.Current?.AddTag(TelemetryConstants.TagName.IsServerCommandInvoked, false);

                return await InvokeToolLearn(request, intent ?? "", tool, cancellationToken);
            }
            else if (!string.IsNullOrEmpty(tool) && !string.IsNullOrEmpty(command))
            {
                var toolParams = GetParametersDictionary(request);
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
                ]
        };
    }

    private async Task<CallToolResult> InvokeChildToolAsync(RequestContext<CallToolRequestParams> request, string? intent, string tool, string command, Dictionary<string, object?> parameters, CancellationToken cancellationToken)
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

        McpClient client;
        try
        {
            var clientOptions = CreateClientOptions(request.Server);
            client = await _serverDiscoveryStrategy.GetOrCreateClientAsync(tool, clientOptions);
            if (client == null)
            {
                _logger.LogError("Failed to get provider client for tool: {Tool}", tool);
                return await InvokeToolLearn(request, intent, tool, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception thrown while getting provider client for tool: {Tool}", tool);
            return await InvokeToolLearn(request, intent, tool, cancellationToken);
        }

        try
        {
            var availableTools = await GetChildToolListAsync(request, tool);

            // When the specified command is not available, we try to learn about the tool's capabilities
            // and infer the command and parameters from the users intent.
            if (!availableTools.Any(t => string.Equals(t.Name, command, StringComparison.OrdinalIgnoreCase)))
            {
                _logger.LogWarning("Tool {Tool} does not have a command {Command}.", tool, command);
                if (string.IsNullOrWhiteSpace(intent))
                {
                    return await InvokeToolLearn(request, intent, tool, cancellationToken);
                }

                var samplingResult = await GetCommandAndParametersFromIntentAsync(request, intent, tool, availableTools, cancellationToken);
                if (string.IsNullOrWhiteSpace(samplingResult.commandName))
                {
                    return await InvokeToolLearn(request, intent ?? "", tool, cancellationToken);
                }

                command = samplingResult.commandName;
                parameters = samplingResult.parameters;
            }

            // At this point we should always have a valid command (child tool) call to invoke.
            await NotifyProgressAsync(request, $"Calling {tool} {command}...", cancellationToken);
            var toolCallResponse = await client.CallToolAsync(command, parameters, cancellationToken: cancellationToken);
            if (toolCallResponse.IsError is true)
            {
                _logger.LogWarning("Tool {Tool} command {Command} returned an error.", tool, command);
            }

            foreach (var content in toolCallResponse.Content)
            {
                var textContent = content as TextContentBlock;
                if (textContent == null || string.IsNullOrWhiteSpace(textContent.Text))
                {
                    continue;
                }

                if (textContent.Text.Contains("Missing required options", StringComparison.OrdinalIgnoreCase))
                {
                    var childToolSpecJson = await GetChildToolJsonAsync(request, tool, command);

                    _logger.LogWarning("Tool {Tool} command {Command} requires additional parameters.", tool, command);
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
                        ]
                    };

                    foreach (var contentBlock in toolCallResponse.Content)
                    {
                        finalResponse.Content.Add(contentBlock);
                    }

                    return finalResponse;
                }
            }

            return toolCallResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception thrown while calling tool: {Tool}, command: {Command}", tool, command);
            return new CallToolResult
            {
                Content =
                [
                    new TextContentBlock {
                        Text = $"""
                            There was an error finding or calling tool and command.
                            Failed to call tool: {tool}, command: {command}
                            Error: {ex.Message}

                            Run again with the "learn=true" to get a list of available commands and their parameters.
                            """
                    }
                ]
            };
        }
    }

    private async Task<CallToolResult> InvokeToolLearn(RequestContext<CallToolRequestParams> request, string? intent, string tool, CancellationToken cancellationToken)
    {
        var toolsJson = await GetChildToolListJsonAsync(request, tool);

        var learnResponse = new CallToolResult
        {
            Content =
            [
                new TextContentBlock {
                    Text = $"""
                        Here are the available command and their parameters for '{tool}' tool.
                        If you do not find a suitable command, run again with the "learn=true" to get a list of available commands and their parameters.
                        Next, identify the command you want to execute and run again with the "command" and "parameters" arguments.

                        {toolsJson}
                        """
                }
            ]
        };
        var response = learnResponse;
        if (SupportsSampling(request.Server) && !string.IsNullOrWhiteSpace(intent))
        {
            var availableTools = await GetChildToolListAsync(request, tool);
            (string? commandName, Dictionary<string, object?> parameters) = await GetCommandAndParametersFromIntentAsync(request, intent, tool, availableTools, cancellationToken);
            if (commandName != null)
            {
                response = await InvokeChildToolAsync(request, intent, tool, commandName, parameters, cancellationToken);
            }
        }
        return response;
    }

    /// <summary>
    /// Gets the available tools from the child MCP server and caches the result as JSON.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="tool"></param>
    /// <returns></returns>
    private async Task<List<Tool>> GetChildToolListAsync(RequestContext<CallToolRequestParams> request, string tool)
    {
        if (_cachedToolLists.TryGetValue(tool, out var cachedList))
        {
            return cachedList;
        }

        if (string.IsNullOrWhiteSpace(request.Params?.Name))
        {
            throw new ArgumentNullException(nameof(request.Params.Name), "Tool name cannot be null or empty.");
        }

        var clientOptions = CreateClientOptions(request.Server);
        var client = await _serverDiscoveryStrategy.GetOrCreateClientAsync(request.Params.Name, clientOptions);
        if (client == null)
        {
            return [];
        }

        var listTools = await client.ListToolsAsync();
        if (listTools == null)
        {
            _logger.LogWarning("No tools found for tool: {Tool}", tool);
            return [];
        }

        var list = listTools
            .Select(t => t.ProtocolTool)
            .Where(t => !(options?.Value?.ReadOnly ?? false) || (t.Annotations?.ReadOnlyHint == true))
            .ToList();

        _cachedToolLists[tool] = list;
        return list;
    }

    private async Task<string> GetChildToolListJsonAsync(RequestContext<CallToolRequestParams> request, string tool)
    {
        var listTools = await GetChildToolListAsync(request, tool);
        return JsonSerializer.Serialize(listTools, ServerJsonContext.Default.ListTool);
    }

    private async Task<Tool> GetChildToolAsync(RequestContext<CallToolRequestParams> request, string toolName, string commandName)
    {
        var tools = await GetChildToolListAsync(request, toolName);
        return tools.First(t => string.Equals(t.Name, commandName, StringComparison.OrdinalIgnoreCase));
    }

    private async Task<string> GetChildToolJsonAsync(RequestContext<CallToolRequestParams> request, string toolName, string commandName)
    {
        var tool = await GetChildToolAsync(request, toolName, commandName);
        return JsonSerializer.Serialize(tool, ServerJsonContext.Default.Tool);
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
    private async Task<(string? commandName, Dictionary<string, object?> parameters)> GetCommandAndParametersFromIntentAsync(
        RequestContext<CallToolRequestParams> request,
        string intent,
        string tool,
        List<Tool> availableTools,
        CancellationToken cancellationToken)
    {
        await NotifyProgressAsync(request, $"Learning about {tool} capabilities...", cancellationToken);

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
                            This is a list of available commands for the {tool} server.

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
            Dictionary<string, object?> parameters = [];
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
                    parameters = parametersElem.EnumerateObject().ToDictionary(prop => prop.Name, prop => (object?)prop.Value) ?? [];
                }
            }
            if (commandName != null && commandName != "Unknown")
            {
                return (commandName, parameters);
            }
        }
        catch
        {
            _logger.LogError("Failed to get command and parameters from intent: {Intent} for tool: {Tool}", intent, tool);
        }

        return (null, new Dictionary<string, object?>());
    }

    /// <summary>
    /// Disposes resources owned by this tool loader.
    /// Clears the cached tool lists dictionary.
    /// </summary>
    protected override async ValueTask DisposeAsyncCore()
    {
        _cachedToolLists.Clear();
        await ValueTask.CompletedTask;
    }
}
