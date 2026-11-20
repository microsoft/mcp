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
/// POC: creates an Azure Catalyst machine assessment from a collector ZIP through the external assessment create API, waits for its
/// operation to complete (assessment operation status API) and then starts the HA skill on the Discovery Service, all within one tool
/// call while the caller's token is valid. On success the operation returns a temporary read-only URL of the assessment output. The
/// HA session is created and invoked, and its input is a mini zip (see <see cref="HaInputPackage"/>) holding the read SAS URLs of the
/// inventory and of the assessment output, uploaded by the server to the SAS URL returned by the invoke; the HA skill downloads the
/// files itself. The operation is polled up to <see cref="MaxPollAttempts"/> times with exponential backoff (see
/// <see cref="GetPollDelay"/>); if it has not completed by then, the command returns the operation so the assessment status command
/// can check it later, and the HA session is not started.
/// </summary>
[CommandMetadata(
    Id = "4a38afca-7663-42f4-bb6d-943b745bb8cf",
    Name = "create",
    Title = "Create Azure Catalyst Assessment",
    Description = "Creates an Azure Catalyst assessment for the signed-in user from their offline collector ZIP, waits for it to complete " +
        "(up to about 17 minutes) and then starts the HA assessment for it, returning a link to download the assessment. Requires " +
        "'assessment-name', 'target-region' (an Azure region name such as 'eastus'), 'currency' (a three-letter code such as 'USD') and " +
        "'inventory-sas-uri' (a read-only https SAS URL of the collector ZIP); ask the user for any that are missing and do not invent " +
        "them. Optional settings such as savings option, Azure Hybrid Benefit, discount, scaling factors, environment type, disk types, " +
        "VM families, uptime and licensing program use the assessment defaults when left out; pass them only when the user asks for them " +
        "and do not ask for them otherwise. Tell the user this can take up to about 17 minutes before calling it. Returns 'status', 'isCompleted', " +
        "'assessmentOutputUrl', 'haOperationId' and 'instructions'. Follow 'instructions'; if the assessment is still running, keep " +
        "'operationId' to check its status later. Do not mention tool or parameter names to the user.",
    OperationPlane = ToolOperationPlane.NotApplicable,
    Destructive = false,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed partial class CreateAssessmentCommand(
    IAssessmentCreateClient createClient,
    IAssessmentOperationStatusClient statusClient,
    IHaSessionClient haClient,
    IHttpContextAccessor httpContextAccessor)
    : BaseCommand<CreateAssessmentOptions, CreateAssessmentCommand.CreateAssessmentCommandResult>
{
    // Status polls after the create call: 1, 2, 4, ... 512 seconds apart (about 17 minutes in total).
    internal const int MaxPollAttempts = 10;
    internal static readonly TimeSpan InitialPollDelay = TimeSpan.FromSeconds(1);

    // Operation statuses of the assessment: Running, Succeeded, Failed or Canceled.
    private static readonly string[] s_terminalStatuses = ["Succeeded", "Failed", "Canceled"];

    // Currencies accepted by the assessment.
    private static readonly string[] s_supportedCurrencies =
    [
        "USD", "DKK", "CAD", "IDR", "JPY", "KRW", "NZD", "NOK", "RUB", "SAR", "ZAR", "SEK", "TRY", "GBP", "MXN", "MYR", "INR", "HKD",
        "BRL", "TWD", "EUR", "CHF", "ARS", "AUD", "CNY",
    ];

    private readonly IAssessmentCreateClient _createClient = createClient;
    private readonly IAssessmentOperationStatusClient _statusClient = statusClient;
    private readonly IHaSessionClient _haClient = haClient;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    [GeneratedRegex("^[A-Za-z0-9][A-Za-z0-9_-]{0,63}$")]
    private static partial Regex AssessmentNameRegex();

    [GeneratedRegex("^[a-z][a-z0-9]{1,39}$")]
    private static partial Regex RegionRegex();

    internal static bool IsValidAssessmentName(string? name) => !string.IsNullOrEmpty(name) && AssessmentNameRegex().IsMatch(name);

    /// <summary>Returns true for an absolute http(s) URL, the shape accepted for 'inventory-sas-uri' and SAS URLs in general.</summary>
    internal static bool IsValidInventorySasUri(string? uri) =>
        !string.IsNullOrWhiteSpace(uri)
        && Uri.TryCreate(uri, UriKind.Absolute, out Uri? parsed)
        && (parsed.Scheme == Uri.UriSchemeHttps || parsed.Scheme == Uri.UriSchemeHttp);

    internal static bool IsTerminal(string? status) =>
        status is not null && s_terminalStatuses.Contains(status, StringComparer.OrdinalIgnoreCase);

    /// <summary>Returns the region in lower case without spaces (for example 'East US' becomes 'eastus'), or null if invalid.</summary>
    internal static string? NormalizeRegion(string? region)
    {
        string? normalized = region?.Replace(" ", string.Empty, StringComparison.Ordinal).Trim().ToLowerInvariant();
        return !string.IsNullOrEmpty(normalized) && RegionRegex().IsMatch(normalized) ? normalized : null;
    }

    /// <summary>Returns the currency code in upper case, or null if the assessment does not support it.</summary>
    internal static string? NormalizeCurrency(string? currency)
    {
        string? normalized = currency?.Trim().ToUpperInvariant();
        return normalized is not null && s_supportedCurrencies.Contains(normalized, StringComparer.Ordinal) ? normalized : null;
    }

    /// <summary>Delay before status poll <paramref name="attempt"/> (0-based): <see cref="InitialPollDelay"/> doubled on each attempt.</summary>
    internal static TimeSpan GetPollDelay(int attempt) => InitialPollDelay * Math.Pow(2, attempt);

    /// <summary>
    /// Validates the optional settings and returns them with enum values in the assessment's spelling, or null when none were supplied.
    /// </summary>
    internal static (AssessmentSettings? Settings, string? Error) BuildSettings(CreateAssessmentOptions o)
    {
        List<string> errors = [];

        double? Range(string option, double? value, double min, double max)
        {
            if (value is not null && (double.IsNaN(value.Value) || value < min || value > max))
            {
                errors.Add($"'{option}' must be between {min} and {max}.");
            }

            return value;
        }

        int? IntRange(string option, int? value, int min, int max)
        {
            if (value is not null && (value < min || value > max))
            {
                errors.Add($"'{option}' must be between {min} and {max}.");
            }

            return value;
        }

        string? One(string option, string? value, string[] allowed)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            string? match = allowed.FirstOrDefault(a => a.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase));
            if (match is null)
            {
                errors.Add($"'{option}' must be one of: {string.Join(", ", allowed)}.");
            }

            return match;
        }

        string[]? Many(string option, string[]? values, string[]? allowed)
        {
            string[]? items = values?.Where(v => !string.IsNullOrWhiteSpace(v)).Select(v => v.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
            if (items is null || items.Length == 0)
            {
                return null;
            }

            return allowed is null ? items : items.Select(v => One(option, v, allowed)!).ToArray();
        }

        string? subscriptionId = string.IsNullOrWhiteSpace(o.BillingSubscriptionId) ? null : o.BillingSubscriptionId.Trim();
        if (subscriptionId is not null && !Guid.TryParse(subscriptionId, out _))
        {
            errors.Add("'billing-subscription-id' must be a subscription ID (GUID).");
        }

        AssessmentSettings settings = new()
        {
            ScalingFactor = Range("scaling-factor", o.ScalingFactor, 1, 2),
            StorageScalingFactor = Range("storage-scaling-factor", o.StorageScalingFactor, 1, 2),
            DiscountPercentage = Range("discount-percentage", o.DiscountPercentage, 0, 100),
            EnvironmentType = One("environment-type", o.EnvironmentType, ["Production", "DevTest"]),
            LicensingProgram = One("licensing-program", o.LicensingProgram, ["Retail", "EA", "MCA"]),
            BillingSubscriptionId = subscriptionId,
            SavingsOption = One("savings-option", o.SavingsOption, ["None", "RI1Year", "RI3Year", "SavingsPlan1Year", "SavingsPlan3Year"]),
            AzureOfferCode = One("azure-offer-code", o.AzureOfferCode,
                ["MSAZR0003P", "MSAZR0023P", "MSMCAZR0044P", "MSMCAZR0059P", "MSMCAZR0060P", "MSMCAZR0063P", "MSAZRUSGOV0003P", "EA"]),
            SecurityOfferingType = One("security-offering-type", o.SecurityOfferingType, ["NO", "MDC"]),
            DiskTypes = Many("disk-types", o.DiskTypes, ["Standard", "StandardSSD", "Premium", "StandardOrPremium", "Ultra", "PremiumV2"]),
            PricingTier = One("pricing-tier", o.PricingTier, ["Standard", "Basic"]),
            StorageRedundancy = One("storage-redundancy", o.StorageRedundancy,
                ["LocallyRedundant", "GeoRedundant", "ZoneRedundant", "ReadAccessGeoRedundant"]),
            AzureHybridBenefit = One("azure-hybrid-benefit", o.AzureHybridBenefit, ["Yes", "No"]),
            LinuxAzureHybridBenefit = One("linux-azure-hybrid-benefit", o.LinuxAzureHybridBenefit, ["Yes", "No"]),
            VmFamilies = Many("vm-families", o.VmFamilies, null),
            UptimeDaysPerMonth = IntRange("uptime-days-per-month", o.UptimeDaysPerMonth, 1, 31),
            UptimeHoursPerDay = IntRange("uptime-hours-per-day", o.UptimeHoursPerDay, 1, 24),
            VmSecurityOptions = Many("vm-security-options", o.VmSecurityOptions, ["Standard", "TVM", "CVM"]),
            ManagementServices = Many("management-services", o.ManagementServices, null),
        };

        if (errors.Count > 0)
        {
            return (null, string.Join(" ", errors.Distinct()));
        }

        return settings == new AssessmentSettings() ? (null, null) : (settings, null);
    }

    /// <inheritdoc/>
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, CreateAssessmentOptions options, CancellationToken cancellationToken)
    {
        CreateAssessmentCommandResult result = await RunAsync(
            options, (delay, ct) => Task.Delay(delay, ct), cancellationToken);

        context.Response.Status = HttpStatusCode.OK;
        context.Response.Results = ResponseResult.Create(result, AzureMigrateJsonContext.Default.CreateAssessmentCommandResult);
        context.Response.Message = string.Empty;
        return context.Response;
    }

    internal async Task<CreateAssessmentCommandResult> RunAsync(
        CreateAssessmentOptions options,
        Func<TimeSpan, CancellationToken, Task> delayAsync,
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
            return Fail(name, null, $"'currency' is required and must be one of: {string.Join(", ", s_supportedCurrencies)}.");
        }

        string? inventorySasUri = options.InventorySasUri?.Trim();
        if (!IsValidInventorySasUri(inventorySasUri))
        {
            return Fail(name, null, "'inventory-sas-uri' is required and must be an https read-only SAS URL of the collector ZIP.");
        }

        (AssessmentSettings? settings, string? settingsError) = BuildSettings(options);
        if (settingsError is not null)
        {
            return Fail(name, null, settingsError);
        }

        (string? token, string? authError) = DsBackdoorClient.GetBearerToken(_httpContextAccessor);
        if (token is null)
        {
            return Fail(name, null, authError);
        }

        // TODO: Pass the caller's workspace id once it is defined.
        AssessmentRequest request = new(name!, WorkspaceId: null, inventorySasUri!, region, currency, settings);
        AssessmentOperation operation = await _createClient.CreateAssessmentAsync(request, token, cancellationToken);
        if (operation.HttpStatus is not (>= 200 and < 300) || operation.OperationId is null)
        {
            return Fail(name, operation.HttpStatus,
                $"Assessment create failed (HTTP {operation.HttpStatus}): {operation.ErrorCode} {operation.ErrorMessage}".TrimEnd());
        }

        Guid operationId = operation.OperationId.Value;
        AssessmentOperation? lastError = null;
        for (int attempt = 0; attempt < MaxPollAttempts && !IsTerminal(operation.Status); attempt++)
        {
            await delayAsync(GetPollDelay(attempt), cancellationToken);
            AssessmentOperation polled = await _statusClient.GetOperationAsync(operationId, request.WorkspaceId, token, cancellationToken);
            if (polled.HttpStatus is >= 200 and < 300 && !string.IsNullOrWhiteSpace(polled.Status))
            {
                operation = polled;
                lastError = null;
                continue;
            }

            // A missing operation will not appear later; other errors (throttling, server or network errors) are retried.
            if (polled.HttpStatus == (int)HttpStatusCode.NotFound)
            {
                return Fail(name, polled.HttpStatus,
                    $"Assessment status check failed (HTTP {polled.HttpStatus}): {polled.ErrorCode} {polled.ErrorMessage}".TrimEnd(),
                    operationId);
            }

            lastError = polled;
        }

        if (!IsTerminal(operation.Status))
        {
            string lastErrorText = lastError is null
                ? string.Empty
                : $" The last status check failed (HTTP {lastError.HttpStatus}): {lastError.ErrorCode} {lastError.ErrorMessage}".TrimEnd();
            return Result(request, operation, operationId, null,
                "Tell the user the assessment was started but is still running, so the HA assessment has not been started yet. " +
                "They can ask for its status later; keep operationId for that." + lastErrorText);
        }

        if (!string.Equals(operation.Status, "Succeeded", StringComparison.OrdinalIgnoreCase))
        {
            return Result(request, operation, operationId, null,
                "Tell the user the assessment did not complete, so the HA assessment was not started. Quote 'errorMessage' if present.");
        }

        if (!IsValidInventorySasUri(operation.OutputUrl))
        {
            HaSessionResult noOutput = new("assessmentOutput", null, null, false, null, null,
                "The assessment did not return a valid output URL, so the HA input could not be prepared.");
            return Result(request, operation, operationId, noOutput,
                "Tell the user the assessment has completed but the HA assessment could not be started because the assessment output " +
                "link is missing. They can ask for the assessment status to get a new link.");
        }

        HaSessionResult ha = await _haClient.StartAsync(token, new HaInput(inventorySasUri!, operation.OutputUrl!), cancellationToken);
        return Result(request, operation, operationId, ha,
            ha.FailedStep is null && MockHaSessionClient.IsMockSession(ha.SessionId)
                ? "Tell the user the assessment has completed. The HA assessment is mocked because the HA skill is not deployed yet, " +
                  "so no HA assessment was run. " + DownloadInstructions
                : ha.FailedStep is null
                ? "Tell the user the assessment has completed and the HA assessment has started with their inventory and the assessment; " +
                  "it runs in the background and they do not need to upload anything. " + DownloadInstructions + " Keep haSessionId " +
                  "and haOperationId to check the HA assessment later."
                : "Tell the user the assessment has completed but starting the HA assessment failed. Quote 'haErrorMessage' if present. " +
                  DownloadInstructions);
    }

    /// <summary>Instructions for downloading the assessment output with 'assessmentDownloadCommand'.</summary>
    internal const string DownloadInstructions =
        "If assessmentDownloadCommand is present, show it to the user exactly as returned, in a code block, so they can download " +
        "the assessment (recommendations JSON); never retype or shorten it and never show the URL inside it on its own. Ask them to " +
        $"replace {ExportInventoryCommand.FilePathPlaceholder} with the full path to save the file and run it within an hour, " +
        "after which the link expires and they can ask for the assessment status again to get a new one.";

    private static CreateAssessmentCommandResult Result(
        AssessmentRequest request,
        AssessmentOperation operation,
        Guid operationId,
        HaSessionResult? ha,
        string instructions) =>
        new(
            Success: ha?.FailedStep is null,
            Error: ha?.FailedStep is null ? null : $"Starting the HA assessment failed at {ha.FailedStep}: {ha.Error}",
            HttpStatus: operation.HttpStatus,
            AssessmentName: request.AssessmentName,
            TargetRegion: request.TargetRegion,
            Currency: request.Currency,
            OperationId: operationId.ToString("D"),
            Status: operation.Status,
            IsCompleted: IsTerminal(operation.Status),
            ErrorCode: operation.ErrorCode,
            ErrorMessage: operation.ErrorMessage,
            AssessmentOutputUrl: operation.OutputUrl,
            AssessmentDownloadCommand: ExportInventoryCommand.BuildDownloadCommand(operation.OutputUrl),
            HaFailedStep: ha?.FailedStep,
            HaHttpStatus: ha?.HttpStatus,
            HaSessionId: ha?.SessionId,
            HaReusedExistingSession: ha?.ReusedExistingSession,
            HaOperationId: ha?.OperationId,
            HaErrorMessage: ha?.Error,
            Instructions: instructions);

    private static CreateAssessmentCommandResult Fail(string? name, int? status, string? error, Guid? operationId = null) =>
        new(
            Success: false,
            Error: error ?? "Unknown error.",
            HttpStatus: status,
            AssessmentName: name,
            TargetRegion: null,
            Currency: null,
            OperationId: operationId?.ToString("D"),
            Status: null,
            IsCompleted: false,
            ErrorCode: null,
            ErrorMessage: null,
            AssessmentOutputUrl: null,
            AssessmentDownloadCommand: null,
            HaFailedStep: null,
            HaHttpStatus: null,
            HaSessionId: null,
            HaReusedExistingSession: null,
            HaOperationId: null,
            HaErrorMessage: null,
            Instructions: null);

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
        string? AssessmentOutputUrl,
        string? AssessmentDownloadCommand,
        string? HaFailedStep,
        int? HaHttpStatus,
        string? HaSessionId,
        bool? HaReusedExistingSession,
        string? HaOperationId,
        string? HaErrorMessage,
        string? Instructions);
}
