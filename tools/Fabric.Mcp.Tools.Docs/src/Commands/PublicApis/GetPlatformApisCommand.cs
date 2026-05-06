// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization.Metadata;
using Fabric.Mcp.Tools.Docs.Models;
using Fabric.Mcp.Tools.Docs.Options;
using Fabric.Mcp.Tools.Docs.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Fabric.Mcp.Tools.Docs.Commands.PublicApis;

public sealed class GetPlatformApisCommand(ILogger<GetPlatformApisCommand> logger) : GlobalCommand<BaseFabricOptions, FabricWorkloadPublicApi>()
{
    private const string CommandTitle = "Platform API Specification";
    private readonly ILogger<GetPlatformApisCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public override string Id => "2338df97-d6d9-4f1d-9e92-e118efe9c643";

    public override string Name => "platform-api-spec";

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

    protected override JsonTypeInfo<FabricWorkloadPublicApi> ResultTypeInfo => FabricJsonContext.Default.FabricWorkloadPublicApi;

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

            SetResult(context, apis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Fabric platform public APIs");
            HandleException(context, ex);
        }

        return context.Response;
    }
}
