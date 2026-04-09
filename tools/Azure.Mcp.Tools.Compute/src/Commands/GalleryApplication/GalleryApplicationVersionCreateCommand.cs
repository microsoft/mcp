// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Compute.Models;
using Azure.Mcp.Tools.Compute.Options;
using Azure.Mcp.Tools.Compute.Options.GalleryApplication;
using Azure.Mcp.Tools.Compute.Services;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.Compute.Commands.GalleryApplication;

public sealed class GalleryApplicationVersionCreateCommand(ILogger<GalleryApplicationVersionCreateCommand> logger, IComputeService computeService)
    : BaseComputeCommand<GalleryApplicationVersionCreateOptions>(true)
{
    private const string CommandTitle = "Create Gallery Application Version";
    private readonly ILogger<GalleryApplicationVersionCreateCommand> _logger = logger;
    private readonly IComputeService _computeService = computeService;

    public override string Id => "6a91c4c2-88f6-4e80-aed4-05f728f355ec";

    public override string Name => "create";

    public override string Description =>
        """
        Create an Azure Compute Gallery application version under a gallery application using flattened publishing profile options.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = true,
        Idempotent = false,
        OpenWorld = false,
        ReadOnly = false,
        LocalRequired = false,
        Secret = true
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(ComputeOptionDefinitions.Gallery.AsRequired());
        command.Options.Add(ComputeOptionDefinitions.GalleryApplication.AsRequired());
        command.Options.Add(ComputeOptionDefinitions.GalleryApplicationVersion.AsRequired());
        command.Options.Add(ComputeOptionDefinitions.Location.AsRequired());
        command.Options.Add(ComputeOptionDefinitions.Tags.AsOptional());
        command.Options.Add(ComputeOptionDefinitions.SourceMediaLink.AsRequired());
        command.Options.Add(ComputeOptionDefinitions.DefaultConfigurationLink.AsOptional());
        command.Options.Add(ComputeOptionDefinitions.ReplicaCount.AsOptional());
        command.Options.Add(ComputeOptionDefinitions.ExcludeFromLatest.AsOptional());
        command.Options.Add(ComputeOptionDefinitions.ManageActionInstall.AsOptional());
        command.Options.Add(ComputeOptionDefinitions.ManageActionRemove.AsOptional());
        command.Options.Add(ComputeOptionDefinitions.ManageActionUpdate.AsOptional());
        command.Options.Add(ComputeOptionDefinitions.PackageFileName.AsOptional());
        command.Options.Add(ComputeOptionDefinitions.ConfigFileName.AsOptional());
        command.Options.Add(ComputeOptionDefinitions.ScriptBehaviorAfterReboot.AsOptional());
        command.Options.Add(ComputeOptionDefinitions.EndOfLifeDate.AsOptional());
        command.Options.Add(ComputeOptionDefinitions.TargetRegions.AsOptional());
    }

    protected override GalleryApplicationVersionCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Gallery = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.Gallery.Name);
        options.GalleryApplication = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.GalleryApplication.Name);
        options.GalleryApplicationVersion = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.GalleryApplicationVersion.Name);
        options.Location = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.Location.Name);
        options.Tags = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.Tags.Name);
        options.SourceMediaLink = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.SourceMediaLink.Name);
        options.DefaultConfigurationLink = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.DefaultConfigurationLink.Name);
        options.ReplicaCount = parseResult.GetValueOrDefault<int?>(ComputeOptionDefinitions.ReplicaCount.Name);
        options.ExcludeFromLatest = parseResult.GetValueOrDefault<bool?>(ComputeOptionDefinitions.ExcludeFromLatest.Name);
        options.ManageActionInstall = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.ManageActionInstall.Name);
        options.ManageActionRemove = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.ManageActionRemove.Name);
        options.ManageActionUpdate = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.ManageActionUpdate.Name);
        options.PackageFileName = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.PackageFileName.Name);
        options.ConfigFileName = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.ConfigFileName.Name);
        options.ScriptBehaviorAfterReboot = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.ScriptBehaviorAfterReboot.Name);
        options.EndOfLifeDate = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.EndOfLifeDate.Name);
        options.TargetRegions = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.TargetRegions.Name);
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
            var result = await _computeService.CreateGalleryApplicationVersionAsync(
                options.Gallery!,
                options.GalleryApplication!,
                options.GalleryApplicationVersion!,
                options.ResourceGroup!,
                options.Subscription!,
                options.Location!,
                options.Tags,
                options.SourceMediaLink!,
                options.DefaultConfigurationLink,
                options.ReplicaCount,
                options.ExcludeFromLatest,
                options.ManageActionInstall,
                options.ManageActionRemove,
                options.ManageActionUpdate,
                options.PackageFileName,
                options.ConfigFileName,
                options.ScriptBehaviorAfterReboot,
                options.EndOfLifeDate,
                options.TargetRegions,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(result),
                ComputeJsonContext.Default.GalleryApplicationVersionCreateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error creating gallery application version. Gallery: {Gallery}, GalleryApplication: {GalleryApplication}, GalleryApplicationVersion: {GalleryApplicationVersion}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}.",
                options.Gallery,
                options.GalleryApplication,
                options.GalleryApplicationVersion,
                options.ResourceGroup,
                options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record GalleryApplicationVersionCreateCommandResult(GalleryApplicationVersionInfo GalleryApplicationVersion);
}
