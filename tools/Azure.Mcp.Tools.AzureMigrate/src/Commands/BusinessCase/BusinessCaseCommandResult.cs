// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AzureMigrate.Commands.BusinessCase;

/// <summary>
/// Result of the business case create and status commands. The flow has two stages: Assessment (the assessment, then the Business
/// Case Service on-premises TCO and the HA evaluate, all run by the status command once the assessment succeeds) and HaSkill (the HA
/// skill run on the Discovery Service). Nothing is stored on the server: <see cref="NextStatusParameters"/> carries every value the
/// next status call needs and is null once the business case has finished. <see cref="Success"/> is false only for invalid input or
/// a missing token; a failed business case has <see cref="Status"/> Failed.
/// </summary>
public sealed record BusinessCaseCommandResult(
    bool Success,
    string? Error,
    string? Status,
    bool IsCompleted,
    string? Stage,
    int? StageNumber,
    int TotalStages,
    string? AssessmentName,
    string? AsOperationId,
    string? DsSessionId,
    string? DsOperationId,
    string? FailedStep,
    int? HttpStatus,
    string? ErrorCode,
    string? ErrorMessage,
    string? Result,
    int? RetryAfterSeconds,
    BusinessCaseNextStatusParameters? NextStatusParameters,
    string? Instructions)
{
    /// <summary>The ROI report (ROI.md) Markdown once the business case succeeded, when it could be downloaded; else null.</summary>
    public string? Report { get; init; }

    /// <summary>
    /// Read SAS URL of the business case artifacts ZIP (the Discovery Service 'internal' export) once the business case succeeded,
    /// when it could be exported; else null.
    /// </summary>
    public string? Artifacts { get; init; }
}

/// <summary>
/// Parameters for the next business case status call, named as the tool parameters so they can be passed back unchanged.
/// </summary>
public sealed record BusinessCaseNextStatusParameters(
    [property: JsonPropertyName("assessment-name")] string? AssessmentName,
    [property: JsonPropertyName("as-operation-id")] string AsOperationId,
    [property: JsonPropertyName("inventory-sas-uri")] string InventorySasUri,
    [property: JsonPropertyName("ds-session-id")] string? DsSessionId,
    [property: JsonPropertyName("ds-operation-id")] string? DsOperationId,
    [property: JsonPropertyName("network-cost-percentage")] double NetworkCostPercentage,
    [property: JsonPropertyName("iaas-labor-cost-percentage")] double IaasLaborCostPercentage,
    [property: JsonPropertyName("paas-labor-cost-percentage")] double PaasLaborCostPercentage,
    [property: JsonPropertyName("management-labor-cost-percentage")] double ManagementLaborCostPercentage,
    [property: JsonPropertyName("infrastructure-growth-rate")] double InfrastructureGrowthRate,
    [property: JsonPropertyName("migration-completion-by-year")] IReadOnlyList<double> MigrationCompletionByYear)
{
    internal static BusinessCaseNextStatusParameters Create(
        string? assessmentName, Guid asOperationId, string inventorySasUri, BusinessCaseHaSettings settings,
        string? dsSessionId = null, string? dsOperationId = null) =>
        new(assessmentName, asOperationId.ToString("D"), inventorySasUri, dsSessionId, dsOperationId,
            settings.NetworkCostPercentage, settings.IaasLaborCostPercentage, settings.PaasLaborCostPercentage,
            settings.ManagementLaborCostPercentage, settings.InfrastructureGrowthRate, settings.MigrationCompletionByYear);

    /// <inheritdoc/>
    public override string ToString() => $"BusinessCaseNextStatusParameters {{ AsOperationId = {AsOperationId}, SAS URL hidden }}";
}

/// <summary>Stage names and shared result builders of the business case commands.</summary>
internal static class BusinessCaseStages
{
    internal const string Assessment = "Assessment";
    internal const string HaSkill = "HaSkill";
    internal const int Total = 2;

    internal const string Running = "Running";
    internal const string Succeeded = "Succeeded";
    internal const string Failed = "Failed";

    internal const string CheckAgainInstructions =
        "Check the status again with exactly the values in 'nextStatusParameters', after about 'retryAfterSeconds' seconds. " +
        "Do not change or invent any of them. Call the status tool directly yourself; never write or run a script or code to call it.";

    /// <summary>
    /// Used instead of <see cref="CheckAgainInstructions"/> when the report card is enabled: the agent keeps its turn open by waiting
    /// quietly with the wait tool, then shows the report card with one status call and replies once with the ROI summary.
    /// </summary>
    internal const string WaitInstructions =
        "Right away, without waiting for the user, call the business case wait tool with exactly the values in " +
        "'nextStatusParameters', and keep calling it with the latest 'nextStatusParameters' it returns, one call at a time, until " +
        "'isCompleted' is true. Do not write anything to the user between these calls and do not call the business case status tool " +
        "for this; once 'isCompleted' is true, follow the 'instructions' of that last result.";

    /// <summary>Instructions of a wait tool result while the business case still runs.</summary>
    internal const string WaitAgainInstructions =
        "The business case is still running. Call the business case wait tool again right away with exactly the values in " +
        "'nextStatusParameters', without writing anything to the user and without changing or inventing any value. Call it directly " +
        "yourself; never write or run a script or code to call it.";

    /// <summary>Instructions of a wait tool result once the business case succeeded and the report card is enabled.</summary>
    internal const string ShowReportInstructions =
        "The business case has finished. Right away, without writing anything to the user first, call the business case status tool " +
        "once with exactly the same values you passed to this last wait call; it shows the ROI report card to the user. Then reply once " +
        "by following the 'instructions' of that status result.";

    /// <summary>Returns what the agent should do next while the business case runs, depending on whether the card is enabled.</summary>
    internal static string TrackingInstructions => BusinessCaseReportWidget.IsEnabled ? WaitInstructions : CheckAgainInstructions;

    internal static int StageNumber(string stage) => stage == HaSkill ? 2 : 1;

    internal static BusinessCaseCommandResult Invalid(string? error, string? assessmentName = null, string? asOperationId = null) =>
        new(false, error ?? "Unknown error.", null, false, null, null, Total, assessmentName, asOperationId, null, null,
            null, null, null, null, null, null, null,
            ReplyFormat.Failure("Business case request not valid.", "ask for the correct value of that input; do not guess it."));
}
