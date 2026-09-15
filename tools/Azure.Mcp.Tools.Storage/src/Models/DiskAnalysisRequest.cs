// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Storage.Models;

public sealed class DiskAnalysisRequest
{
    public required string ResourceId { get; init; }

    public string[]? SubResourceIds { get; init; }

    public string? IssueStartTime { get; init; }

    public string? IssueEndTime { get; init; }
}
