// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Globalization;
using System.Net;
using System.Text.Json.Serialization.Metadata;
using Azure.Identity;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Options;
using Fabric.Mcp.Tools.Core.Services;
using Fabric.Mcp.Tools.Core.Validation;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Fabric.Mcp.Tools.Core.Commands;

[CommandMetadata(
    Id = "47a1843f-948a-4778-aeaf-803046e7cc45",
    Name = "update-item",
    Title = "Update Fabric Item",
    Description = """
        Updates an existing Fabric item's display name or description and returns its metadata.
        Requires workspace-id and item-id UUIDs and at least one of display-name or description.
        Omitted properties remain unchanged; an empty description clears it. Descriptions allow at most 256 characters;
        display-name rules depend on item type. Does not change item definitions, data, permissions, tags, or identity.
        Requires read and write permission on the item. Delegated callers need Item.ReadWrite.All or the corresponding
        item-specific scope; service-principal and managed-identity support depends on item type.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = true,
    Idempotent = true,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false)]
public sealed class ItemUpdateCommand(
    ILogger<ItemUpdateCommand> logger,
    IFabricCoreService fabricCoreService) : FabricCoreCommand<ItemUpdateOptions, ItemUpdateCommandResult>
{
    private readonly ILogger<ItemUpdateCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IFabricCoreService _fabricCoreService = fabricCoreService ?? throw new ArgumentNullException(nameof(fabricCoreService));

    public override JsonTypeInfo<ItemUpdateCommandResult> ResultTypeInfo => CoreJsonContext.Default.ItemUpdateCommandResult;

    public override void ValidateOptions(ItemUpdateOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        ItemUpdateInputValidator.Validate(options.WorkspaceId, options.ItemId, options.DisplayName, options.Description, validationResult);
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ItemUpdateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _fabricCoreService.UpdateItemAsync(
                options.WorkspaceId,
                options.ItemId,
                new(options.DisplayName, options.Description),
                cancellationToken);

            SetResult(context, new(item));
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            _logger.LogError("Error updating Fabric item. Status: {StatusCode}.", (int)GetStatusCode(ex));
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        InvalidDataException => HttpStatusCode.BadGateway,
        InvalidOperationException => HttpStatusCode.InternalServerError,
        OperationCanceledException => HttpStatusCode.GatewayTimeout,
        _ => base.GetStatusCode(ex)
    };

    protected override string GetErrorMessage(Exception ex)
    {
        var message = ex switch
        {
            AuthenticationFailedException =>
                "Authentication failed. Check the configured Fabric identity and its access to the item.",
            ArgumentException =>
                "Invalid update request. Provide nonempty workspace and item UUIDs and at least one valid display name or description.",
            InvalidDataException =>
                "Fabric returned an invalid update response. The update may have completed; verify the item's state before retrying.",
            HttpRequestException { StatusCode: HttpStatusCode.BadRequest } =>
                "Fabric rejected the update. Check the display-name rules for the item's type and the description's 256-character limit.",
            HttpRequestException { StatusCode: HttpStatusCode.Unauthorized } =>
                "Authentication failed. Check the configured Fabric identity and its access to the item.",
            HttpRequestException { StatusCode: HttpStatusCode.Forbidden } =>
                "Access denied. The caller needs read and write permission on the item. Delegated callers need Item.ReadWrite.All or the item-specific scope. Service-principal and managed-identity support depends on item type.",
            HttpRequestException { StatusCode: HttpStatusCode.NotFound } =>
                "The Fabric workspace or item was not found, or the caller does not have access.",
            HttpRequestException { StatusCode: HttpStatusCode.Conflict } =>
                "The update conflicts with an existing item name or state. Verify the item before retrying.",
            HttpRequestException { StatusCode: HttpStatusCode.TooManyRequests } =>
                "Fabric throttled the update. No automatic retry was attempted.",
            HttpRequestException { StatusCode: { } statusCode } =>
                $"Fabric returned HTTP {(int)statusCode}. Verify the item's state before retrying.",
            HttpRequestException =>
                "The Fabric update could not be completed because of a network error. Verify the item's state before retrying.",
            OperationCanceledException or TimeoutException =>
                "The Fabric update timed out. Verify the item's state before retrying.",
            _ =>
                "The Fabric update could not be completed. Verify the item's state before retrying."
        };

        if (ex is ItemUpdateRequestException { RetryAfterSeconds: { } seconds })
        {
            message += $" Wait at least {seconds.ToString(CultureInfo.InvariantCulture)} seconds before another request.";
        }

        return message.TrimEnd('.');
    }
}
