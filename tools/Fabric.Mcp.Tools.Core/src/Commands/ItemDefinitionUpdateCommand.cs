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
    Id = "e15155f3-c5e4-423b-9d88-ce4e8d701a4f",
    Name = "update-item-definition",
    Title = "Update Fabric Item Definition",
    Description = """
        Replaces the entire definition of an existing Fabric item using known nonempty workspace-id and item-id UUIDs.
        Requires exactly one complete definition source: inline definition JSON, or an existing local
        definition-directory with an explicit definition-files mapping and optional definition-format.
        Saved-file input reads only selected saved files on the hosting machine; HTTP callers must use inline content.
        This is not a changed-files patch: it never gets the old definition, merges parts, or resolves names.
        Shared validation checks InlineBase64 payloads, portable paths, and bounded byte/count budgets.
        Optional definition-item-type is only a local validation hint, never the API identity or a metadata lookup.
        It enables existing minimum checks for recognized/default formats of Notebook, DataPipeline, SemanticModel, and Report;
        other types and explicit unrecognized formats receive generic validation rather than guessed format rules.
        Fabric validates actual item-type requirements, content, dependencies, and item-specific options.
        Optional top-level options is bounded JSON for the item's Update Definition API, not LRO wait settings.
        update-metadata must be explicitly true to request metadata updates from a supplied .platform part.
        Omission leaves that query parameter omitted. .platform is never added, removed, or rewritten.
        Updating a definition does not affect the sensitivity label; no label, folder, or creation properties are accepted.
        Sends one POST. A synchronous 200 returns a Succeeded receipt with no item/result body.
        A 202 returns a resumable operation receipt. With sync=true, asynchronously poll state for up to
        max-wait-seconds (default 120, maximum 600); confirmed Succeeded completes without a /result or Get Item call.
        early-poll=true permits one probe after at most 3 seconds instead of the initial Retry-After.
        Later polls and 429/503 read retries honor validated hints. Wait expiry preserves last-known state.
        Client cancellation stops only the local wait, not the Fabric operation.
        Resume pending updates with get-operation-state, never by resubmitting the update or requesting a result.
        Requires read and write item permissions and, for delegated callers, Item.ReadWrite.All or the
        item-specific ReadWrite.All scope. Service-principal and managed-identity support depends on item type.
        Does not perform preflight reads, download schemas, create dependencies, or follow arbitrary URLs.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = true,
    Idempotent = true,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false)]
public sealed class ItemDefinitionUpdateCommand(
    ILogger<ItemDefinitionUpdateCommand> logger,
    IFabricCoreService fabricCoreService,
    IFabricItemDefinitionInputService definitionInputService) : FabricCoreCommand<ItemDefinitionUpdateOptions, ItemDefinitionUpdateCommandResult>
{
    private readonly ILogger<ItemDefinitionUpdateCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IFabricCoreService _fabricCoreService = fabricCoreService ?? throw new ArgumentNullException(nameof(fabricCoreService));
    private readonly IFabricItemDefinitionInputService _definitionInputService = definitionInputService ?? throw new ArgumentNullException(nameof(definitionInputService));

    public override JsonTypeInfo<ItemDefinitionUpdateCommandResult> ResultTypeInfo => CoreJsonContext.Default.ItemDefinitionUpdateCommandResult;

    public override void ValidateOptions(ItemDefinitionUpdateOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        try
        {
            ValidateInput(options);
        }
        catch (FabricItemInputException ex)
        {
            validationResult.Errors.Add(ex.Message);
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, ItemDefinitionUpdateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            ValidateInput(options);
            // Parse workload options before any file access. They remain opaque to item-specific validation.
            var apiOptions = FabricItemJsonInput.ParseObject(options.Options, "--options");
            var input = options.DefinitionInput;
            var definition = await _definitionInputService.ReadAsync(options.DefinitionItemType, input?.Definition,
                input?.DefinitionDirectory, input?.DefinitionFiles, input?.DefinitionFormat, cancellationToken) ??
                throw new FabricItemInputException("Provide a complete replacement definition using exactly one inline or saved-file source.");
            var result = await _fabricCoreService.UpdateItemDefinitionAsync(options.WorkspaceId, options.ItemId,
                new(definition, apiOptions), options.UpdateMetadata,
                new(options.Sync, options.MaxWaitSeconds, options.EarlyPoll), cancellationToken);

            SetResult(context, result);
            context.Response.Status = result.Operation switch
            {
                { Status: FabricOperationStatus.Succeeded } => HttpStatusCode.OK,
                { Status: FabricOperationStatus.Failed } => HttpStatusCode.BadGateway,
                { Status: FabricOperationStatus.TrackingStopped or FabricOperationStatus.ResultUnavailable, Issue.HttpStatus: { } status } => (HttpStatusCode)status,
                _ => HttpStatusCode.Accepted
            };
            if (result.Operation.Issue is { } issue)
            {
                context.Response.Message = issue.Message;
            }
        }
        catch (Exception ex) when (ex is FabricOperationCanceledException || ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            _logger.LogError("Error updating a Fabric item definition ({ExceptionType}).", ex.GetType().Name);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override void HandleException(CommandContext context, Exception ex)
    {
        base.HandleException(context, ex);
        if (ex is FabricOperationCanceledException canceled)
        {
            SetResult(context, new(canceled.Operation));
            context.Response.Message = canceled.Message;
        }
        if (ex is CommandValidationException validationException)
        {
            context.Response.Message = GetValidationErrorMessage(validationException,
                "Invalid definition update request. Provide nonempty workspace and item UUIDs, exactly one complete definition source, " +
                "and valid options; --update-metadata requires an explicit true or false.");
        }
    }

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        FabricItemDefinitionInputException or FabricItemInputException => HttpStatusCode.BadRequest,
        FabricOperationCanceledException => HttpStatusCode.RequestTimeout,
        _ => base.GetStatusCode(ex)
    };

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        FabricItemDefinitionInputException or FabricItemInputException => ex.Message,
        CredentialUnavailableException or AuthenticationFailedException =>
            "Fabric authentication failed. Authenticate the configured identity and check its access to the item",
        FabricOperationException { StatusCode: HttpStatusCode.BadGateway } or FabricThrottledException => ex.Message,
        OperationCanceledException or TimeoutException =>
            "The definition update timed out or was canceled. Check the item's state before retrying; Fabric may still complete the update",
        _ => GetStatusCode(ex) switch
        {
            HttpStatusCode.BadRequest =>
                "Fabric rejected the definition update. Check the workspace and item UUIDs, complete definition, format, and item-specific options",
            HttpStatusCode.Unauthorized =>
                "Authentication failed. Check the configured Fabric identity and its access to the item",
            HttpStatusCode.Forbidden =>
                "Fabric denied the definition update. Read and write item permissions are required; delegated callers need Item.ReadWrite.All " +
                "or the item-specific scope. Service-principal and managed-identity support depends on item type",
            HttpStatusCode.NotFound =>
                "The Fabric workspace or item was not found, or the caller does not have access",
            HttpStatusCode.Conflict =>
                "The definition update conflicts with the item's current state. Check the item before retrying",
            HttpStatusCode.TooManyRequests =>
                "Fabric throttled the definition update. No automatic mutation retry was attempted",
            _ =>
                "The Fabric definition update could not be confirmed. Check the item's state before retrying; do not automatically resubmit"
        }
    };

    private static void ValidateInput(ItemDefinitionUpdateOptions options)
    {
        if (!Guid.TryParse(options.WorkspaceId, out var workspaceId) || workspaceId == Guid.Empty ||
            !Guid.TryParse(options.ItemId, out var itemId) || itemId == Guid.Empty)
        {
            throw new FabricItemInputException("--workspace-id and --item-id must be nonempty UUIDs.");
        }
        if (!FabricOperationWaitOptions.IsValidBudget(options.MaxWaitSeconds))
        {
            throw new FabricItemInputException(FormattableString.Invariant(
                $"--max-wait-seconds must be finite, greater than zero, and at most {FabricOperationWaitOptions.MaximumWaitSeconds} seconds."));
        }
        if (options.DefinitionItemType is not null && string.IsNullOrWhiteSpace(options.DefinitionItemType))
        {
            throw new FabricItemInputException("Omit --definition-item-type for generic checks, or supply a nonempty local validation hint.");
        }
        if (options.DefinitionInput?.HasInput is not true)
        {
            throw new FabricItemInputException("Provide a complete replacement definition using exactly one inline or saved-file source.");
        }
    }
}
