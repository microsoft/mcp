// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Compute.Models;
using Azure.Mcp.Tools.Compute.Options.PlacementScore;
using Azure.Mcp.Tools.Compute.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Compute.Commands.PlacementScore;

public sealed class SpotPlacementMetadataCommand(
    ILogger<SpotPlacementMetadataCommand> logger,
    IComputePlacementService placementService)
    : BaseComputePlacementCommand<BaseComputePlacementOptions>()
{
    private const string CommandTitle = "Get Spot Placement Scores Metadata";
    private readonly ILogger<SpotPlacementMetadataCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IComputePlacementService _placementService = placementService ?? throw new ArgumentNullException(nameof(placementService));

    public override string Id => "3dc42585-4e1c-44ef-8651-598595f46f9d";

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
            var metadata = await _placementService.GetSpotPlacementMetadataAsync(
                options.Location!,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(metadata),
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
