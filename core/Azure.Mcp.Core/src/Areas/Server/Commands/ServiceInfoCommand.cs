// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Azure.Mcp.Core.Areas.Server.Commands;

/// <summary>
/// Command that provides basic server information.
/// </summary>
[HiddenCommand]
public sealed class ServiceInfoCommand(IOptions<AzureMcpServerConfiguration> serverOptions, ILogger<ServiceInfoCommand> logger) : BaseCommand<EmptyOptions>
{
    private static readonly EmptyOptions EmptyOptions = new EmptyOptions();

    private readonly IOptions<AzureMcpServerConfiguration> _serverOptions = serverOptions;
    private readonly ILogger<ServiceInfoCommand> _logger = logger;

    public override string Id => "add0f6fe-258c-45c4-af74-0c165d4913cb";

    public override string Name => "info";

    public override string Description => "Displays running MCP server information.";

    public override string Title => "Server information.";

    public override ToolMetadata Metadata => new ToolMetadata
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    public override Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        try
        {
            context.Response.Results = ResponseResult.Create(
                new(
                    _serverOptions.Value.Name,
                    _serverOptions.Value.Version),
                ServiceInfoJsonContext.Default.ServiceInfoCommandResult);
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

    internal record ServiceInfoCommandResult(string Name, string Version);
}
