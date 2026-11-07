// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.DeviceProvisioning.Models;

namespace Azure.Mcp.Tools.DeviceProvisioning.Services;

internal sealed class OnboardingValidationBuilder(
    string targetType,
    string targetId,
    string serviceName,
    string? intendedHub = null)
    : IOnboardingValidationSink
{
    private readonly List<OnboardingValidationFinding> _findings = [];
    private readonly List<OnboardingValidationExecutionLimitation> _executionLimitations = [];
    private readonly List<string> _limitations =
    [
        "This validation does not perform a device registration attempt.",
        "This validation does not establish which enrollment DPS would authoritatively select when another enrollment may match."
    ];
    private readonly List<OnboardingDeviceValidationResult> _deviceResults = [];

    public string IdScope { get; set; } = string.Empty;

    public string ServiceState { get; set; } = string.Empty;

    public string ProvisioningState { get; set; } = string.Empty;

    public string AttestationType { get; set; } = string.Empty;

    public string EffectiveAllocationPolicy { get; set; } = string.Empty;

    public OnboardingRegistrationState? RegistrationState { get; set; }

    public void AddFinding(
        string checkId,
        string status,
        string message,
        string? remediation = null) =>
        _findings.Add(new(checkId, status, message, remediation));

    public void AddExecutionLimitation(
        string operation,
        string reason,
        string message,
        string? requiredAction = null) =>
        _executionLimitations.Add(new(operation, reason, message, requiredAction));

    public void AddLimitation(string limitation)
    {
        if (!_limitations.Contains(limitation, StringComparer.Ordinal))
        {
            _limitations.Add(limitation);
        }
    }

    public void AddDeviceResults(IEnumerable<OnboardingDeviceValidationResult> deviceResults) =>
        _deviceResults.AddRange(deviceResults);

    public OnboardingValidationResult Build(bool essentialCheckIncomplete = false)
    {
        var passedCount = Count("passed") + _deviceResults.Sum(result => result.PassedCount);
        var blockerCount = Count("blocker") + _deviceResults.Sum(result => result.BlockerCount);
        var warningCount = Count("warning") + _deviceResults.Sum(result => result.WarningCount);
        var informationalCount = Count("informational") +
            _deviceResults.Sum(result => result.InformationalCount);
        var unverifiedCount = Count("unverified") +
            _deviceResults.Sum(result => result.UnverifiedCount);
        var isPartial =
            _executionLimitations.Count > 0 ||
            unverifiedCount > 0 ||
            _deviceResults.Any(result =>
                string.Equals(
                    result.ValidationCompleteness,
                    "partial",
                    StringComparison.OrdinalIgnoreCase));
        var validationResult = essentialCheckIncomplete
            ? "indeterminate"
            : blockerCount > 0
                ? "issuesFound"
                : "passed";
        var overallReadiness = _deviceResults.Count == 0
            ? null
            : blockerCount > 0
                ? "blocked"
                : warningCount > 0 || isPartial
                    ? "readyWithWarnings"
                    : "ready";
        var customerReadiness = overallReadiness ??
            (blockerCount > 0
                ? "blocked"
                : warningCount > 0 || isPartial
                    ? "readyWithWarnings"
                    : "ready");
        var customerSummary = BuildCustomerSummary(
            validationResult,
            customerReadiness,
            warningCount,
            isPartial);

        return new OnboardingValidationResult(
            validationResult,
            isPartial ? "partial" : "complete",
            targetType,
            targetId,
            serviceName,
            customerSummary,
            IdScope,
            ServiceState,
            ProvisioningState,
            AttestationType,
            EffectiveAllocationPolicy,
            passedCount,
            blockerCount,
            warningCount,
            informationalCount,
            unverifiedCount,
            _findings,
            _executionLimitations,
            RegistrationState,
            _limitations,
            overallReadiness,
            _deviceResults.Count == 0 ? null : _deviceResults.Count,
            _deviceResults.Count == 0
                ? null
                : _deviceResults.Count(result =>
                    string.Equals(result.OverallReadiness, "ready", StringComparison.Ordinal)),
            _deviceResults.Count == 0
                ? null
                : _deviceResults.Count(result =>
                    string.Equals(
                        result.OverallReadiness,
                        "readyWithWarnings",
                        StringComparison.Ordinal)),
            _deviceResults.Count == 0
                ? null
                : _deviceResults.Count(result =>
                    string.Equals(result.OverallReadiness, "blocked", StringComparison.Ordinal)),
            _deviceResults.Count == 0 ? null : _deviceResults);
    }

    private int Count(string status) =>
        _findings.Count(finding =>
            string.Equals(finding.Status, status, StringComparison.OrdinalIgnoreCase));

    private string BuildCustomerSummary(
        string validationResult,
        string? overallReadiness,
        int warningCount,
        bool isPartial)
    {
        var lines = new List<string>
        {
            $"Readiness: {FormatReadiness(overallReadiness, validationResult)}",
            $"Target: {serviceName} / {targetId}"
        };

        if (_deviceResults.Count > 0)
        {
            var readyCount = _deviceResults.Count(result =>
                string.Equals(result.OverallReadiness, "ready", StringComparison.Ordinal));
            var readyWithWarningsCount = _deviceResults.Count(result =>
                string.Equals(
                    result.OverallReadiness,
                    "readyWithWarnings",
                    StringComparison.Ordinal));
            var blockedCount = _deviceResults.Count(result =>
                string.Equals(result.OverallReadiness, "blocked", StringComparison.Ordinal));
            lines.Add(
                $"Devices: {_deviceResults.Count}/{_deviceResults.Count} assessed | " +
                $"{readyCount} ready | {readyWithWarningsCount} with warnings | " +
                $"{blockedCount} blocked");

            var assignedCount = _deviceResults.Count(result =>
                string.Equals(
                    result.RegistrationState?.Status,
                    "assigned",
                    StringComparison.OrdinalIgnoreCase));
            var intendedHubMatchCount = CountDeviceFindings(
                "registrationState.assignedHubIntent",
                "passed");
            var assignmentMismatchCount = CountDeviceFindings(
                "registrationState.assignedHubIntent",
                "warning");
            var missingRegistrationStateCount = _deviceResults.Count(result =>
                result.RegistrationState is null &&
                result.Findings.Any(finding =>
                    string.Equals(
                        finding.CheckId,
                        "registrationState.available",
                        StringComparison.Ordinal) &&
                    string.Equals(
                        finding.Status,
                        "informational",
                        StringComparison.OrdinalIgnoreCase)));
            if (assignedCount > 0 || assignmentMismatchCount > 0)
            {
                if (!string.IsNullOrWhiteSpace(intendedHub))
                {
                    lines.Add(string.Empty);
                    lines.Add("| Comparison | Prediction | Actual |");
                    lines.Add("|---|---|---|");
                    lines.Add(
                        $"| Registration | Devices complete assignment | " +
                        $"{assignedCount}/{_deviceResults.Count} assigned |");
                    lines.Add(
                        $"| Hub assignment | `{intendedHub}` | " +
                        $"{intendedHubMatchCount}/{_deviceResults.Count} matches, " +
                        $"{assignmentMismatchCount} mismatches |");
                }
                else
                {
                    lines.Add(
                        $"Registration: {assignedCount}/{_deviceResults.Count} assigned");
                }
            }
            else if (missingRegistrationStateCount == _deviceResults.Count)
            {
                lines.Add(
                    $"Registration: no stored state for {_deviceResults.Count}/{_deviceResults.Count} devices; expected before first provisioning");
            }
        }
        else if (RegistrationState is not null)
        {
            var assignedCount = string.Equals(
                RegistrationState.Status,
                "assigned",
                StringComparison.OrdinalIgnoreCase)
                ? 1
                : 0;
            var intendedHubMatchCount = CountFindings(
                "registrationState.assignedHubIntent",
                "passed");
            var assignmentMismatchCount = CountFindings(
                "registrationState.assignedHubIntent",
                "warning");
            if (!string.IsNullOrWhiteSpace(intendedHub))
            {
                lines.Add(string.Empty);
                lines.Add("| Comparison | Prediction | Actual |");
                lines.Add("|---|---|---|");
                lines.Add($"| Registration | Device completes assignment | {assignedCount}/1 assigned |");
                lines.Add(
                    $"| Hub assignment | `{intendedHub}` | " +
                    $"{intendedHubMatchCount}/1 matches, {assignmentMismatchCount} mismatches |");
            }
            else
            {
                lines.Add($"Registration: {assignedCount}/1 assigned");
            }
        }
        else if (_findings.Any(finding =>
            string.Equals(
                finding.CheckId,
                "registrationState.available",
                StringComparison.Ordinal) &&
            string.Equals(
                finding.Status,
                "informational",
                StringComparison.OrdinalIgnoreCase)))
        {
            lines.Add("Registration: no stored state for 1/1 device; expected before first provisioning");
        }

        var blockers = SummarizeFindings("blocker");
        if (blockers.Count > 0)
        {
            lines.Add(string.Empty);
            lines.Add("Blockers:");
            lines.AddRange(blockers.Select(blocker => $"- {blocker}"));
        }

        var warnings = SummarizeFindings("warning", "unverified");
        if (warnings.Count > 0)
        {
            lines.Add(string.Empty);
            lines.Add("Warnings:");
            lines.AddRange(warnings.Select(warning => $"- {warning}"));
        }
        else if (warningCount > 0 || isPartial)
        {
            lines.Add(string.Empty);
            lines.Add("Warnings: see detailed findings.");
        }

        lines.Add(string.Empty);
        lines.Add("Read-only validation; no enrollments, registrations, or devices were modified.");
        return string.Join(Environment.NewLine, lines);
    }

    private int CountDeviceFindings(string checkId, string status) =>
        _deviceResults.Count(result =>
            result.Findings.Any(finding =>
                string.Equals(finding.CheckId, checkId, StringComparison.Ordinal) &&
                string.Equals(finding.Status, status, StringComparison.OrdinalIgnoreCase)));

    private int CountFindings(string checkId, string status) =>
        _findings.Count(finding =>
            string.Equals(finding.CheckId, checkId, StringComparison.Ordinal) &&
            string.Equals(finding.Status, status, StringComparison.OrdinalIgnoreCase));

    private List<string> SummarizeFindings(params string[] statuses) =>
        [
            .. _findings
                .Concat(_deviceResults.SelectMany(result => result.Findings))
                .Where(finding => statuses.Contains(
                    finding.Status,
                    StringComparer.OrdinalIgnoreCase))
                .Select(SummarizeFinding)
                .Distinct(StringComparer.Ordinal)
        ];

    private static string SummarizeFinding(OnboardingValidationFinding finding) =>
        finding.CheckId switch
        {
            "x509.certificateValidity" =>
                "One or more certificates expire within 30 days.",
            "device.retryReadiness" =>
                "Device SDK and firmware retry behavior was not assessed.",
            "registrationState.assignedHubIntent" =>
                "One or more devices are assigned to a different Hub than intended.",
            _ => finding.Message
        };

    private static string FormatReadiness(
        string? overallReadiness,
        string validationResult) =>
        overallReadiness switch
        {
            "ready" => "READY",
            "readyWithWarnings" => "READY WITH WARNINGS",
            "blocked" => "BLOCKED",
            _ when string.Equals(
                validationResult,
                "issuesFound",
                StringComparison.OrdinalIgnoreCase) => "BLOCKED",
            _ when string.Equals(
                validationResult,
                "indeterminate",
                StringComparison.OrdinalIgnoreCase) => "INDETERMINATE",
            _ => "READY"
        };
}
