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

public sealed class GalleryApplicationGetCommand(ILogger<GalleryApplicationGetCommand> logger, IComputeService computeService)
    : BaseComputeCommand<GalleryApplicationGetOptions>(true)
{
    private const string CommandTitle = "Get Gallery Application";
    private readonly ILogger<GalleryApplicationGetCommand> _logger = logger;
    private readonly IComputeService _computeService = computeService;

    public override string Id => "f3b35f65-d5c0-4d40-8f4d-e5a6e325b5a2";

    public override string Name => "get";

    public override string Description =>
        """
        Get one or more Azure Compute Gallery applications from a gallery in a resource group. When --gallery-application is provided, returns a single application. When --gallery-application is omitted, returns all applications in the specified gallery.
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
        command.Options.Add(ComputeOptionDefinitions.Gallery.AsRequired());
        command.Options.Add(ComputeOptionDefinitions.GalleryApplication.AsOptional());
    }

    protected override GalleryApplicationGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Gallery = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.Gallery.Name);
        options.GalleryApplication = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.GalleryApplication.Name);
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
            if (!string.IsNullOrEmpty(options.GalleryApplication))
            {
                var galleryApplication = await _computeService.GetGalleryApplicationAsync(
                    options.Gallery!,
                    options.GalleryApplication,
                    options.ResourceGroup!,
                    options.Subscription!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(
                    new(galleryApplication),
                    ComputeJsonContext.Default.GalleryApplicationGetCommandResult);
            }
            else
            {
                var galleryApplications = await _computeService.ListGalleryApplicationsAsync(
                    options.Gallery!,
                    options.ResourceGroup!,
                    options.Subscription!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(
                    new(galleryApplications),
                    ComputeJsonContext.Default.GalleryApplicationGetListResult);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error retrieving gallery application. Gallery: {Gallery}, GalleryApplication: {GalleryApplication}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}.",
                options.Gallery,
                options.GalleryApplication,
                options.ResourceGroup,
                options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Gallery application not found. Verify the gallery name, application name, resource group, and access permissions.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed accessing the gallery application. Verify your permissions. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    internal record GalleryApplicationGetCommandResult(GalleryApplicationInfo GalleryApplication);
    internal record GalleryApplicationGetListResult(List<GalleryApplicationInfo> GalleryApplications);
}
