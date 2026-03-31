// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.Docs.Options;
using Fabric.Mcp.Tools.Docs.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Fabric.Mcp.Tools.Docs.Commands.PublicApis;

public sealed class GetPlatformApisCommand(ILogger<GetPlatformApisCommand> logger) : GlobalCommand<BaseFabricOptions>()
{
    private readonly ILogger<GetPlatformApisCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var fabricService = context.GetService<IFabricPublicApiService>();
            var apis = await fabricService.GetWorkloadPublicApis("platform", cancellationToken);

            context.Response.Results = ResponseResult.Create(apis, FabricJsonContext.Default.FabricWorkloadPublicApi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Fabric platform public APIs");
            HandleException(context, ex);
        }

        return context.Response;
    }
}
