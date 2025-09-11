// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Fabric.Mcp.Tools.PublicApi.Options;
using Fabric.Mcp.Tools.PublicApi.Services;
using Microsoft.Extensions.Logging;

namespace Fabric.Mcp.Tools.PublicApi.Commands.PublicApis;

public sealed class GetPlatformApisCommand(ILogger<GetPlatformApisCommand> logger) : GlobalCommand<BaseFabricOptions>()
{
    private const string CommandTitle = "Get Platform API Specification";
    private readonly ILogger<GetPlatformApisCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public override string Name => "get";

    public override string Description =>
        """
        Retrieve the OpenAPI/Swagger specification for Microsoft Fabric platform APIs.
        These are the core platform APIs that work across all Fabric workloads, such as 
        workspace management, authentication, and common resource operations. Returns the 
        complete API specification in JSON format with supplementary definition files.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var fabricService = context.GetService<IFabricPublicApiService>();
            var apis = await fabricService.GetWorkloadPublicApis("platform");

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
