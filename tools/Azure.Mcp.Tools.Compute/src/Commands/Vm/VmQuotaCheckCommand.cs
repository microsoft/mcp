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
    Id = "9b1f7d0b-3d1d-4c2a-8a4e-7c2b1d6e0a03",
    Name = "check-quota",
    Title = "Check Virtual Machine Quota",
    Description = """
        Check current Azure Compute vCPU quota usage and limits in a region before attempting a create.
        Returns per-family vCPU usage and limits (e.g., standardDSv5Family, standardNCSv3Family). Use
        --family-prefix (case-insensitive) to narrow results to one family. Set --requested-vcpus to flag
        the result as 'Insufficient' if available quota would not cover the requested vCPU count, and
        'NearLimit' when current usage is above 80% of the limit.
        Use this tool during guided VM/VMSS create flows to fail fast on quota issues rather than waiting
        for the create to return QuotaExceeded. Wraps 'az vm list-usage' patterns via the .NET SDK.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class VmQuotaCheckCommand(ILogger<VmQuotaCheckCommand> logger, IComputeService computeService)
    : BaseComputeCommand<VmQuotaCheckOptions>(false)
{
    private readonly ILogger<VmQuotaCheckCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IComputeService _computeService = computeService ?? throw new ArgumentNullException(nameof(computeService));

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(ComputeOptionDefinitions.Location.AsRequired());
        command.Options.Add(ComputeOptionDefinitions.FamilyPrefix);
        command.Options.Add(ComputeOptionDefinitions.RequestedVCpus);
    }

    protected override VmQuotaCheckOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Location = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.Location.Name);
        options.FamilyPrefix = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.FamilyPrefix.Name);
        options.RequestedVCpus = parseResult.GetValueOrDefault<int?>(ComputeOptionDefinitions.RequestedVCpus.Name);
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

            var quotas = await _computeService.CheckVmQuotaAsync(
                options.Subscription!,
                options.Location!,
                options.FamilyPrefix,
                options.RequestedVCpus,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(quotas), ComputeJsonContext.Default.VmQuotaCheckCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error checking VM quota. Location: {Location}, Subscription: {Subscription}, Family: {Family}.",
                options.Location, options.Subscription, options.FamilyPrefix);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Location or subscription not found. Verify the --location value and your subscription access.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed reading quota. Verify Reader access on the subscription. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    internal record VmQuotaCheckCommandResult(List<VmQuotaInfo> Quotas);
}
