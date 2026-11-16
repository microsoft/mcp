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
/// POC: creates an Azure Migrate assessment through the Assessment Service backdoor PUT (currently mocked) and returns the
/// tracking operation. It does not wait for completion; the assessment status command checks it.
/// </summary>
[CommandMetadata(
    Id = "4a38afca-7663-42f4-bb6d-943b745bb8cf",
    Name = "create",
    Title = "Create Azure Migrate Assessment",
    Description = "Creates an Azure Migrate assessment for the signed-in user. Requires 'assessment-name', 'target-region' (an Azure " +
        "region name such as 'eastus') and 'currency' (a three-letter code such as 'USD'); ask the user for any that are missing and " +
        "do not invent them. Returns 'assessmentName', 'operationId', 'status' and 'instructions'. The assessment runs in the " +
        "background; follow 'instructions' and keep 'assessmentName' and 'operationId' to check its status later. " +
        "Do not mention tool or parameter names to the user.",
    OperationPlane = ToolOperationPlane.NotApplicable,
    Destructive = false,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed partial class CreateAssessmentCommand(IAssessmentServiceClient client, IHttpContextAccessor httpContextAccessor)
    : BaseCommand<CreateAssessmentOptions, CreateAssessmentCommand.CreateAssessmentCommandResult>
{
    private readonly IAssessmentServiceClient _client = client;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    [GeneratedRegex("^[A-Za-z0-9][A-Za-z0-9_-]{0,63}$")]
    private static partial Regex AssessmentNameRegex();

    [GeneratedRegex("^[a-z][a-z0-9]{1,39}$")]
    private static partial Regex RegionRegex();

    [GeneratedRegex("^[A-Z]{3}$")]
    private static partial Regex CurrencyRegex();

    internal static bool IsValidAssessmentName(string? name) => !string.IsNullOrEmpty(name) && AssessmentNameRegex().IsMatch(name);

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

    /// <inheritdoc/>
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, CreateAssessmentOptions options, CancellationToken cancellationToken)
    {
        CreateAssessmentCommandResult result = await RunAsync(options, cancellationToken);

        context.Response.Status = HttpStatusCode.OK;
        context.Response.Results = ResponseResult.Create(result, AzureMigrateJsonContext.Default.CreateAssessmentCommandResult);
        context.Response.Message = string.Empty;
        return context.Response;
    }

    internal async Task<CreateAssessmentCommandResult> RunAsync(CreateAssessmentOptions options, CancellationToken cancellationToken)
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

        AssessmentOperation operation = await _client.CreateAssessmentAsync(new(name!, region, currency), token, cancellationToken);
        if (operation.HttpStatus is not (>= 200 and < 300) || operation.OperationId is null)
        {
            return Fail(name, operation.HttpStatus,
                $"Assessment Service create failed (HTTP {operation.HttpStatus}): {operation.ErrorCode} {operation.ErrorMessage}".TrimEnd());
        }

        return new(
            Success: true,
            Error: null,
            HttpStatus: operation.HttpStatus,
            AssessmentName: name,
            TargetRegion: region,
            Currency: currency,
            OperationId: operation.OperationId.Value.ToString("D"),
            Status: operation.Status,
            RetryAfterSeconds: operation.RetryAfterSeconds,
            Instructions: "Tell the user the assessment has started for the given region and currency and runs in the background. " +
                "They can ask for its status in a minute; keep assessmentName and operationId for that.");
    }

    private static CreateAssessmentCommandResult Fail(string? name, int? status, string? error) =>
        new(false, error ?? "Unknown error.", status, name, null, null, null, null, null, null);

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
        int? RetryAfterSeconds,
        string? Instructions);
}
