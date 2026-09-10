// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.Docs.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Fabric.Mcp.Tools.Docs.Commands.PublicApis;

[CommandMetadata(
    Id = "b1f80251-df7b-4054-953b-5f452c42dd09",
    Name = "list-item-types",
    Title = "Available Fabric Item Types",
    Description = "Lists the Microsoft Fabric item types that have public API specifications available. Use this when the user needs to discover which Fabric APIs exist. Returns item type names such as notebook, lakehouse, dataPipeline and report, plus a few non-item areas: platform and admin (platform and tenant APIs), spark (workspace Spark settings) and realTimeIntelligence (workload-level copilot APIs).",
    OperationPlane = ToolOperationPlane.NotApplicable,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    LocalRequired = false,
    Secret = false)]
public sealed class ListItemTypesCommand(IFabricPublicApiService service, ILogger<ListItemTypesCommand> logger)
    : AuthenticatedCommand<EmptyOptions, ListItemTypesCommand.ItemTypeListCommandResult>
{
    private readonly ILogger<ListItemTypesCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IFabricPublicApiService _service = service ?? throw new ArgumentNullException(nameof(service));

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, EmptyOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var itemTypes = await _service.ListItemTypesAsync(cancellationToken);

            context.Response.Results = ResponseResult.Create(new(itemTypes), FabricJsonContext.Default.ItemTypeListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching Fabric public item types");
            HandleException(context, ex);
        }

        return context.Response;
    }

    public record ItemTypeListCommandResult(IEnumerable<string> ItemTypes);
}
