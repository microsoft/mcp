// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.DeviceProvisioning.Models;

namespace Azure.Mcp.Tools.DeviceProvisioning.Services;

internal sealed class OnboardingDeviceValidationBuilder(string registrationId)
    : IOnboardingValidationSink
{
    private readonly List<OnboardingValidationFinding> _findings = [];
    private readonly List<OnboardingValidationExecutionLimitation> _executionLimitations = [];
    private readonly List<string> _limitations = [];

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

    public OnboardingDeviceValidationResult Build()
    {
        var blockerCount = Count("blocker");
        var warningCount = Count("warning");
        var unverifiedCount = Count("unverified");
        var isPartial = _executionLimitations.Count > 0 || unverifiedCount > 0;
        var overallReadiness = blockerCount > 0
            ? "blocked"
            : warningCount > 0 || isPartial
                ? "readyWithWarnings"
                : "ready";

        return new OnboardingDeviceValidationResult(
            registrationId,
            overallReadiness,
            blockerCount > 0 ? "issuesFound" : "passed",
            isPartial ? "partial" : "complete",
            Count("passed"),
            blockerCount,
            warningCount,
            Count("informational"),
            unverifiedCount,
            _findings,
            _executionLimitations,
            RegistrationState,
            _limitations);
    }

    private int Count(string status) =>
        _findings.Count(finding =>
            string.Equals(finding.Status, status, StringComparison.OrdinalIgnoreCase));
}
