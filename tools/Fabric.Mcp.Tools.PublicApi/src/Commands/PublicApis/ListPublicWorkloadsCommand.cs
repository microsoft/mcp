// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Fabric.Mcp.Tools.PublicApi.Options;
using Fabric.Mcp.Tools.PublicApi.Services;
using Microsoft.Extensions.Logging;

namespace Fabric.Mcp.Tools.PublicApi.Commands.PublicApis;

public sealed class ListPublicWorkloadsCommand(ILogger<ListPublicWorkloadsCommand> logger) : GlobalCommand<BaseFabricOptions>()
{
    private const string CommandTitle = "List Fabric Workloads";

    private readonly ILogger<ListPublicWorkloadsCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public override string Name => "list";

    public override string Description =>
        """
        List all workloads supported by Microsoft Fabric public APIs.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        try
        {
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            {
                return context.Response;
            }

            var fabricService = context.GetService<IFabricPublicApiService>();
            var workloads = await fabricService.ListFabricWorkloadsAsync();

            context.Response.Results = ResponseResult.Create(new ItemListCommandResult(workloads), FabricJsonContext.Default.ItemListCommandResult);
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
