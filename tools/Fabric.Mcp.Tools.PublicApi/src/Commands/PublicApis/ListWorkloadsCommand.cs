// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Fabric.Mcp.Tools.PublicApi.Options;
using Fabric.Mcp.Tools.PublicApi.Services;
using Microsoft.Extensions.Logging;

namespace Fabric.Mcp.Tools.PublicApi.Commands.PublicApis;

public sealed class ListWorkloadsCommand(ILogger<ListWorkloadsCommand> logger) : GlobalCommand<BaseFabricOptions>()
{
    private const string CommandTitle = "List Available Fabric Workloads";

    private readonly ILogger<ListWorkloadsCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public override string Name => "list";

    public override string Description =>
        """
        List all Microsoft Fabric workload types that have public API specifications available.
        Returns workload names like 'notebook', 'report', 'platform', etc. that can be used 
        with other commands to retrieve their specific API documentation.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        try
        {
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            {
                return context.Response;
            }

            var fabricService = context.GetService<IFabricPublicApiService>();
            var workloads = await fabricService.ListWorkloadsAsync();

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
