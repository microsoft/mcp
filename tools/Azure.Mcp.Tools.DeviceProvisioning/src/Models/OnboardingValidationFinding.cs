// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.DeviceProvisioning.Models;

public record OnboardingValidationFinding(
    string CheckId,
    string Status,
    string Message,
    string? Remediation = null);
