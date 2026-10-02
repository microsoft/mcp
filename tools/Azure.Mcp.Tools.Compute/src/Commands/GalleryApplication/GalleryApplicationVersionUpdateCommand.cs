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

public sealed class GalleryApplicationVersionUpdateCommand(ILogger<GalleryApplicationVersionUpdateCommand> logger, IComputeService computeService)
    : BaseComputeCommand<GalleryApplicationVersionUpdateOptions>(true)
{
    private const string CommandTitle = "Update Gallery Application Version";
    private readonly ILogger<GalleryApplicationVersionUpdateCommand> _logger = logger;
    private readonly IComputeService _computeService = computeService;

    public override string Id => "ad0de68f-a4e5-4a7d-8f35-22384d7472b4";

    public override string Name => "update";

    public override string Description =>
        """
        Update an Azure Compute Gallery application version under a gallery application using flattened publishing profile options.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
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
        command.Options.Add(ComputeOptionDefinitions.Location.AsOptional());
        command.Options.Add(ComputeOptionDefinitions.Tags.AsOptional());
        command.Options.Add(ComputeOptionDefinitions.SourceMediaLink.AsOptional());
        command.Options.Add(ComputeOptionDefinitions.ExcludeFromLatest.AsOptional());
        command.Options.Add(ComputeOptionDefinitions.EndOfLifeDate.AsOptional());
        command.Options.Add(ComputeOptionDefinitions.TargetRegions.AsOptional());
    }

    protected override GalleryApplicationVersionUpdateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Gallery = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.Gallery.Name);
        options.GalleryApplication = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.GalleryApplication.Name);
        options.GalleryApplicationVersion = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.GalleryApplicationVersion.Name);
        options.Location = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.Location.Name);
        options.Tags = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.Tags.Name);
        options.SourceMediaLink = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.SourceMediaLink.Name);
        options.ExcludeFromLatest = parseResult.GetValueOrDefault<bool?>(ComputeOptionDefinitions.ExcludeFromLatest.Name);
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
            var result = await _computeService.UpdateGalleryApplicationVersionAsync(
                options.Gallery!,
                options.GalleryApplication!,
                options.GalleryApplicationVersion!,
                options.ResourceGroup!,
                options.Subscription!,
                options.Location,
                options.Tags,
                options.SourceMediaLink,
                options.ExcludeFromLatest,
                options.EndOfLifeDate,
                options.TargetRegions,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(result),
                ComputeJsonContext.Default.GalleryApplicationVersionUpdateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error updating gallery application version. Gallery: {Gallery}, GalleryApplication: {GalleryApplication}, GalleryApplicationVersion: {GalleryApplicationVersion}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}.",
                options.Gallery,
                options.GalleryApplication,
                options.GalleryApplicationVersion,
                options.ResourceGroup,
                options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Gallery application version not found. Verify gallery, application, version name, and permissions.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed updating gallery application version. Verify your permissions. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    internal record GalleryApplicationVersionUpdateCommandResult(GalleryApplicationVersionInfo GalleryApplicationVersion);
}
