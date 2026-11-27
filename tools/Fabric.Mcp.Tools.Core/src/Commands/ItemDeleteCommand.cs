// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json.Serialization.Metadata;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Options;
using Fabric.Mcp.Tools.Core.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Fabric.Mcp.Tools.Core.Commands;

[CommandMetadata(
    Id = "fc3e35e8-bdf8-4080-919e-94372f4e656a",
    Name = "delete-item",
    Title = "Delete Fabric Item",
    Description = """
        Deletes one Microsoft Fabric item using its required workspace UUID and item UUID.
        Omit hard-delete or explicitly pass false to use Fabric's default deletion behavior:
        soft deletion only for supported item types, without a guarantee of recovery.
        Explicitly pass hard-delete true to permanently delete the item; permanent deletion cannot be recovered.
        Soft deletion requires item write permission, and permanent deletion requires workspace Admin.
        Delegated callers need Item.ReadWrite.All or the item-specific write scope.
        Returns only the requested workspace ID, item ID, and hard-delete mode after a successful response.
        Does not resolve names, infer item type, retry, or fall back to permanent deletion.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = true,
    Idempotent = true,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false)]
public sealed class ItemDeleteCommand(
    ILogger<ItemDeleteCommand> logger,
    IFabricCoreService fabricCoreService) : FabricCoreCommand<ItemDeleteOptions, ItemDeleteCommandResult>
{
    private readonly ILogger<ItemDeleteCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IFabricCoreService _fabricCoreService = fabricCoreService ?? throw new ArgumentNullException(nameof(fabricCoreService));

    public override JsonTypeInfo<ItemDeleteCommandResult> ResultTypeInfo => CoreJsonContext.Default.ItemDeleteCommandResult;

    public override void ValidateOptions(ItemDeleteOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (!Guid.TryParse(options.WorkspaceId, out var workspaceId) || workspaceId == Guid.Empty)
        {
            validationResult.Errors.Add("--workspace-id must be a nonempty UUID.");
        }

        if (!Guid.TryParse(options.ItemId, out var itemId) || itemId == Guid.Empty)
        {
            validationResult.Errors.Add("--item-id must be a nonempty UUID.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ItemDeleteOptions options, CancellationToken cancellationToken)
    {
        try
        {
            await _fabricCoreService.DeleteItemAsync(options.WorkspaceId, options.ItemId, options.HardDelete, cancellationToken);

            SetResult(context, new(
                Guid.Parse(options.WorkspaceId),
                Guid.Parse(options.ItemId),
                options.HardDelete is true));
        }
        catch (Exception ex)
        {
            _logger.LogError("Error deleting Fabric item ({ExceptionType}).", ex.GetType().Name);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        OperationCanceledException => HttpStatusCode.RequestTimeout,
        _ => base.GetStatusCode(ex)
    };

    protected override string GetErrorMessage(Exception ex) => GetStatusCode(ex) switch
    {
        HttpStatusCode.BadRequest when ex is ArgumentException => "Provide nonempty workspace and item UUIDs and an explicit true or false value when supplying hard-delete",
        HttpStatusCode.Unauthorized => "Authentication failed. Use an identity authorized to delete the Fabric item",
        HttpStatusCode.Forbidden => "Access denied. Soft deletion needs item write permission; permanent deletion needs workspace Admin. Delegated callers also need Item.ReadWrite.All or the item-specific write scope",
        HttpStatusCode.NotFound => "The Fabric item was not found. Check its workspace ID, item ID, and the caller's access. Deletion was not confirmed",
        HttpStatusCode.BadRequest or HttpStatusCode.Conflict or HttpStatusCode.UnprocessableEntity => "Fabric rejected item deletion. Check the item state, item-type soft-deletion support, and tenant settings. No fallback to permanent deletion was attempted",
        HttpStatusCode.TooManyRequests when ex is FabricItemDeleteThrottledException => ex.Message,
        HttpStatusCode.TooManyRequests => "Fabric throttled the deletion request. Wait before retrying; the request was not automatically retried",
        HttpStatusCode.RequestTimeout or HttpStatusCode.GatewayTimeout => "The Fabric item deletion request was canceled or timed out. The item's deletion state was not confirmed",
        HttpStatusCode.BadGateway => "Fabric returned an unexpected item deletion response. Deletion was not confirmed",
        _ => "Unable to confirm Fabric item deletion. Check permissions, item state, and service availability"
    };
}
