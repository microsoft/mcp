// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Compute.Models;
using Azure.Mcp.Tools.Compute.Options.PlacementScore;
using Azure.Mcp.Tools.Compute.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Compute.Commands.PlacementScore;

public sealed class SpotPlacementScoreCommand(ILogger<SpotPlacementScoreCommand> logger)
    : BaseComputePlacementCommand<SpotPlacementScoreOptions>()
{
    private const string CommandTitle = "Generate Spot Placement Scores";
    private readonly ILogger<SpotPlacementScoreCommand> _logger = logger;

    public override string Id => "b2c3d4e5-f6a7-8901-bcde-f12345678901";

    public override string Name => "generate";

    public override string Description =>
        """
        Generates placement scores for Spot VM SKUs across desired regions and availability zones, indicating
        the likelihood of successful allocation. Returns a score of High, Medium, or Low for each SKU/region/zone
        combination, along with quota availability information. Use this tool to evaluate where Spot VMs are most
        likely to be successfully deployed. Rate limited to 4 calls per 60 minutes per subscription.
        Requires a subscription with Reader role, a location for API routing, desired regions to evaluate,
        and VM SKU sizes to check (e.g., 'Standard_D2_v2').
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

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(ComputePlacementOptionDefinitions.DesiredLocations);
        command.Options.Add(ComputePlacementOptionDefinitions.DesiredSizes);
        command.Options.Add(ComputePlacementOptionDefinitions.DesiredCount);
        command.Options.Add(ComputePlacementOptionDefinitions.AvailabilityZones);
    }

    protected override SpotPlacementScoreOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.DesiredLocations = parseResult.GetValueOrDefault<string[]>(ComputePlacementOptionDefinitions.DesiredLocations.Name);
        options.DesiredSizes = parseResult.GetValueOrDefault<string[]>(ComputePlacementOptionDefinitions.DesiredSizes.Name);
        options.DesiredCount = parseResult.GetValueOrDefault<int>(ComputePlacementOptionDefinitions.DesiredCount.Name);
        options.AvailabilityZones = parseResult.GetValueOrDefault<bool>(ComputePlacementOptionDefinitions.AvailabilityZones.Name);
        return options;
    }

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

            var scores = await service.GetSpotPlacementScoresAsync(
                options.Location!,
                options.Subscription!,
                options.DesiredLocations!,
                options.DesiredSizes!,
                options.DesiredCount,
                options.AvailabilityZones,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new SpotPlacementScoreCommandResult(scores),
                ComputePlacementJsonContext.Default.SpotPlacementScoreCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error generating spot placement scores. Location: {Location}, Subscription: {Subscription}, Options: {@Options}",
                options.Location, options.Subscription, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record SpotPlacementScoreCommandResult(List<PlacementScoreInfo> Scores);
}
