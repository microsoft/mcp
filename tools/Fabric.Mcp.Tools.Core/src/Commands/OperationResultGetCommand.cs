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
    Id = "4e4b0f52-b2dd-4f2a-9cd4-62a2b36ec8ca",
    Name = "get-operation-result",
    Title = "Get Fabric Operation Result",
    Description = """
        Reads the result of one completed Fabric long-running operation by operation-id UUID in a single request.
        Returns a JSON envelope with operationId, hasBody, and value; JSON null differs from an empty body.
        Only JSON and empty results are supported, up to 1 MiB (1,048,576 bytes). Binary results are unsupported.
        Use get-operation-state first to confirm Succeeded. Not every initiating API produces a result;
        a missing result or HTTP 404 does not establish successful no-result completion.
        Requires the same identity, resource permissions, and delegated scopes as the initiating API.
        Does not poll, retry, follow response URLs, download binary data, or resubmit the original mutation.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = true,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false)]
public sealed class OperationResultGetCommand(
    ILogger<OperationResultGetCommand> logger,
    IFabricCoreService fabricCoreService) : FabricCoreCommand<OperationGetOptions, OperationResult>
{
    private readonly ILogger<OperationResultGetCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IFabricCoreService _fabricCoreService = fabricCoreService ?? throw new ArgumentNullException(nameof(fabricCoreService));

    public override JsonTypeInfo<OperationResult> ResultTypeInfo => CoreJsonContext.Default.OperationResult;

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
            SetResult(context, await _fabricCoreService.GetOperationResultAsync(options.OperationId, cancellationToken));
        }
        catch (Exception ex)
        {
            _logger.LogError("Error reading a Fabric operation result ({ExceptionType}).", ex.GetType().Name);
            HandleException(context, ex);
        }
        return context.Response;
    }

    protected override HttpStatusCode GetStatusCode(Exception ex) => FabricOperationErrors.GetStatusCode(ex);

    protected override string GetErrorMessage(Exception ex) => FabricOperationErrors.GetMessage(ex);
}
