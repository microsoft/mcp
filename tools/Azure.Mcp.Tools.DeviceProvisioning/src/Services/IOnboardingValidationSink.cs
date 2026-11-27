// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.DeviceProvisioning.Services;

internal interface IOnboardingValidationSink
{
    void AddFinding(
        string checkId,
        string status,
        string message,
        string? remediation = null);

    void AddExecutionLimitation(
        string operation,
        string reason,
        string message,
        string? requiredAction = null);

    void AddLimitation(string limitation);
}
