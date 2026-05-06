// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization.Metadata;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Configuration;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Models.Command;

namespace Microsoft.Mcp.Core.Areas.Server.Commands;

/// <summary>
/// Command that provides basic server information.
/// </summary>
[HiddenCommand]
[CommandMetadata(
    Id = "add0f6fe-258c-45c4-af74-0c165d4913cb",
    Name = "info",
    Title = "Server information.",
    Description = "Displays running MCP server information.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    LocalRequired = false,
    Secret = false)]
public sealed class ServiceInfoCommand(IOptions<McpServerConfiguration> serverOptions, ILogger<ServiceInfoCommand> logger) : BaseCommand<EmptyOptions, ServiceInfoCommand.ServiceInfoCommandResult>
{
    private static readonly EmptyOptions EmptyOptions = new();

    private readonly IOptions<McpServerConfiguration> _serverOptions = serverOptions;
    private readonly ILogger<ServiceInfoCommand> _logger = logger;

    protected override JsonTypeInfo<ServiceInfoCommandResult> ResultTypeInfo
        => ServiceInfoJsonContext.Default.ServiceInfoCommandResult;

    public override Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        try
        {
            SetResult(context, new ServiceInfoCommandResult(_serverOptions.Value.Name, _serverOptions.Value.Version));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obtaining server information.");
            HandleException(context, ex);
        }

        return Task.FromResult(context.Response);
    }

    protected override EmptyOptions BindOptions(ParseResult parseResult)
    {
        return EmptyOptions;
    }

    public record ServiceInfoCommandResult(string Name, string Version);
}
