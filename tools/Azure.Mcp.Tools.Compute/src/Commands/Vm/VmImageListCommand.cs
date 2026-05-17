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
    Id = "9b1f7d0b-3d1d-4c2a-8a4e-7c2b1d6e0a02",
    Name = "list-images",
    Title = "List Virtual Machine Images",
    Description = """
        Discover Azure VM images that can be used as --image for compute_vm_create / compute_vmss_create.
        Three modes:
        (1) No image filters: returns the alias catalog (e.g., 'Ubuntu2404', 'Win2022Datacenter') with the
            marketplace URN each alias currently maps to — useful for a beginner picker.
        (2) Single --alias: resolves that alias to its URN.
        (3) --publisher + --offer + --image-sku: lists concrete image versions from the marketplace catalog
            in --location (e.g., publisher='Canonical', offer='ubuntu-24_04-lts', image-sku='server').
        Use this tool during guided VM/VMSS create flows so the user can pick a real, valid image instead
        of guessing a URN. compute_vm_create accepts both aliases and URNs.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class VmImageListCommand(ILogger<VmImageListCommand> logger, IComputeService computeService)
    : BaseComputeCommand<VmImageListOptions>(false)
{
    private readonly ILogger<VmImageListCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IComputeService _computeService = computeService ?? throw new ArgumentNullException(nameof(computeService));

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(ComputeOptionDefinitions.Location.AsRequired());
        command.Options.Add(ComputeOptionDefinitions.Alias);
        command.Options.Add(ComputeOptionDefinitions.Publisher);
        command.Options.Add(ComputeOptionDefinitions.Offer);
        command.Options.Add(ComputeOptionDefinitions.ImageSku);
        command.Options.Add(ComputeOptionDefinitions.Top);
    }

    protected override VmImageListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Location = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.Location.Name);
        options.Alias = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.Alias.Name);
        options.Publisher = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.Publisher.Name);
        options.Offer = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.Offer.Name);
        options.Sku = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.ImageSku.Name);
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
            context.Activity?.AddTag("location", options.Location);

            var images = await _computeService.ListVmImagesAsync(
                options.Subscription!,
                options.Location!,
                options.Alias,
                options.Publisher,
                options.Offer,
                options.Sku,
                options.Top,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(images), ComputeJsonContext.Default.VmImageListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error listing VM images. Location: {Location}, Subscription: {Subscription}, Publisher: {Publisher}, Offer: {Offer}, Sku: {Sku}.",
                options.Location, options.Subscription, options.Publisher, options.Offer, options.Sku);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Image, publisher, offer, or location not found. Verify the values match the marketplace catalog for that region.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed listing VM images. Verify Reader access on the subscription. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    internal record VmImageListCommandResult(List<VmImageInfo> Images);
}
