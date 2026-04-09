// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Compute.Options;
using Azure.Mcp.Tools.Compute.Options.GalleryApplication;
using Azure.Mcp.Tools.Compute.Services;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.Compute.Commands.GalleryApplication;

public sealed class GalleryApplicationVersionDeleteCommand(ILogger<GalleryApplicationVersionDeleteCommand> logger, IComputeService computeService)
    : BaseComputeCommand<GalleryApplicationVersionDeleteOptions>(true)
{
    private const string CommandTitle = "Delete Gallery Application Version";
    private readonly ILogger<GalleryApplicationVersionDeleteCommand> _logger = logger;
    private readonly IComputeService _computeService = computeService;

    public override string Id => "9c91c935-c02d-4816-91ec-e4876da219d8";

    public override string Name => "delete";

    public override string Description =>
        """
        Delete an Azure Compute Gallery application version under a gallery application. This operation is idempotent and returns deleted=false if the version is not found.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = true,
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
    }

    protected override GalleryApplicationVersionDeleteOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Gallery = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.Gallery.Name);
        options.GalleryApplication = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.GalleryApplication.Name);
        options.GalleryApplicationVersion = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.GalleryApplicationVersion.Name);
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
            var deleted = await _computeService.DeleteGalleryApplicationVersionAsync(
                options.Gallery!,
                options.GalleryApplication!,
                options.GalleryApplicationVersion!,
                options.ResourceGroup!,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new GalleryApplicationVersionDeleteCommandResult(deleted, options.GalleryApplicationVersion!),
                ComputeJsonContext.Default.GalleryApplicationVersionDeleteCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error deleting gallery application version. Gallery: {Gallery}, GalleryApplication: {GalleryApplication}, GalleryApplicationVersion: {GalleryApplicationVersion}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}.",
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
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed deleting gallery application version. Verify your permissions. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    internal record GalleryApplicationVersionDeleteCommandResult(bool Deleted, string GalleryApplicationVersion);
}
