// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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
    Id = "bfdfd3c0-4551-4454-a930-5bf5b1ad5690",
    Name = "create-item",
    Title = "Create Fabric Item",
    Description = """
        Creates a Fabric item using a nonempty workspace UUID, display-name, and item-type.
        Supply workspace-id or the backward-compatible workspace UUID alias; workspace names are not resolved.
        Optional folder-id is an existing folder UUID, passed without lookup or folder creation.
        Supply bounded item-specific creation-payload JSON or a definition, never both.
        Optionally supply inline definition JSON, or in local execution a definition-directory and explicit
        definition-files mapping with optional definition-format. Never combine these sources.
        File input reads saved, explicitly selected files only; HTTP callers must supply inline content.
        Definitions support InlineBase64 parts, portable paths, and bounded byte/count budgets.
        Definition format, safe part names, and content are forwarded unchanged after generic validation.
        Fabric validates item-specific layouts, required files, supported formats, content semantics, and dependencies.
        Top-level options JSON requires a definition and contains item-specific settings, not LRO wait settings.
        Optional sensitivity-label-settings JSON requires a nonempty labelId UUID and accepts optional sensitivityLabelApplyStrategy.
        Omitted label settings or strategy stay omitted; Fabric defaults the strategy to ApplyOrFail.
        Ignore is explicit opt-in to proceed without the label if it cannot be applied; no policy downgrade or retry is performed.
        Sends one creation request. A 201 response returns item metadata; 202 returns a resumable operation receipt.
        With sync=true, asynchronously wait up to max-wait-seconds (default 120, server maximum 600 seconds)
        after acceptance and retrieve the result if available. Both modes immediately return synchronous completion without polling.
        early-poll=true permits one probe after at most 3 seconds instead of the initial 202 Retry-After;
        false honors that hint. Later polls and 429/503 retries honor validated hints.
        Wait expiry returns last-known state, not a failed mutation; client cancellation can stop waiting sooner.
        Description is limited to 256 characters; naming rules depend on item type.
        Requires Contributor or higher workspace access and, for delegated callers, Item.ReadWrite.All or
        the item-specific ReadWrite.All scope. Non-Power BI items require a supported Fabric capacity and
        tenant/capacity settings that enable Fabric item creation; Power BI items require the appropriate license.
        Service-principal and managed-identity support depends on item type.
        Does not create dependencies, cancel Fabric operations, or resubmit creation.
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
    IFabricCoreService fabricCoreService,
    IFabricItemDefinitionInputService definitionInputService) : FabricCoreCommand<ItemCreateOptions, ItemCreateCommandResult>
{
    private readonly ILogger<ItemCreateCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IFabricCoreService _fabricCoreService = fabricCoreService ?? throw new ArgumentNullException(nameof(fabricCoreService));
    private readonly IFabricItemDefinitionInputService _definitionInputService = definitionInputService ?? throw new ArgumentNullException(nameof(definitionInputService));

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
            validationResult.Errors.Add(FormattableString.Invariant(
                $"--max-wait-seconds must be finite, greater than zero, and at most {FabricOperationWaitOptions.MaximumWaitSeconds} seconds."));
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
            var request = ItemCreateRequestInput.CreateRequest(options, cancellationToken);
            var definitionInput = options.DefinitionInput;
            request.Definition = await _definitionInputService.ReadAsync(definitionInput?.Definition,
                definitionInput?.DefinitionDirectory, definitionInput?.DefinitionFiles, definitionInput?.DefinitionFormat, cancellationToken);

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
        ex switch
        {
            FabricItemDefinitionInputException or ItemCreateInputException => HttpStatusCode.BadRequest,
            FabricOperationCanceledException => HttpStatusCode.RequestTimeout,
            _ => base.GetStatusCode(ex)
        };

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        FabricItemDefinitionInputException or ItemCreateInputException => ex.Message,
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
                "Fabric rejected the item creation request. Check the workspace UUID, item type, display name, and description; " +
                "also check any definition, creation-payload, folder-id, options, or sensitivity-label-settings",
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
