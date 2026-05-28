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
    Id = "9b1f7d0b-3d1d-4c2a-8a4e-7c2b1d6e0a01",
    Name = "list-skus",
    Title = "List Virtual Machine SKUs",
    Description = """
        Discover available Azure Virtual Machine SKUs (sizes) in a region with optional pricing.
        Returns a filtered catalog of VM SKUs including vCPU count, memory, GPUs, accelerated networking support,
        availability zones, and SKU-level restrictions. Use --min-vcpus, --min-memory-gb, and --family-prefix
        (e.g., 'Standard_D' for general-purpose, 'Standard_N' for GPU, 'Standard_E' for memory-optimized,
        'Standard_F' for compute-optimized, 'Standard_B' for burstable) to narrow the catalog before showing
        choices to the user. Set --include-pricing to augment results with pay-as-you-go and spot hourly rates
        from the Azure Retail Prices API (adds network latency).
        Use this tool during guided VM/VMSS create flows to present a real, filtered list of candidates
        before calling compute_vm_create or compute_vmss_create.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class VmSkuListCommand(ILogger<VmSkuListCommand> logger, IComputeService computeService)
    : BaseComputeCommand<VmSkuListOptions>(false)
{
    private readonly ILogger<VmSkuListCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IComputeService _computeService = computeService ?? throw new ArgumentNullException(nameof(computeService));

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(ComputeOptionDefinitions.Location.AsRequired());
        command.Options.Add(ComputeOptionDefinitions.MinVCpus);
        command.Options.Add(ComputeOptionDefinitions.MinMemoryGb);
        command.Options.Add(ComputeOptionDefinitions.FamilyPrefix);
        command.Options.Add(ComputeOptionDefinitions.Top);
        command.Options.Add(ComputeOptionDefinitions.IncludePricing);
    }

    protected override VmSkuListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Location = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.Location.Name);
        options.MinVCpus = parseResult.GetValueOrDefault<int?>(ComputeOptionDefinitions.MinVCpus.Name);
        options.MinMemoryGb = parseResult.GetValueOrDefault<double?>(ComputeOptionDefinitions.MinMemoryGb.Name);
        options.FamilyPrefix = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.FamilyPrefix.Name);
        options.Top = parseResult.GetValueOrDefault<int?>(ComputeOptionDefinitions.Top.Name);
        options.IncludePricing = parseResult.GetValueOrDefault<bool>(ComputeOptionDefinitions.IncludePricing.Name);
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
            context.Activity?.AddTag("location", options.Location);

            var skus = await _computeService.ListVmSkusAsync(
                options.Subscription!,
                options.Location!,
                options.MinVCpus,
                options.MinMemoryGb,
                options.FamilyPrefix,
                options.Top,
                options.IncludePricing,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(skus), ComputeJsonContext.Default.VmSkuListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error listing VM SKUs. Location: {Location}, Subscription: {Subscription}.",
                options.Location, options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Location or subscription not found. Verify the --location value and your subscription access.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed listing VM SKUs. Verify Reader access on the subscription. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    internal record VmSkuListCommandResult(List<VmSkuInfo> Skus);
}
