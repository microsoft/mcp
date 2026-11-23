// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization.Metadata;
using Azure.Mcp.Tools.Support.Models;
using Azure.Mcp.Tools.Support.Options.Service;
using Azure.Mcp.Tools.Support.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Support.Commands.Service;

[CommandMetadata(
    Id = "7e3b453e-8923-44f8-b9c2-bd5397d03801",
    Name = "list",
    Title = "List Azure Support Services",
    Description = """
        List the Azure services that support tickets can be opened for. Returns each service's name (ID), display
        name, and the Azure resource types it covers. Use this to find the service for a problem, then list its
        problem classifications to get the problem classification ID required to create a support ticket.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class ServiceListCommand(ILogger<ServiceListCommand> logger, ISupportService supportService)
    : AuthenticatedCommand<ServiceListOptions, ServiceListCommand.ServiceListCommandResult>
{
    private readonly ILogger<ServiceListCommand> _logger = logger;
    private readonly ISupportService _supportService = supportService;

    public override JsonTypeInfo<ServiceListCommandResult>? ResultTypeInfo => SupportJsonContext.Default.ServiceListCommandResult;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ServiceListOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var services = await _supportService.ListServicesAsync(options.Tenant, cancellationToken);
            SetResult(context, new(services));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing Azure support services.");
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record ServiceListCommandResult(List<SupportServiceInfo> Services);
}
