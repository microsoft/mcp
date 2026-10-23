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
    Id = "a5207176-a6ac-4bc9-b588-d00a68b675d8",
    Name = "get-operation-state",
    Title = "Get Fabric Operation State",
    Description = """
        Reads the state of one Fabric long-running operation by operation-id UUID in a single request.
        Returns the status, optional progress/timestamps, safe failure code/request ID, and polling guidance.
        A successful state read can report a Failed operation; unknown future statuses are not completion.
        After Succeeded, use get-operation-result only if the initiating API produces a result.
        Requires the same identity, resource permissions, and delegated scopes as the initiating API.
        Does not poll, retry, cancel operations, follow response URLs, or resubmit the original mutation.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = true,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false)]
public sealed class OperationStateGetCommand(
    ILogger<OperationStateGetCommand> logger,
    IFabricCoreService fabricCoreService) : FabricCoreCommand<OperationGetOptions, OperationStateResult>
{
    private readonly ILogger<OperationStateGetCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IFabricCoreService _fabricCoreService = fabricCoreService ?? throw new ArgumentNullException(nameof(fabricCoreService));

    public override JsonTypeInfo<OperationStateResult> ResultTypeInfo => CoreJsonContext.Default.OperationStateResult;

    public override void ValidateOptions(OperationGetOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        if (!Guid.TryParse(options.OperationId, out var id) || id == Guid.Empty)
        {
            validationResult.Errors.Add("--operation-id must be a nonempty UUID.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, OperationGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            SetResult(context, await _fabricCoreService.GetOperationStateAsync(options.OperationId, cancellationToken));
        }
        catch (Exception ex)
        {
            _logger.LogError("Error reading Fabric operation state ({ExceptionType}).", ex.GetType().Name);
            HandleException(context, ex);
        }
        return context.Response;
    }

    protected override HttpStatusCode GetStatusCode(Exception ex) => FabricOperationErrors.GetStatusCode(ex);

    protected override string GetErrorMessage(Exception ex) => FabricOperationErrors.GetMessage(ex);
}
