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

public sealed class GalleryApplicationVersionGetCommand(ILogger<GalleryApplicationVersionGetCommand> logger, IComputeService computeService)
    : BaseComputeCommand<GalleryApplicationVersionGetOptions>(true)
{
    private const string CommandTitle = "Get Gallery Application Version";
    private readonly ILogger<GalleryApplicationVersionGetCommand> _logger = logger;
    private readonly IComputeService _computeService = computeService;

    public override string Id => "5927fe0e-8ff4-4c6f-af03-cf2f4f5108f0";

    public override string Name => "get";

    public override string Description =>
        """
        Get one gallery application version or list all versions under a gallery application. When --gallery-application-version is provided, returns a single version; otherwise returns all versions under the specified gallery application.
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
        command.Options.Add(ComputeOptionDefinitions.GalleryApplication.AsRequired());
        command.Options.Add(ComputeOptionDefinitions.GalleryApplicationVersion.AsOptional());
    }

    protected override GalleryApplicationVersionGetOptions BindOptions(ParseResult parseResult)
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
            if (!string.IsNullOrEmpty(options.GalleryApplicationVersion))
            {
                var version = await _computeService.GetGalleryApplicationVersionAsync(
                    options.Gallery!,
                    options.GalleryApplication!,
                    options.GalleryApplicationVersion,
                    options.ResourceGroup!,
                    options.Subscription!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(
                    new(version),
                    ComputeJsonContext.Default.GalleryApplicationVersionGetCommandResult);
            }
            else
            {
                var versions = await _computeService.ListGalleryApplicationVersionsAsync(
                    options.Gallery!,
                    options.GalleryApplication!,
                    options.ResourceGroup!,
                    options.Subscription!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(
                    new(versions),
                    ComputeJsonContext.Default.GalleryApplicationVersionGetListResult);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error retrieving gallery application version(s). Gallery: {Gallery}, GalleryApplication: {GalleryApplication}, GalleryApplicationVersion: {GalleryApplicationVersion}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}.",
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
            "Gallery application version not found. Verify gallery, application, version name, resource group, and permissions.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed accessing gallery application version(s). Verify your permissions. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    internal record GalleryApplicationVersionGetCommandResult(GalleryApplicationVersionInfo GalleryApplicationVersion);
    internal record GalleryApplicationVersionGetListResult(List<GalleryApplicationVersionInfo> GalleryApplicationVersions);
}
