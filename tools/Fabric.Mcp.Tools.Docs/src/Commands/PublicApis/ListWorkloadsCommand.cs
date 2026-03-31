// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.Docs.Options;
using Fabric.Mcp.Tools.Docs.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Fabric.Mcp.Tools.Docs.Commands.PublicApis;

public sealed class ListWorkloadsCommand(ILogger<ListWorkloadsCommand> logger) : GlobalCommand<BaseFabricOptions>()
{

    private readonly ILogger<ListWorkloadsCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        try
        {
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            {
                return context.Response;
            }

            var fabricService = context.GetService<IFabricPublicApiService>();
            var workloads = await fabricService.ListWorkloadsAsync(cancellationToken);

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
