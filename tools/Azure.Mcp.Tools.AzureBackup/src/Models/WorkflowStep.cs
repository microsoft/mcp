// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Models;

public sealed record WorkflowStep(
    string StepName,
    string Status,
    string? Detail);
