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
/// POC: returns the current status of an assessment tracking operation with one call (currently mocked). It does not wait or poll.
/// </summary>
[CommandMetadata(
    Id = "d5ff4998-2934-4f4b-b805-87c8a1c1f540",
    Name = "status",
    Title = "Get Azure Migrate Assessment Status",
    Description = "Gets the current status of an Azure Migrate assessment started by the assessment create tool. Call this once when " +
        "the user asks about the progress of their assessment; do not loop or wait. Requires 'assessment-name' and 'operation-id' " +
        "from the latest assessment create result in the conversation (ask the user to create an assessment if there is none). " +
        "Returns 'status' (for example Running, Succeeded or Failed), 'isCompleted' and 'instructions'. Follow 'instructions'. " +
        "Do not mention tool or parameter names to the user.",
    OperationPlane = ToolOperationPlane.NotApplicable,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class GetAssessmentStatusCommand(IAssessmentServiceClient client, IHttpContextAccessor httpContextAccessor)
    : BaseCommand<GetAssessmentStatusOptions, GetAssessmentStatusCommand.GetAssessmentStatusCommandResult>
{
    private static readonly string[] s_terminalStatuses = ["Succeeded", "Failed", "Canceled", "Cancelled"];

    private readonly IAssessmentServiceClient _client = client;
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
        string? name = options.AssessmentName?.Trim();
        string? operationIdText = options.OperationId?.Trim();
        if (!CreateAssessmentCommand.IsValidAssessmentName(name))
        {
            return Fail(name, operationIdText, null, "'assessment-name' is required and must be the name returned when the assessment was created.");
        }

        if (!Guid.TryParse(operationIdText, out Guid operationId))
        {
            return Fail(name, operationIdText, null, "'operation-id' is required and must be a GUID.");
        }

        (string? token, string? authError) = DsBackdoorClient.GetBearerToken(_httpContextAccessor);
        if (token is null)
        {
            return Fail(name, operationIdText, null, authError);
        }

        AssessmentOperation operation = await _client.GetOperationAsync(name!, operationId, token, cancellationToken);
        return Parse(name!, operationId.ToString("D"), operation);
    }

    internal static GetAssessmentStatusCommandResult Parse(string name, string operationId, AssessmentOperation operation)
    {
        if (operation.HttpStatus == (int)HttpStatusCode.NotFound)
        {
            return Fail(name, operationId, operation.HttpStatus, "No assessment operation was found for this assessment name and operation ID.");
        }

        if (operation.HttpStatus is not (>= 200 and < 300) || string.IsNullOrWhiteSpace(operation.Status))
        {
            return Fail(name, operationId, operation.HttpStatus,
                $"Assessment Service status check failed (HTTP {operation.HttpStatus}): {operation.ErrorCode} {operation.ErrorMessage}".TrimEnd());
        }

        bool isCompleted = s_terminalStatuses.Contains(operation.Status, StringComparer.OrdinalIgnoreCase);
        bool succeeded = string.Equals(operation.Status, "Succeeded", StringComparison.OrdinalIgnoreCase);
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
            Instructions: !isCompleted
                ? "Tell the user the assessment is still running and suggest checking again in about a minute."
                : succeeded
                    ? "Tell the user the assessment has completed."
                    : "Tell the user the assessment did not complete and quote 'errorMessage' if present.");
    }

    private static GetAssessmentStatusCommandResult Fail(string? name, string? operationId, int? status, string? error) =>
        new(false, error ?? "Unknown error.", status, name, operationId, null, false, null, null, null, null);

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
        string? Instructions);
}
