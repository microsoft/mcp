// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.RegularExpressions;
using Azure.Mcp.Tools.AzureMigrate.Commands.Inventory;
using Azure.Mcp.Tools.AzureMigrate.Options.Assessment;
using Microsoft.AspNetCore.Http;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AzureMigrate.Commands.Assessment;

/// <summary>
/// POC: creates an Azure Migrate assessment through the Assessment Service backdoor PUT, waits for its tracking operation to
/// complete and then sends the PUT HA assessment call, all within one tool call while the caller's token is valid. Both
/// downstream calls are currently mocked. If the assessment does not complete within <see cref="DefaultTimeout"/>, the command
/// returns the operation so the assessment status command can check it later; the PUT HA call is then not sent.
/// </summary>
[CommandMetadata(
    Id = "4a38afca-7663-42f4-bb6d-943b745bb8cf",
    Name = "create",
    Title = "Create Azure Migrate Assessment",
    Description = "Creates an Azure Migrate assessment for the signed-in user, waits for it to complete (up to a few minutes) and then " +
        "creates the HA assessment for it. Requires 'assessment-name', 'target-region' (an Azure region name such as 'eastus') and " +
        "'currency' (a three-letter code such as 'USD'); ask the user for any that are missing and do not invent them. Tell the user " +
        "this can take a few minutes before calling it. Returns 'status', 'isCompleted', 'haStatus' and 'instructions'. Follow " +
        "'instructions'; if the assessment is still running, keep 'assessmentName' and 'operationId' to check its status later. " +
        "Do not mention tool or parameter names to the user.",
    OperationPlane = ToolOperationPlane.NotApplicable,
    Destructive = false,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed partial class CreateAssessmentCommand(
    IAssessmentServiceClient client,
    IHaAssessmentClient haClient,
    IHttpContextAccessor httpContextAccessor)
    : BaseCommand<CreateAssessmentOptions, CreateAssessmentCommand.CreateAssessmentCommandResult>
{
    internal static readonly TimeSpan DefaultTimeout = TimeSpan.FromMinutes(3);
    internal static readonly TimeSpan MinPollInterval = TimeSpan.FromSeconds(2);
    internal static readonly TimeSpan MaxPollInterval = TimeSpan.FromSeconds(10);

    private static readonly string[] s_terminalStatuses = ["Succeeded", "Failed", "Canceled", "Cancelled"];

    private readonly IAssessmentServiceClient _client = client;
    private readonly IHaAssessmentClient _haClient = haClient;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    [GeneratedRegex("^[A-Za-z0-9][A-Za-z0-9_-]{0,63}$")]
    private static partial Regex AssessmentNameRegex();

    [GeneratedRegex("^[a-z][a-z0-9]{1,39}$")]
    private static partial Regex RegionRegex();

    [GeneratedRegex("^[A-Z]{3}$")]
    private static partial Regex CurrencyRegex();

    internal static bool IsValidAssessmentName(string? name) => !string.IsNullOrEmpty(name) && AssessmentNameRegex().IsMatch(name);

    internal static bool IsTerminal(string? status) =>
        status is not null && s_terminalStatuses.Contains(status, StringComparer.OrdinalIgnoreCase);

    /// <summary>Returns the region in lower case without spaces (for example 'East US' becomes 'eastus'), or null if invalid.</summary>
    internal static string? NormalizeRegion(string? region)
    {
        string? normalized = region?.Replace(" ", string.Empty, StringComparison.Ordinal).Trim().ToLowerInvariant();
        return !string.IsNullOrEmpty(normalized) && RegionRegex().IsMatch(normalized) ? normalized : null;
    }

    /// <summary>Returns the currency code in upper case, or null if it is not three letters.</summary>
    internal static string? NormalizeCurrency(string? currency)
    {
        string? normalized = currency?.Trim().ToUpperInvariant();
        return !string.IsNullOrEmpty(normalized) && CurrencyRegex().IsMatch(normalized) ? normalized : null;
    }

    /// <summary>Uses the service's retry-after hint, kept between <see cref="MinPollInterval"/> and <see cref="MaxPollInterval"/>.</summary>
    internal static TimeSpan GetPollInterval(int? retryAfterSeconds)
    {
        TimeSpan hint = TimeSpan.FromSeconds(retryAfterSeconds ?? 0);
        return hint < MinPollInterval ? MinPollInterval : hint > MaxPollInterval ? MaxPollInterval : hint;
    }

    /// <inheritdoc/>
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, CreateAssessmentOptions options, CancellationToken cancellationToken)
    {
        CreateAssessmentCommandResult result = await RunAsync(
            options, TimeProvider.System, (delay, ct) => Task.Delay(delay, TimeProvider.System, ct), DefaultTimeout, cancellationToken);

        context.Response.Status = HttpStatusCode.OK;
        context.Response.Results = ResponseResult.Create(result, AzureMigrateJsonContext.Default.CreateAssessmentCommandResult);
        context.Response.Message = string.Empty;
        return context.Response;
    }

    internal async Task<CreateAssessmentCommandResult> RunAsync(
        CreateAssessmentOptions options,
        TimeProvider timeProvider,
        Func<TimeSpan, CancellationToken, Task> delayAsync,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        string? name = options.AssessmentName?.Trim();
        if (!IsValidAssessmentName(name))
        {
            return Fail(name, null, "'assessment-name' is required: 1-64 characters, starting with a letter or digit, using only letters, digits, '-' or '_'.");
        }

        string? region = NormalizeRegion(options.TargetRegion);
        if (region is null)
        {
            return Fail(name, null, "'target-region' is required and must be an Azure region name such as 'eastus'.");
        }

        string? currency = NormalizeCurrency(options.Currency);
        if (currency is null)
        {
            return Fail(name, null, "'currency' is required and must be a three-letter currency code such as 'USD'.");
        }

        (string? token, string? authError) = DsBackdoorClient.GetBearerToken(_httpContextAccessor);
        if (token is null)
        {
            return Fail(name, null, authError);
        }

        AssessmentRequest request = new(name!, region, currency);
        AssessmentOperation operation = await _client.CreateAssessmentAsync(request, token, cancellationToken);
        if (operation.HttpStatus is not (>= 200 and < 300) || operation.OperationId is null)
        {
            return Fail(name, operation.HttpStatus,
                $"Assessment Service create failed (HTTP {operation.HttpStatus}): {operation.ErrorCode} {operation.ErrorMessage}".TrimEnd());
        }

        Guid operationId = operation.OperationId.Value;
        DateTimeOffset deadline = timeProvider.GetUtcNow() + timeout;
        while (!IsTerminal(operation.Status))
        {
            TimeSpan delay = GetPollInterval(operation.RetryAfterSeconds);
            if (timeProvider.GetUtcNow() + delay > deadline)
            {
                return Result(request, operation, operationId, null,
                    "Tell the user the assessment was started but is still running, so the HA assessment has not been created yet. " +
                    "They can ask for its status later; keep assessmentName and operationId for that.");
            }

            await delayAsync(delay, cancellationToken);
            AssessmentOperation polled = await _client.GetOperationAsync(request.AssessmentName, operationId, token, cancellationToken);
            if (polled.HttpStatus is not (>= 200 and < 300) || string.IsNullOrWhiteSpace(polled.Status))
            {
                return Fail(name, polled.HttpStatus,
                    $"Assessment Service status check failed (HTTP {polled.HttpStatus}): {polled.ErrorCode} {polled.ErrorMessage}".TrimEnd(),
                    operationId);
            }

            operation = polled;
        }

        if (!string.Equals(operation.Status, "Succeeded", StringComparison.OrdinalIgnoreCase))
        {
            return Result(request, operation, operationId, null,
                "Tell the user the assessment did not complete, so the HA assessment was not created. Quote 'errorMessage' if present.");
        }

        HaAssessmentResult ha = await _haClient.PutHaAssessmentAsync(request, token, cancellationToken);
        bool haSucceeded = ha.HttpStatus is >= 200 and < 300;
        return Result(request, operation, operationId, ha,
            haSucceeded
                ? "Tell the user the assessment has completed and the HA assessment has been created."
                : "Tell the user the assessment has completed but creating the HA assessment failed. Quote 'haErrorMessage' if present.");
    }

    private static CreateAssessmentCommandResult Result(
        AssessmentRequest request,
        AssessmentOperation operation,
        Guid operationId,
        HaAssessmentResult? ha,
        string instructions) =>
        new(
            Success: ha is null || ha.HttpStatus is >= 200 and < 300,
            Error: ha is null || ha.HttpStatus is >= 200 and < 300
                ? null
                : $"HA assessment create failed (HTTP {ha.HttpStatus}): {ha.ErrorCode} {ha.ErrorMessage}".TrimEnd(),
            HttpStatus: operation.HttpStatus,
            AssessmentName: request.AssessmentName,
            TargetRegion: request.TargetRegion,
            Currency: request.Currency,
            OperationId: operationId.ToString("D"),
            Status: operation.Status,
            IsCompleted: IsTerminal(operation.Status),
            ErrorCode: operation.ErrorCode,
            ErrorMessage: operation.ErrorMessage,
            HaHttpStatus: ha?.HttpStatus,
            HaStatus: ha?.Status,
            HaErrorMessage: ha?.ErrorMessage,
            Instructions: instructions);

    private static CreateAssessmentCommandResult Fail(string? name, int? status, string? error, Guid? operationId = null) =>
        new(false, error ?? "Unknown error.", status, name, null, null, operationId?.ToString("D"), null, false, null, null, null, null, null, null);

    /// <summary>Result of the assessment create command.</summary>
    public sealed record CreateAssessmentCommandResult(
        bool Success,
        string? Error,
        int? HttpStatus,
        string? AssessmentName,
        string? TargetRegion,
        string? Currency,
        string? OperationId,
        string? Status,
        bool IsCompleted,
        string? ErrorCode,
        string? ErrorMessage,
        int? HaHttpStatus,
        string? HaStatus,
        string? HaErrorMessage,
        string? Instructions);
}
