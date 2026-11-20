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

    /// <summary>Used instead of <see cref="CheckAgainInstructions"/> when the business case card tracks the business case itself.</summary>
    internal const string CardTracksInstructions =
        "The business case card shown to the user tracks the business case by itself, updates its stage and shows the ROI report " +
        "when it is ready. Say so in the 'Next step:' line, then end your reply: do not check the status again yourself " +
        "unless the user asks for it or says the card is not updating, and then use the latest 'nextStatusParameters' in the conversation.";

    /// <summary>Returns what the agent should do next while the business case runs, depending on whether the card tracks it.</summary>
    internal static string TrackingInstructions => BusinessCaseReportWidget.IsEnabled ? CardTracksInstructions : CheckAgainInstructions;

    internal static int StageNumber(string stage) => stage == HaSkill ? 2 : 1;

    internal static BusinessCaseCommandResult Invalid(string? error, string? assessmentName = null, string? asOperationId = null) =>
        new(false, error ?? "Unknown error.", null, false, null, null, Total, assessmentName, asOperationId, null, null,
            null, null, null, null, null, null, null,
            ReplyFormat.Failure("Business case request not valid.", "ask for the correct value of that input; do not guess it."));
}
