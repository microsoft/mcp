// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Areas.Server.Models;
using Microsoft.Mcp.Core.Areas.Server.Options;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Helpers;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace Microsoft.Mcp.Core.Areas.Server.Commands.ToolLoading;

public sealed partial class CommandFactoryToolLoader
{
    private readonly ConcurrentDictionary<string, List<Tool>> _cachedCommandGroupTools = new(StringComparer.OrdinalIgnoreCase);

    private const string ToolCallProxySchema = """
        {
          "type": "object",
          "properties": {
            "command": {
              "type": "string",
              "description": "The name of the command to call."
            },
            "parameters": {
              "type": "object",
              "description": "A key/value pair of parameter names and values to pass to the command."
            }
          },
          "additionalProperties": false
        }
        """;

    private static readonly HashSet<string> s_metaKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "intent",
        "command",
        "learn",
        "parameters"
    };

    private async ValueTask<CallToolResult> CallCommandGroupToolHandler(
        RequestContext<CallToolRequestParams> request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Params?.Name))
        {
            throw new ArgumentNullException(nameof(request.Params.Name), "Tool name cannot be null or empty.");
        }

        var namespaceName = request.Params.Name;
        var arguments = request.Params.Arguments;
        string? intent = null;
        string? command = null;
        var learn = false;

        Activity.Current?.SetTag(TagName.IsServerCommandInvoked, false)
            .SetTag(TagName.ToolParameters, McpHelper.CreateToolParametersTelemetry(arguments?.Keys))
            .SetTag(TagName.ToolArea, namespaceName);

        if (arguments != null)
        {
            if (arguments.TryGetValue("intent", out var intentElement) && intentElement.ValueKind == JsonValueKind.String)
            {
                intent = intentElement.GetString();
            }

            if (arguments.TryGetValue("learn", out var learnElement) && learnElement.ValueKind == JsonValueKind.True)
            {
                learn = true;
            }

            if (arguments.TryGetValue("command", out var commandElement) && commandElement.ValueKind == JsonValueKind.String)
            {
                command = commandElement.GetString();
            }
        }

        if (!learn && !string.IsNullOrEmpty(intent) && string.IsNullOrEmpty(command))
        {
            learn = true;
        }

        try
        {
            if (learn)
            {
                return await InvokeCommandGroupLearnAsync(request, intent ?? string.Empty, namespaceName, cancellationToken);
            }

            if (!string.IsNullOrEmpty(command))
            {
                return await InvokeChildCommandAsync(
                    request,
                    intent ?? string.Empty,
                    namespaceName,
                    command,
                    GetParametersFromArgs(arguments),
                    cancellationToken);
            }
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogError(ex, "Key not found while calling tool: {Tool}", namespaceName);
            return new CallToolResult
            {
                Content =
                [
                    new TextContentBlock
                    {
                        Text = $"""
                            The tool '{namespaceName}.{command}' was not found or does not support the specified command.
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
                new TextContentBlock
                {
                    Text = """
                        The "command" parameter is required when not learning.
                        Run again with the "learn" argument to get a list of available tools and their parameters.
                        To learn about a specific tool, use the "command" argument with the name of the tool.
                        """
                }
            ],
            IsError = false
        };
    }

    internal async Task<CallToolResult> InvokeChildCommandAsync(
        RequestContext<CallToolRequestParams> request,
        string intent,
        string namespaceName,
        string commandName,
        IDictionary<string, JsonElement> parameters,
        CancellationToken cancellationToken)
    {
        try
        {
            var namespaceCommands = GetCommandsForGroup(namespaceName);
            var availableTools = GetChildToolList(namespaceName);

            if (!availableTools.Any(tool => string.Equals(tool.Name, commandName, StringComparison.OrdinalIgnoreCase)))
            {
                _logger.LogWarning("Namespace {Namespace} does not have a command {Command}.", namespaceName, commandName);
                if (string.IsNullOrWhiteSpace(intent))
                {
                    return await InvokeCommandGroupLearnAsync(request, intent, namespaceName, cancellationToken);
                }

                var sampledCommand = await GetCommandAndParametersFromIntentAsync(
                    request,
                    intent,
                    namespaceName,
                    availableTools,
                    cancellationToken);

                if (string.IsNullOrWhiteSpace(sampledCommand.CommandName))
                {
                    return await InvokeCommandGroupLearnAsync(request, intent, namespaceName, cancellationToken);
                }

                commandName = sampledCommand.CommandName;
                parameters = sampledCommand.Parameters;
            }

            if (!namespaceCommands.TryGetValue(commandName, out var command))
            {
                _logger.LogError("Command {Command} is missing from namespace {Namespace}.", commandName, namespaceName);
                return await InvokeCommandGroupLearnAsync(request, intent, namespaceName, cancellationToken);
            }

            Activity.Current?.SetTag(TagName.ToolParameters, McpHelper.CreateToolParametersTelemetry(parameters.Keys));
            await NotifyProgressAsync(request, $"Calling {namespaceName} {commandName}...", cancellationToken);

            var result = await ExecuteCommandAsync(
                request,
                commandName,
                $"{namespaceName} {commandName}",
                command,
                parameters,
                namespaceName,
                cancellationToken);

            return AddMissingOptionsGuidance(result, availableTools, commandName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception thrown while calling namespace: {Namespace}, command: {Command}", namespaceName, commandName);
            return new CallToolResult
            {
                Content =
                [
                    new TextContentBlock
                    {
                        Text = $"""
                            There was an error finding or calling tool and command.
                            Failed to call namespace: {namespaceName}, command: {commandName}
                            Error: {ex.Message}

                            Run again with the "learn=true" to get a list of available commands and their parameters.
                            """
                    }
                ],
                IsError = true
            };
        }
    }

    private async Task<CallToolResult> InvokeCommandGroupLearnAsync(
        RequestContext<CallToolRequestParams> request,
        string intent,
        string namespaceName,
        CancellationToken cancellationToken)
    {
        Activity.Current?.SetTag(TagName.IsServerCommandInvoked, false)
            .SetTag(TagName.IsLearn, true);

        var availableTools = GetChildToolList(namespaceName);
        var toolsJson = JsonSerializer.Serialize(
            availableTools.Select(tool => new ToolCommandInfo(tool)),
            ServerJsonContext.Default.IEnumerableToolCommandInfo);

        var learnResponse = new CallToolResult
        {
            Content =
            [
                new TextContentBlock
                {
                    Text = $"""
                        Here are the available commands and their input schema for '{namespaceName}' tool.
                        If you do not find a suitable "command", run again with the "learn=true" argument to get a list of available commands and their parameters.
                        Next, identify the command you want to execute and run again with the "command" and "parameters" arguments, respecting "required" parameters if present.

                        {toolsJson}
                        """
                }
            ],
            IsError = false
        };

        if (!SupportsSampling(request.Server) || string.IsNullOrWhiteSpace(intent))
        {
            return learnResponse;
        }

        var sampledCommand = await GetCommandAndParametersFromIntentAsync(
            request,
            intent,
            namespaceName,
            availableTools,
            cancellationToken);

        return sampledCommand.CommandName == null
            ? learnResponse
            : await InvokeChildCommandAsync(
                request,
                intent,
                namespaceName,
                sampledCommand.CommandName,
                sampledCommand.Parameters,
                cancellationToken);
    }

    internal List<Tool> GetChildToolList(string namespaceName)
    {
        if (_cachedCommandGroupTools.TryGetValue(namespaceName, out var cachedTools))
        {
            return cachedTools;
        }

        if (!_commandGroups.Value.Any(group => string.Equals(group.Name, namespaceName, StringComparison.OrdinalIgnoreCase)))
        {
            var availableNamespaces = string.Join(", ", _commandGroups.Value.Select(group => group.Name));
            throw new KeyNotFoundException($"The namespace '{namespaceName}' was not found. Available namespaces: {availableNamespaces}");
        }

        var tools = GetCommandsForGroup(namespaceName)
            .Where(command => IsToolIncluded(command.Key))
            .Where(command => !_configuration.Value.ReadOnly || command.Value.Metadata.ReadOnly)
            .Where(command => !_configuration.Value.IsHttpMode || !command.Value.Metadata.LocalRequired)
            .Select(command => GetTool(command.Key, command.Value))
            .ToList();

        _cachedCommandGroupTools[namespaceName] = tools;
        return tools;
    }

    private IReadOnlyDictionary<string, IBaseCommand> GetCommandsForGroup(string namespaceName)
    {
        if (_configuration.Value.Mode != ModeTypes.ConsolidatedProxy)
        {
            return _commandFactory.GroupCommands([namespaceName]);
        }

        return _commandGroups.Value
            .First(group => string.Equals(group.Name, namespaceName, StringComparison.OrdinalIgnoreCase))
            .Commands;
    }

    internal static Dictionary<string, JsonElement> GetParametersFromArgs(IDictionary<string, JsonElement>? arguments)
    {
        if (arguments == null)
        {
            return [];
        }

        var parametersKey = arguments.Keys.FirstOrDefault(key => string.Equals(key, "parameters", StringComparison.OrdinalIgnoreCase));
        if (parametersKey != null &&
            arguments.TryGetValue(parametersKey, out var parametersElement) &&
            parametersElement.ValueKind == JsonValueKind.Object)
        {
            return parametersElement.EnumerateObject()
                .ToDictionary(property => property.Name, property => property.Value);
        }

        return arguments
            .Where(argument => !s_metaKeys.Contains(argument.Key))
            .ToDictionary(argument => argument.Key, argument => argument.Value);
    }

    private static CallToolResult AddMissingOptionsGuidance(
        CallToolResult result,
        List<Tool> availableTools,
        string commandName)
    {
        var missingOptionsContent = result.Content
            .OfType<TextContentBlock>()
            .FirstOrDefault(content => content.Text.Contains("Missing required options", StringComparison.OrdinalIgnoreCase));

        if (missingOptionsContent == null)
        {
            return result;
        }

        var childTool = availableTools.First(tool => string.Equals(tool.Name, commandName, StringComparison.OrdinalIgnoreCase));
        var childToolJson = JsonSerializer.Serialize(new ToolCommandInfo(childTool), ServerJsonContext.Default.ToolCommandInfo);
        result.Content.Insert(0, new TextContentBlock
        {
            Text = $"""
                {missingOptionsContent.Text}

                - Review the following command spec and identify the required arguments from the input schema.
                - Omit any arguments that are not required or do not apply to your use case.
                - Wrap all command arguments into the root "parameters" argument.
                - If required data is missing infer the data from your context or prompt the user as needed.
                - Run the tool again with the "command" and root "parameters" object.

                Command Spec:
                {childToolJson}
                """
        });
        result.IsError = true;
        return result;
    }

    private static async Task NotifyProgressAsync(
        RequestContext<CallToolRequestParams> request,
        string message,
        CancellationToken cancellationToken)
    {
        var progressToken = request.Params?.ProgressToken;
        if (progressToken == null)
        {
            return;
        }

        await request.Server.NotifyProgressAsync(
            progressToken.Value,
            new ProgressNotificationValue
            {
                Progress = 0f,
                Message = message,
            },
            cancellationToken: cancellationToken);
    }

    private async Task<(string? CommandName, Dictionary<string, JsonElement> Parameters)> GetCommandAndParametersFromIntentAsync(
        RequestContext<CallToolRequestParams> request,
        string intent,
        string namespaceName,
        List<Tool> availableTools,
        CancellationToken cancellationToken)
    {
#pragma warning disable MCP9005 // Sampling APIs remain for backward compatibility during migration.
        await NotifyProgressAsync(request, $"Learning about {namespaceName} capabilities...", cancellationToken);

        var availableToolsJson = JsonSerializer.Serialize(
            availableTools.Select(tool => new ToolCommandInfo(tool)),
            ServerJsonContext.Default.IEnumerableToolCommandInfo);

        var samplingRequest = new CreateMessageRequestParams
        {
            MaxTokens = 1000,
            Messages =
            [
                new SamplingMessage
                {
                    Role = Role.Assistant,
                    Content =
                    [
                        new TextContentBlock
                        {
                            Text = $"""
                                Your task:
                                - Select the single command that best matches the user's intent.
                                - Return a valid JSON object that matches the provided result schema.
                                - Map the user's intent and known parameters to the command's input schema, ensuring parameter names and types match the schema exactly.
                                - Only include parameters that are defined in the selected command's input schema.
                                - Do not guess or invent parameters.
                                - If no command matches, return JSON with an "Unknown" command name.

                                Result Schema:
                                {ToolCallProxySchema}

                                Intent:
                                {intent}

                                Known Parameters:
                                {GetParametersJsonElement(request).GetRawText()}

                                Available Commands:
                                {availableToolsJson}
                                """
                        }
                    ]
                }
            ],
        };

        try
        {
            var samplingResponse = await request.Server.SampleAsync(samplingRequest, cancellationToken);
            var samplingContent = samplingResponse.Content is { Count: > 0 }
                ? samplingResponse.Content[0] as TextContentBlock
                : null;
            var commandCallJson = samplingContent?.Text?.Trim();

            if (!string.IsNullOrEmpty(commandCallJson))
            {
                using var jsonDocument = JsonDocument.Parse(commandCallJson);
                var root = jsonDocument.RootElement;
                var commandName = root.TryGetProperty("command", out var commandElement) && commandElement.ValueKind == JsonValueKind.String
                    ? commandElement.GetString()
                    : null;
                var parameters = root.TryGetProperty("parameters", out var parametersElement) && parametersElement.ValueKind == JsonValueKind.Object
                    ? parametersElement.EnumerateObject().ToDictionary(property => property.Name, property => property.Value.Clone())
                    : [];

                if (!string.IsNullOrEmpty(commandName) && commandName != "Unknown")
                {
                    return (commandName, parameters);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get command and parameters from intent for namespace: {Namespace}", namespaceName);
        }

        return (null, []);
#pragma warning restore MCP9005
    }
}
