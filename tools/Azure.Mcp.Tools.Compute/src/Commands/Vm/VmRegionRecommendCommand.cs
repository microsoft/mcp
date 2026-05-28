// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Compute.Models;
using Azure.Mcp.Tools.Compute.Options;
using Azure.Mcp.Tools.Compute.Options.Vm;
using Azure.Mcp.Tools.Compute.Services;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.Compute.Commands.Vm;

[CommandMetadata(
    Id = "9b1f7d0b-3d1d-4c2a-8a4e-7c2b1d6e0a04",
    Name = "recommend-region",
    Title = "Recommend Virtual Machine Region",
    Description = """
        Suggest Azure regions for a new VMSS Flex scale set or single VM based on workload hints, geography
        preference, and Availability Zone requirements. Returns a ranked list of regions with a score and a
        short rationale. Multi-AZ (3+ zone) regions are favored for VMSS Flex / HA workloads — pass
        --workload-hint containing 'scale', 'vmss', 'ha', 'production', or 'prod' (or set
        --require-availability-zones) to nearly triple the AZ weight so zone-rich regions like eastus,
        westus3, and westeurope outrank single-zone regions.
        Use --workload-hint to bias ranking by workload type (e.g., 'vmss production web', 'gpu training',
        'general dev/test', 'database'), --geography-preference to bias by geography substring (e.g.,
        'us', 'europe', 'asia'), and --require-availability-zones to filter out regions without multi-AZ
        support.
        Use this tool during guided VMSS Flex or single-VM create flows so a beginner can be offered a
        sensible default region and an advanced user can compare a few ranked candidates before picking
        one for compute_vmss_create or compute_vm_create.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class VmRegionRecommendCommand(ILogger<VmRegionRecommendCommand> logger, IComputeService computeService)
    : BaseComputeCommand<VmRegionRecommendOptions>(false)
{
    private readonly ILogger<VmRegionRecommendCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IComputeService _computeService = computeService ?? throw new ArgumentNullException(nameof(computeService));

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(ComputeOptionDefinitions.WorkloadHint);
        command.Options.Add(ComputeOptionDefinitions.GeographyPreference);
        command.Options.Add(ComputeOptionDefinitions.RequireAvailabilityZones);
        command.Options.Add(ComputeOptionDefinitions.Top);
    }

    protected override VmRegionRecommendOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.WorkloadHint = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.WorkloadHint.Name);
        options.GeographyPreference = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.GeographyPreference.Name);
        options.RequireAvailabilityZones = parseResult.GetValueOrDefault<bool>(ComputeOptionDefinitions.RequireAvailabilityZones.Name);
        options.Top = parseResult.GetValueOrDefault<int?>(ComputeOptionDefinitions.Top.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            context.Activity?.AddTag("subscription", options.Subscription);

            var regions = await _computeService.RecommendVmRegionsAsync(
                options.Subscription!,
                options.WorkloadHint,
                options.GeographyPreference,
                options.RequireAvailabilityZones,
                options.Top,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(regions), ComputeJsonContext.Default.VmRegionRecommendCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error recommending VM regions. Subscription: {Subscription}, WorkloadHint: {WorkloadHint}, Geography: {Geography}.",
                options.Subscription, options.WorkloadHint, options.GeographyPreference);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed listing locations. Verify Reader access on the subscription. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    internal record VmRegionRecommendCommandResult(List<VmRegionRecommendation> Regions);
}
