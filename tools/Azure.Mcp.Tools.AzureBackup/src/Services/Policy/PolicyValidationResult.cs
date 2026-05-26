// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Services.Policy;

/// <summary>
/// Aggregate outcome of <see cref="PolicyCreateValidator"/>. When invalid the
/// command surfaces every <see cref="Issues"/> entry to the caller in a single response.
/// </summary>
public sealed record PolicyValidationResult(bool IsValid, IReadOnlyList<PolicyValidationIssue> Issues)
{
    public static PolicyValidationResult Ok { get; } = new(true, []);

    public static PolicyValidationResult Fail(IReadOnlyList<PolicyValidationIssue> issues) => new(false, issues);
}
