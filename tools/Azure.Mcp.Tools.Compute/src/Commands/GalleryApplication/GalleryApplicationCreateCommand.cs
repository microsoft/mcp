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

public sealed class GalleryApplicationCreateCommand(ILogger<GalleryApplicationCreateCommand> logger, IComputeService computeService)
    : BaseComputeCommand<GalleryApplicationCreateOptions>(true)
{
    private const string CommandTitle = "Create Gallery Application";
    private readonly ILogger<GalleryApplicationCreateCommand> _logger = logger;
    private readonly IComputeService _computeService = computeService;

    public override string Id => "e6dbb5f8-22c7-4f85-93d8-18a2ad26722f";

    public override string Name => "create";

    public override string Description =>
        """
        Create or update an Azure Compute Gallery application in a gallery. This operation calls GalleryApplicationCollection.CreateOrUpdate and returns the resulting gallery application.
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
        command.Options.Add(ComputeOptionDefinitions.Location.AsRequired());
        command.Options.Add(ComputeOptionDefinitions.Tags.AsOptional());
    }

    protected override GalleryApplicationCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Gallery = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.Gallery.Name);
        options.GalleryApplication = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.GalleryApplication.Name);
        options.Location = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.Location.Name);
        options.Tags = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.Tags.Name);
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
            var result = await _computeService.CreateGalleryApplicationAsync(
                options.Gallery!,
                options.GalleryApplication!,
                options.ResourceGroup!,
                options.Subscription!,
                options.Location!,
                options.Tags,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(result),
                ComputeJsonContext.Default.GalleryApplicationCreateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error creating gallery application. Gallery: {Gallery}, GalleryApplication: {GalleryApplication}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}.",
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
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed creating the gallery application. Verify your permissions. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            $"A gallery application with this name already exists or has a conflicting state. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    internal record GalleryApplicationCreateCommandResult(GalleryApplicationInfo GalleryApplication);
}
