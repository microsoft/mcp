// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.DeviceProvisioning.Models;

public record OnboardingDeviceValidationResult(
    string RegistrationId,
    string OverallReadiness,
    string ValidationResult,
    string ValidationCompleteness,
    int PassedCount,
    int BlockerCount,
    int WarningCount,
    int InformationalCount,
    int UnverifiedCount,
    List<OnboardingValidationFinding> Findings,
    List<OnboardingValidationExecutionLimitation> ExecutionLimitations,
    OnboardingRegistrationState? RegistrationState,
    List<string> Limitations);
