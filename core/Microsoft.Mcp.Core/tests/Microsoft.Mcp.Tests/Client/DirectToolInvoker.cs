// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Microsoft.Mcp.Tests.Client;

public sealed class DirectToolInvoker(
    ICommandFactory commandFactory,
    ICommandContextAccessor contextAccessor,
    ILogger<DirectToolInvoker> logger)
{
    public async Task<CommandResponse> InvokeAsync(
        string toolName,
        IDictionary<string, JsonElement> parameters,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        CommandRegistration registration = commandFactory.FindCommandRegistration(toolName)
            ?? throw new KeyNotFoundException($"Tool '{toolName}' is not registered in the selected test area.");
        var command = registration.Command;
        var definition = command.GetCommand();
        var options = definition.Options.Where(option => option.Name != ICommandFactory.LearnOptionName).ToList();
        ParseResult? parsed;
        if (options.Count == 1 &&
            string.Equals(options[0].Name.TrimStart('-'), "raw-mcp-tool-input", StringComparison.OrdinalIgnoreCase))
        {
            parsed = definition.ParseFromRawMcpToolInput(parameters);
        }
        else if (!definition.TryParseFromDictionary(parameters, out parsed, out var error))
        {
            logger.LogWarning("Invalid arguments for test tool {Tool}: {Error}", toolName, error);
            return new CommandResponse { Status = HttpStatusCode.BadRequest, Message = error! };
        }

        var context = new CommandContext { ToolNamespaceName = registration.ToolNamespaceName };
        using (contextAccessor.BeginScope(context))
        {
            return await command.ExecuteAsync(context, parsed!, cancellationToken);
        }
    }
}
