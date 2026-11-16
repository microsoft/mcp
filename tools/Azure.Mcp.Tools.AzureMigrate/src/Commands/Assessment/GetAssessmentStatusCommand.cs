// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.AzureMigrate.Commands.Inventory;
using Azure.Mcp.Tools.AzureMigrate.Options.Assessment;
using Microsoft.AspNetCore.Http;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AzureMigrate.Commands.Assessment;

/// <summary>
/// POC: returns the current status of an assessment operation with one call. It does not wait or
/// poll. When the assessment has succeeded, it also returns a temporary read-only URL of the recommendations JSON.
/// </summary>
[CommandMetadata(
    Id = "d5ff4998-2934-4f4b-b805-87c8a1c1f540",
    Name = "status",
    Title = "Get Azure Migrate Assessment Status",
    Description = "Gets the current status of an Azure Migrate assessment started by the assessment create tool. Call this once when " +
        "the user asks about the progress of their assessment or needs a new link to download it; do not loop or wait. Requires " +
        "'operation-id' from the latest assessment create result in the conversation (ask the user to create an assessment if " +
        "there is none); 'assessment-name' is optional. Returns 'status' (Running, Succeeded, Failed or Canceled), 'isCompleted', " +
        "'assessmentDownloadCommand' and 'instructions'. Follow 'instructions'. Do not mention tool or parameter names to the user.",
    OperationPlane = ToolOperationPlane.NotApplicable,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class GetAssessmentStatusCommand(IAssessmentOperationStatusClient client, IHttpContextAccessor httpContextAccessor)
    : BaseCommand<GetAssessmentStatusOptions, GetAssessmentStatusCommand.GetAssessmentStatusCommandResult>
{
    private readonly IAssessmentOperationStatusClient _client = client;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    /// <inheritdoc/>
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, GetAssessmentStatusOptions options, CancellationToken cancellationToken)
    {
        GetAssessmentStatusCommandResult result = await RunAsync(options, cancellationToken);

        context.Response.Status = HttpStatusCode.OK;
        context.Response.Results = ResponseResult.Create(result, AzureMigrateJsonContext.Default.GetAssessmentStatusCommandResult);
        context.Response.Message = string.Empty;
        return context.Response;
    }

    internal async Task<GetAssessmentStatusCommandResult> RunAsync(GetAssessmentStatusOptions options, CancellationToken cancellationToken)
    {
        string? name = string.IsNullOrWhiteSpace(options.AssessmentName) ? null : options.AssessmentName.Trim();
        string? operationIdText = options.OperationId?.Trim();
        if (!Guid.TryParse(operationIdText, out Guid operationId))
        {
            return Fail(name, operationIdText, null, "'operation-id' is required and must be a GUID.");
        }

        (string? token, string? authError) = DsBackdoorClient.GetBearerToken(_httpContextAccessor);
        if (token is null)
        {
            return Fail(name, operationIdText, null, authError);
        }

        // TODO: Pass the caller's workspace id once it is defined; the Business Case Service contract requires it.
        AssessmentOperation operation = await _client.GetOperationAsync(operationId, workspaceId: null, token, cancellationToken);
        return Parse(name, operationId.ToString("D"), operation);
    }

    internal static GetAssessmentStatusCommandResult Parse(string? name, string operationId, AssessmentOperation operation)
    {
        if (operation.HttpStatus == (int)HttpStatusCode.NotFound)
        {
            return Fail(name, operationId, operation.HttpStatus, "No assessment operation was found for this operation ID.");
        }

        if (operation.HttpStatus is not (>= 200 and < 300) || string.IsNullOrWhiteSpace(operation.Status))
        {
            return Fail(name, operationId, operation.HttpStatus,
                $"Assessment status check failed (HTTP {operation.HttpStatus}): {operation.ErrorCode} {operation.ErrorMessage}".TrimEnd());
        }

        bool isCompleted = CreateAssessmentCommand.IsTerminal(operation.Status);
        bool succeeded = string.Equals(operation.Status, "Succeeded", StringComparison.OrdinalIgnoreCase);
        string? outputUrl = succeeded ? operation.OutputUrl : null;
        string? downloadCommand = ExportInventoryCommand.BuildDownloadCommand(outputUrl);
        return new(
            Success: true,
            Error: null,
            HttpStatus: operation.HttpStatus,
            AssessmentName: name,
            OperationId: operationId,
            Status: operation.Status,
            IsCompleted: isCompleted,
            ErrorCode: operation.ErrorCode,
            ErrorMessage: operation.ErrorMessage,
            RetryAfterSeconds: isCompleted ? null : operation.RetryAfterSeconds ?? 30,
            AssessmentOutputUrl: outputUrl,
            AssessmentDownloadCommand: downloadCommand,
            Instructions: !isCompleted
                ? "Tell the user the assessment is still running and suggest checking again in about a minute."
                : succeeded
                    ? "Tell the user the assessment has completed. " + CreateAssessmentCommand.DownloadInstructions
                    : "Tell the user the assessment did not complete and quote 'errorMessage' if present.");
    }

    private static GetAssessmentStatusCommandResult Fail(string? name, string? operationId, int? status, string? error) =>
        new(
            Success: false,
            Error: error ?? "Unknown error.",
            HttpStatus: status,
            AssessmentName: name,
            OperationId: operationId,
            Status: null,
            IsCompleted: false,
            ErrorCode: null,
            ErrorMessage: null,
            RetryAfterSeconds: null,
            AssessmentOutputUrl: null,
            AssessmentDownloadCommand: null,
            Instructions: null);

    /// <summary>Result of the assessment status command.</summary>
    public sealed record GetAssessmentStatusCommandResult(
        bool Success,
        string? Error,
        int? HttpStatus,
        string? AssessmentName,
        string? OperationId,
        string? Status,
        bool IsCompleted,
        string? ErrorCode,
        string? ErrorMessage,
        int? RetryAfterSeconds,
        string? AssessmentOutputUrl,
        string? AssessmentDownloadCommand,
        string? Instructions);
}