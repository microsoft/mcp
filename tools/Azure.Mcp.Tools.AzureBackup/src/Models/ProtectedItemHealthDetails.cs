// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Models;

public sealed record ProtectedItemHealthDetails(
    int? Code,
    string? Title,
    string? Message,
    IReadOnlyList<string>? Recommendations);
