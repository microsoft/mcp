// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Models;

public sealed record AdvisorQueryPage<T>(
    List<T> Results,
    bool AreResultsTruncated,
    string? ContinuationToken = null);
