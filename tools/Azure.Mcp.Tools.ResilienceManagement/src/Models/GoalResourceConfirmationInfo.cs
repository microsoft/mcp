// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.ResilienceManagement.Models;

public sealed record GoalResourceConfirmationInfo(
    string SolutionDisplayName,
    string ConfirmationStatus,
    string? ReasonForRequestingConfirmation);
