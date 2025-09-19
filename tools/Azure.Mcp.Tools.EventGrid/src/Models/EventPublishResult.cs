// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.EventGrid.Models;

public record EventPublishResult(
    string Status,
    string Message,
    int PublishedEventCount,
    string? OperationId = null,
    DateTime? PublishedAt = null);
