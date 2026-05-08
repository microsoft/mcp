// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.Docs.Options;
using Fabric.Mcp.Tools.Docs.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Fabric.Mcp.Tools.Docs.Commands.PublicApis;

[CommandMetadata(
    Id = "b1f80251-df7b-4054-953b-5f452c42dd09",
    Name = "workloads",
    Title = "Available Fabric Workloads",
    Description = "Lists Fabric workload types that have public API specifications available. Use this when the user needs to discover what APIs exist for Fabric workloads. Returns workload names like notebook, report, or platform.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    LocalRequired = false,
    Secret = false)]
public sealed class ListWorkloadsCommand(IFabricPublicApiService service, ILogger<ListWorkloadsCommand> logger)
    : GlobalCommand<BaseFabricOptions>()
{
    private readonly ILogger<ListWorkloadsCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IFabricPublicApiService _service = service ?? throw new ArgumentNullException(nameof(service));

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        try
        {
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            {
                return context.Response;
            }

            var workloads = await _service.ListWorkloadsAsync(cancellationToken);

            context.Response.Results = ResponseResult.Create(new(workloads), FabricJsonContext.Default.ItemListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching Fabric public workloads");
            HandleException(context, ex);
        }

        return context.Response;
    }

    public record ItemListCommandResult(IEnumerable<string> Workloads);
}
