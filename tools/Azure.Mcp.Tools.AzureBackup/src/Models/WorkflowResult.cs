// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Models;

public sealed record WorkflowResult(
    string Status,
    string WorkflowName,
    IReadOnlyList<WorkflowStep> Steps,
    string? Message);

public sealed record WorkflowStep(
    string StepName,
    string Status,
    string? Detail);
