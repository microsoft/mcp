// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
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
    Description = "Creates a new item in a Fabric workspace. Use this when the user wants to create a Lakehouse, Notebook, or other Fabric item type. Requires workspace ID, item name, and item type.",
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
        if (string.IsNullOrWhiteSpace(options.WorkspaceId) && string.IsNullOrWhiteSpace(options.Workspace))
        {
            validationResult.Errors.Add("Workspace identifier is required. Provide --workspace or --workspace-id.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ItemCreateOptions options, CancellationToken cancellationToken)
    {
        var workspaceId = !string.IsNullOrWhiteSpace(options.WorkspaceId) ? options.WorkspaceId : options.Workspace!;

        try
        {
            var request = new CreateItemRequest
            {
                DisplayName = options.DisplayName,
                Type = options.ItemType,
                Description = options.Description
            };

            var item = await _fabricCoreService.CreateItemAsync(workspaceId, request, cancellationToken);

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

    protected override string GetErrorMessage(Exception ex)
    {
        if (ex is not HttpRequestException { StatusCode: { } statusCode })
        {
            return base.GetErrorMessage(ex);
        }

        var message = statusCode switch
        {
            HttpStatusCode.BadRequest =>
                "Fabric rejected the item creation request. Check the item type, display name, and description.",
            HttpStatusCode.Unauthorized =>
                "Authentication failed while creating the Fabric item. Check the configured Fabric identity and its access to the workspace.",
            HttpStatusCode.Forbidden =>
                "Fabric denied item creation. Check workspace permissions and whether the item type is supported and enabled for the tenant and capacity.",
            HttpStatusCode.NotFound =>
                "The Fabric workspace was not found, or the caller does not have access.",
            HttpStatusCode.Conflict =>
                "Item creation conflicts with an existing item name or workspace state. Check for an existing item before retrying.",
            HttpStatusCode.TooManyRequests =>
                "Fabric throttled item creation or reached a capacity limit. Retry the request later.",
            _ =>
                $"Fabric item creation failed with HTTP {(int)statusCode}."
        };

        return $"{message} Details: {ex.Message}";
    }
}
