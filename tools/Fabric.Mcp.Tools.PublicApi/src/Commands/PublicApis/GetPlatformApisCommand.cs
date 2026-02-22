// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Fabric.Mcp.Tools.PublicApi.Options;
using Fabric.Mcp.Tools.PublicApi.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Fabric.Mcp.Tools.PublicApi.Commands.PublicApis;

public sealed class GetPlatformApisCommand(ILogger<GetPlatformApisCommand> logger) : GlobalCommand<BaseFabricOptions>()
{
    private const string CommandTitle = "Get Platform API Specification";
    private readonly ILogger<GetPlatformApisCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public override string Id => "2338df97-d6d9-4f1d-9e92-e118efe9c643";

    public override string Name => "get_platform_api_spec";

    public override string Description =>
        "Retrieves the OpenAPI specification for core Fabric platform APIs. Use this when the user needs documentation for cross-workload platform APIs like workspace management. Returns complete platform API specification.";

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