// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Compute.Models;
using Azure.Mcp.Tools.Compute.Options.Gallery;
using Azure.Mcp.Tools.Compute.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Compute.Commands.Gallery;

/// <summary>
/// Command to create an Azure Compute Gallery.
/// </summary>
[CommandMetadata(
    Id = "7c4e1f2a-9b3d-4e5a-8c61-2d0f3a5b7e91",
    Name = "create",
    Title = "Create Azure Compute Gallery",
    Description = """
        Creates an Azure Compute Gallery (formerly Shared Image Gallery) in the specified resource group.
        A gallery is the top-level container used to store and share VM image definitions and VM application
        definitions across subscriptions, regions, and tenants. Create a gallery before publishing VM images
        or VM applications. If --location is not specified, the gallery is created in the resource group's
        location. This uses create-or-update semantics: if a gallery with the same name already exists, values
        that are not supplied are left unchanged, but supplying --tags replaces the gallery's entire tag set
        rather than merging, so include every tag that should be kept. Returns the
        created gallery including its name, resource ID, location, description, globally unique name,
        sharing permissions, tags, and provisioning state.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class GalleryCreateCommand(ILogger<GalleryCreateCommand> logger, IComputeService computeService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<GalleryCreateOptions, GalleryCreateCommand.GalleryCreateCommandResult>(subscriptionResolver)
{
    private const int MaxGalleryNameLength = 80;

    private readonly ILogger<GalleryCreateCommand> _logger = logger;
    private readonly IComputeService _computeService = computeService;

    public override void ValidateOptions(GalleryCreateOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (!IsValidGalleryName(options.Gallery))
        {
            validationResult.Errors.Add(
                $"Invalid gallery name '{options.Gallery}'. Gallery names must be 1-{MaxGalleryNameLength} characters, " +
                "contain only letters, digits, periods, and underscores, and start and end with a letter or digit. " +
                "Hyphens are not allowed in gallery names.");
        }
    }

    // Unlike most Microsoft.Compute resources, galleries disallow hyphens and must end with an alphanumeric.
    internal static bool IsValidGalleryName(string? gallery)
    {
        if (string.IsNullOrEmpty(gallery) || gallery.Length > MaxGalleryNameLength)
        {
            return false;
        }

        if (!char.IsAsciiLetterOrDigit(gallery[0]) || !char.IsAsciiLetterOrDigit(gallery[^1]))
        {
            return false;
        }

        foreach (var c in gallery)
        {
            if (!char.IsAsciiLetterOrDigit(c) && c is not '.' and not '_')
            {
                return false;
            }
        }

        return true;
    }

    internal static Dictionary<string, string>? ParseTags(string? tags)
    {
        if (string.IsNullOrWhiteSpace(tags))
        {
            return null;
        }

        var parsed = new Dictionary<string, string>();
        foreach (var pair in tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var parts = pair.Split('=', 2);
            if (parts.Length != 2)
            {
                throw new ArgumentException($"Invalid tag '{pair}'. Tags must use the format 'key=value'.");
            }

            var key = parts[0].Trim();
            var value = parts[1].Trim();

            if (key.Length == 0)
            {
                throw new ArgumentException($"Invalid tag '{pair}'. Tag keys cannot be empty.");
            }

            if (value.Length == 0)
            {
                throw new ArgumentException($"Invalid tag '{pair}'. Tag values cannot be empty.");
            }

            parsed[key] = value;
        }

        return parsed.Count > 0 ? parsed : null;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, GalleryCreateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            context.Activity?.AddTag("subscription", options.Subscription);

            // Parse tags before any Azure call so malformed input fails fast without creating a resource.
            var tags = ParseTags(options.Tags);

            var gallery = await _computeService.CreateGalleryAsync(
                options.Gallery,
                options.ResourceGroup,
                options.Subscription!,
                options.Location,
                options.Description,
                tags,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(gallery), ComputeJsonContext.Default.GalleryCreateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error creating gallery. Gallery: {Gallery}, ResourceGroup: {ResourceGroup}, Location: {Location}.",
                options.Gallery, options.ResourceGroup, options.Location ?? "(default)");
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        RequestFailedException reqEx => (HttpStatusCode)reqEx.Status,
        Identity.AuthenticationFailedException => HttpStatusCode.Unauthorized,
        ArgumentException => HttpStatusCode.BadRequest,
        _ => base.GetStatusCode(ex)
    };

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Resource group not found. Verify the resource group exists and you have access.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed creating the gallery. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            $"Conflict creating the gallery. Another operation may be in progress on this gallery. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        Identity.AuthenticationFailedException =>
            "Authentication failed. Please run 'az login' to sign in.",
        ArgumentException argEx => $"Invalid parameter: {argEx.Message}",
        _ => base.GetErrorMessage(ex)
    };

    /// <summary>
    /// Result record for the gallery create command.
    /// </summary>
    public record GalleryCreateCommandResult(GalleryInfo Gallery);
}
