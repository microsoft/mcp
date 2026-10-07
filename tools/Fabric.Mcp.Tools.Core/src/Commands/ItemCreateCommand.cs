// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Identity;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Options;
using Fabric.Mcp.Tools.Core.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Fabric.Mcp.Tools.Core.Commands;

[CommandMetadata(
    Id = "bfdfd3c0-4551-4454-a930-5bf5b1ad5690",
    Name = "create-item",
    Title = "Create Fabric Item",
    Description = """
        Creates a Fabric item using a nonempty workspace UUID, display-name, and item-type.
        Supply workspace-id or the backward-compatible workspace UUID alias; workspace names are not resolved.
        Sends the display name, item type, and optional description in one request and returns item metadata
        when Fabric responds synchronously. Description is limited to 256 characters; naming rules depend on item type.
        Requires Contributor or higher workspace access and, for delegated callers, Item.ReadWrite.All or
        the item-specific ReadWrite.All scope. Non-Power BI items require a supported Fabric capacity and
        tenant/capacity settings that enable Fabric item creation; Power BI items require the appropriate license.
        Service-principal and managed-identity support depends on item type.
        Does not supply definitions or creation payloads, poll asynchronous creation, or retry automatically.
        An accepted asynchronous request is not confirmation that creation completed.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = false,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false)]
public sealed class ItemCreateCommand(
    ILogger<ItemCreateCommand> logger,
    IFabricCoreService fabricCoreService) : AuthenticatedCommand<ItemCreateOptions, ItemCreateCommandResult>
{
    private readonly ILogger<ItemCreateCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IFabricCoreService _fabricCoreService = fabricCoreService ?? throw new ArgumentNullException(nameof(fabricCoreService));

    public override void ValidateOptions(ItemCreateOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        if (!Guid.TryParse(GetWorkspaceId(options), out var workspaceId) || workspaceId == Guid.Empty)
        {
            validationResult.Errors.Add("Provide a nonempty workspace UUID using --workspace-id or --workspace; workspace names are not supported.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ItemCreateOptions options, CancellationToken cancellationToken)
    {
        var workspaceId = GetWorkspaceId(options);

        try
        {
            var request = new CreateItemRequest
            {
                DisplayName = options.DisplayName,
                Type = options.ItemType,
                Description = options.Description
            };

            var item = await _fabricCoreService.CreateItemAsync(workspaceId!, request, cancellationToken);

            _logger.LogInformation("Successfully created {ItemType} '{DisplayName}' in workspace {WorkspaceId}",
                options.ItemType, options.DisplayName, workspaceId);

            context.Response.Results = ResponseResult.Create(new(item), CoreJsonContext.Default.ItemCreateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating item '{DisplayName}' in workspace {WorkspaceId}.",
                options.DisplayName, workspaceId);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override void HandleException(CommandContext context, Exception ex)
    {
        base.HandleException(context, ex);
        context.Response.Results = null;
        if (ex is CommandValidationException)
        {
            context.Response.Message = "Invalid item creation request. Provide a nonempty workspace UUID, display name, and item type; check option names and values.";
        }
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        CredentialUnavailableException =>
            "Fabric credentials are unavailable. Authenticate the configured Fabric identity before creating an item",
        HttpRequestException { StatusCode: null } =>
            "Service unavailable or network connectivity issues prevented Fabric item creation. Check whether the item was created before retrying",
        OperationCanceledException or TimeoutException =>
            "Fabric item creation timed out or was canceled. Check whether the item was created before retrying",
        _ => GetStatusCode(ex) switch
        {
            HttpStatusCode.BadRequest =>
                "Fabric rejected the item creation request. Check the workspace UUID, item type, display name, and description",
            HttpStatusCode.Unauthorized =>
                "Authentication failed while creating the Fabric item. Check the configured Fabric identity and its access to the workspace",
            HttpStatusCode.Forbidden =>
                "Fabric denied item creation. Check workspace permissions and whether the item type is supported and enabled for the tenant and capacity",
            HttpStatusCode.NotFound =>
                "The Fabric workspace was not found, or the caller does not have access",
            HttpStatusCode.Conflict =>
                "Item creation conflicts with an existing item name or workspace state. Check for an existing item before retrying",
            HttpStatusCode.TooManyRequests =>
                "Fabric throttled item creation or reached a capacity limit. Retry the request later",
            _ when ex is HttpRequestException { StatusCode: { } statusCode } =>
                $"Fabric item creation failed with HTTP {(int)statusCode}. Check whether the item was created before retrying",
            _ =>
                "Fabric item creation could not be completed. Check whether the item was created before retrying"
        }
    };

    private static string? GetWorkspaceId(ItemCreateOptions options) =>
        !string.IsNullOrWhiteSpace(options.WorkspaceId) ? options.WorkspaceId : options.Workspace;
}
