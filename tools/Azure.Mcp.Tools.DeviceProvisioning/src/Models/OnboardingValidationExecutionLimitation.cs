// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.DeviceProvisioning.Models;

public record OnboardingValidationExecutionLimitation(
    string Operation,
    string Reason,
    string Message,
    string? RequiredAction = null);
