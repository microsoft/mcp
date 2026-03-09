// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Compute.Models;
using Azure.Mcp.Tools.Compute.Options.PlacementScore;
using Azure.Mcp.Tools.Compute.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Compute.Commands.PlacementScore;

public sealed class SpotPlacementMetadataCommand(ILogger<SpotPlacementMetadataCommand> logger)
    : BaseComputePlacementCommand<SpotPlacementMetadataOptions>()
{
    private const string CommandTitle = "Get Spot Placement Scores Metadata";
    private readonly ILogger<SpotPlacementMetadataCommand> _logger = logger;

    public override string Id => "a1b2c3d4-e5f6-7890-abcd-ef1234567890";

    public override string Name => "get";

    public override string Description =>
        """
        Gets Spot Placement Scores metadata for a given location, including the list of supported resource types.
        Use this tool to discover what resource types are supported for spot placement score evaluation in a specific Azure region.
        Requires a subscription with Reader role and a location (e.g., 'eastus').
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

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        ParseResult parseResult,
        CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var service = context.GetService<IComputePlacementService>();

            var metadata = await service.GetSpotPlacementMetadataAsync(
                options.Location!,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new SpotPlacementMetadataCommandResult(metadata),
                ComputePlacementJsonContext.Default.SpotPlacementMetadataCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting spot placement metadata. Location: {Location}, Subscription: {Subscription}",
                options.Location, options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record SpotPlacementMetadataCommandResult(SpotPlacementMetadataInfo Metadata);
}
