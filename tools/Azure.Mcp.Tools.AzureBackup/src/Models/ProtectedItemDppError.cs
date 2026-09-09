// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Models;

public sealed record ProtectedItemDppError(
    string? Code,
    string? Message,
    IReadOnlyList<string>? RecommendedAction,
    string? Target,
    bool? IsRetryable,
    bool? IsUserError,
    IReadOnlyList<ProtectedItemDppError>? Details,
    ProtectedItemDppError? InnerError,
    IReadOnlyDictionary<string, string>? Properties);
