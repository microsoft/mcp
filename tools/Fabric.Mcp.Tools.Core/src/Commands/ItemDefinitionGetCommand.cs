// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json.Serialization.Metadata;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Options;
using Fabric.Mcp.Tools.Core.Services;
using Fabric.Mcp.Tools.Core.Validation;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Fabric.Mcp.Tools.Core.Commands;

[CommandMetadata(
    Id = "b3ce43c2-45bd-43e9-9c17-35a5a0db669b",
    Name = "get-item-definition",
    Title = "Get Fabric Item Definition",
    Description = """
        Gets a Fabric item's typed definition using explicit nonempty workspace-id and item-id UUIDs.
        Optional format is sent to Fabric without a metadata/name lookup. Sends one POST to getDefinition.
        A 200 response returns definition.parts (path, payload, payloadType) and any returned format.
        A 202 response is a resumable operation receipt, not a definition or a saved directory.
        With sync=true, poll and retrieve the result within max-wait-seconds (default 120, maximum 600);
        early-poll permits one probe after at most 3 seconds, then validated Retry-After hints are honored.
        Wait expiry or caller cancellation stops local waiting, not Fabric's operation; no background polling remains.
        Resume with get-operation-state, then when Succeeded use this tool with operation-id alone
        to retrieve that result once with one GET and no POST or polling.
        Do not combine operation-id with workspace-id, item-id, format, or sync=true.
        The operation result does not contain item identity or prove which item it came from.
        Inline retrieval works locally and over HTTP and never creates unsolicited files.
        Optional output-directory exports completed parts using the existing definition file service.
        This requires local execution and a new absolute directory below an existing parent; it is unavailable over HTTP.
        Export preserves binary bytes, encodings, relative paths, empty files, and .platform, without merging or overwriting.
        Export failures are errors, never partial success or silent inline fallback.
        Because optional export writes files, this tool is excluded by read-only server filtering in every transport.
        Supports JSON responses up to 64 MiB, InlineBase64 parts, and the host's generic path/byte/count limits
        (defaults: 1024 parts, 16 MiB decoded per part, 32 MiB decoded total). Oversized content is rejected, not truncated.
        No per-part downloads, schema downloads, item-specific layout validation, dependency lookups, or item updates.
        Requires read AND write item permissions and Item.ReadWrite.All or item-specific ReadWrite.All delegated scope.
        Service-principal and managed-identity support depends on item type. Sensitivity labels are not in definitions;
        encrypted labels require the caller's applicable usage rights. Treat returned definition content as sensitive.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = true,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = true)]
public sealed class ItemDefinitionGetCommand(
    ILogger<ItemDefinitionGetCommand> logger,
    IFabricCoreService fabricCoreService,
    IFabricItemDefinitionExportService exportService) : FabricCoreCommand<ItemDefinitionGetOptions, ItemDefinitionGetCommandResult>
{
    private readonly ILogger<ItemDefinitionGetCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IFabricCoreService _fabricCoreService = fabricCoreService ?? throw new ArgumentNullException(nameof(fabricCoreService));
    private readonly IFabricItemDefinitionExportService _exportService = exportService ?? throw new ArgumentNullException(nameof(exportService));

    public override JsonTypeInfo<ItemDefinitionGetCommandResult> ResultTypeInfo => CoreJsonContext.Default.ItemDefinitionGetCommandResult;

    public override void ValidateOptions(ItemDefinitionGetOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        try
        {
            ItemDefinitionGetInput.Validate(options.WorkspaceId, options.ItemId, options.Format, options.OperationId,
                new(options.Sync, options.MaxWaitSeconds, options.EarlyPoll));
        }
        catch (ArgumentException ex)
        {
            validationResult.Errors.Add(ex.Message);
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, ItemDefinitionGetOptions options, CancellationToken cancellationToken)
    {
        ItemDefinitionGetCommandResult? result = null;
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var waitOptions = new FabricOperationWaitOptions(options.Sync, options.MaxWaitSeconds, options.EarlyPoll);
            ItemDefinitionGetInput.Validate(options.WorkspaceId, options.ItemId, options.Format, options.OperationId, waitOptions);
            var destination = _exportService.ValidateDestination(options.OutputDirectory);
            result = await _fabricCoreService.GetItemDefinitionAsync(
                options.WorkspaceId, options.ItemId, options.Format, options.OperationId,
                waitOptions, cancellationToken);
            if (destination is not null && result.Definition is { } definition)
            {
                var published = await _exportService.WriteAsync(destination, definition, cancellationToken);
                result = result with { OutputDirectory = published };
            }
            SetResult(context, result);
            context.Response.Status = result.Operation switch
            {
                { Status: FabricOperationStatus.Failed } => HttpStatusCode.BadGateway,
                { Status: FabricOperationStatus.TrackingStopped or FabricOperationStatus.ResultUnavailable, Issue.HttpStatus: { } status } => (HttpStatusCode)status,
                { Status: not FabricOperationStatus.Succeeded } => HttpStatusCode.Accepted,
                _ => HttpStatusCode.OK
            };
            context.Response.Message = result.Operation?.Issue?.Message ?? (result.Definition is null
                ? "Definition retrieval is pending. No files were saved. Check get-operation-state, then use get-item-definition with --operation-id when Succeeded."
                : context.Response.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error retrieving or exporting a Fabric item definition ({ExceptionType}).", ex.GetType().Name);
            HandleException(context, ex);
            if (ex is not FabricOperationCanceledException)
            {
                var operation = result?.Operation;
                if (operation is null && Guid.TryParse(options.OperationId, out var operationId) && operationId != Guid.Empty)
                {
                    operation = new(FabricOperationStatus.TrackingStopped, operationId);
                }
                if (operation is not null)
                {
                    // Preserve the resumable reference, but never return definition content
                    // or a saved path as a success-shaped fallback after export/read failure.
                    SetResult(context, new(null, operation with
                    {
                        Issue = new(result?.Definition is null ? "DefinitionReadFailed" : "DefinitionExportFailed",
                            GetErrorMessage(ex), (int)context.Response.Status)
                    }));
                }
            }
        }
        return context.Response;
    }

    protected override void HandleException(CommandContext context, Exception ex)
    {
        base.HandleException(context, ex);
        if (ex is FabricOperationCanceledException canceled)
        {
            SetResult(context, new(null, canceled.Operation));
            context.Response.Message = canceled.Operation.Issue?.Message ?? GetErrorMessage(ex);
        }
    }

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        FabricItemDefinitionExportException export => export.StatusCode,
        OperationCanceledException => HttpStatusCode.RequestTimeout,
        _ => base.GetStatusCode(ex)
    };

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        FabricItemDefinitionExportException => ex.Message,
        FabricOperationException { Code: "UnexpectedResponse", StatusCode: HttpStatusCode.BadGateway } =>
            "Fabric returned an undocumented success status for definition retrieval. No definition was returned or saved",
        FabricOperationException { StatusCode: HttpStatusCode.BadGateway } or FabricThrottledException => ex.Message,
        OperationCanceledException =>
            "Definition retrieval or export was canceled. No completed export was published; the Fabric operation was not canceled. Resume using a known operation ID",
        _ => GetStatusCode(ex) switch
        {
            HttpStatusCode.BadRequest =>
                "Check workspace-id and item-id UUIDs, the optional format, wait options, and operation-id. A resumed result must already be complete",
            HttpStatusCode.Unauthorized =>
                "Authentication failed while retrieving the definition. Check the configured Fabric identity",
            HttpStatusCode.Forbidden =>
                "Access denied. Definition retrieval requires read AND write item permissions, the applicable delegated ReadWrite.All scope, and any encrypted-label usage rights",
            HttpStatusCode.NotFound =>
                "The Fabric item or operation result was not found or is not accessible",
            HttpStatusCode.TooManyRequests =>
                "Fabric throttled definition retrieval. Wait before retrying",
            _ => "Unable to retrieve or export the Fabric item definition. No completed export was published. Check Fabric availability and local export settings; resume using a known operation ID"
        }
    };
}
