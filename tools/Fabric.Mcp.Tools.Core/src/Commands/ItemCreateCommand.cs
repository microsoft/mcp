// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json.Serialization.Metadata;
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
        Sends one creation request. A 201 response returns item metadata; 202 returns a resumable operation receipt.
        With sync=true, asynchronously wait up to max-wait-seconds (default 120) after acceptance and retrieve
        the result if available. Both modes immediately return synchronous completion without polling.
        early-poll=true permits one probe after at most 3 seconds instead of the initial 202 Retry-After;
        false honors that hint. Later polls and 429/503 retries honor validated hints.
        Wait expiry returns last-known state, not a failed mutation; client cancellation can stop waiting sooner.
        Description is limited to 256 characters; naming rules depend on item type.
        Requires Contributor or higher workspace access and, for delegated callers, Item.ReadWrite.All or
        the item-specific ReadWrite.All scope. Non-Power BI items require a supported Fabric capacity and
        tenant/capacity settings that enable Fabric item creation; Power BI items require the appropriate license.
        Service-principal and managed-identity support depends on item type.
        Does not supply definitions or creation payloads, cancel Fabric operations, or resubmit creation.
        Only JSON/empty operation responses up to 1 MiB are supported; created item metadata is required.
        Acceptance is not completion. Resume using get-operation-state and get-operation-result, not another create.
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
    IFabricCoreService fabricCoreService) : FabricCoreCommand<ItemCreateOptions, ItemCreateCommandResult>
{
    private readonly ILogger<ItemCreateCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IFabricCoreService _fabricCoreService = fabricCoreService ?? throw new ArgumentNullException(nameof(fabricCoreService));

    public override JsonTypeInfo<ItemCreateCommandResult> ResultTypeInfo => CoreJsonContext.Default.ItemCreateCommandResult;

    public override void ValidateOptions(ItemCreateOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        if (!Guid.TryParse(GetWorkspaceId(options), out var workspaceId) || workspaceId == Guid.Empty)
        {
            validationResult.Errors.Add("Provide a nonempty workspace UUID using --workspace-id or --workspace; workspace names are not supported.");
        }
        if (!FabricOperationWaitOptions.IsValidBudget(options.MaxWaitSeconds))
        {
            validationResult.Errors.Add("--max-wait-seconds must be finite and greater than zero.");
        }
        if (options.Description?.Length > 256)
        {
            validationResult.Errors.Add("--description must be at most 256 characters.");
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

            var result = await _fabricCoreService.CreateItemAsync(workspaceId!, request,
                new(options.Sync, options.MaxWaitSeconds, options.EarlyPoll), cancellationToken);
            SetResult(context, result);
            context.Response.Status = result.Operation switch
            {
                { Status: FabricOperationStatus.Failed } => HttpStatusCode.BadGateway,
                { Status: FabricOperationStatus.TrackingStopped or FabricOperationStatus.ResultUnavailable, Issue.HttpStatus: { } status } => (HttpStatusCode)status,
                { Status: not FabricOperationStatus.Succeeded } => HttpStatusCode.Accepted,
                _ => HttpStatusCode.OK
            };
            if (result.Operation?.Issue is { } issue)
            {
                context.Response.Message = issue.Message;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Error creating a Fabric item ({ExceptionType}).", ex.GetType().Name);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override void HandleException(CommandContext context, Exception ex)
    {
        base.HandleException(context, ex);
        if (ex is FabricOperationCanceledException canceled)
        {
            SetResult(context, new(null, canceled.Operation));
            context.Response.Message = canceled.Message;
        }
        if (ex is CommandValidationException validationException)
        {
            context.Response.Message = GetValidationErrorMessage(validationException,
                "Invalid item creation request. Provide a nonempty workspace UUID, display name, and item type; check option names and values.");
        }
    }

    protected override HttpStatusCode GetStatusCode(Exception ex) =>
        ex is FabricOperationCanceledException ? HttpStatusCode.RequestTimeout : base.GetStatusCode(ex);

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        CredentialUnavailableException =>
            "Fabric credentials are unavailable. Authenticate the configured Fabric identity before creating an item",
        FabricOperationException { StatusCode: HttpStatusCode.BadGateway } => ex.Message,
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
