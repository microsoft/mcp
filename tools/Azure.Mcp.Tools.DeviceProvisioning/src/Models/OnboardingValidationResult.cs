// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.DeviceProvisioning.Models;

public record OnboardingValidationResult(
    string ValidationResult,
    string ValidationCompleteness,
    string TargetType,
    string TargetId,
    string ServiceName,
    string CustomerSummary,
    string IdScope,
    string ServiceState,
    string ProvisioningState,
    string AttestationType,
    string EffectiveAllocationPolicy,
    int PassedCount,
    int BlockerCount,
    int WarningCount,
    int InformationalCount,
    int UnverifiedCount,
    List<OnboardingValidationFinding> Findings,
    List<OnboardingValidationExecutionLimitation> ExecutionLimitations,
    OnboardingRegistrationState? RegistrationState,
    List<string> Limitations,
    string? OverallReadiness = null,
    int? DeviceCount = null,
    int? ReadyDeviceCount = null,
    int? ReadyWithWarningsDeviceCount = null,
    int? BlockedDeviceCount = null,
    List<OnboardingDeviceValidationResult>? DeviceResults = null);
