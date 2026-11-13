// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.ResilienceManagement.Models;

public sealed record DrillRunReprotectResult(
    string OperationId,
    string Status,
    bool HasCompleted,
    int HttpStatus,
    string? LocationUri,
    string? AzureAsyncOperationUri);
