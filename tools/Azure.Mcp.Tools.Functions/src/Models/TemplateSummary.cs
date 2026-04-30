// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Functions.Models;

/// <summary>
/// Summary of a single function template, used in list mode.
/// The <see cref="TemplateName"/> is the value to pass to --template in get mode.
/// </summary>
public sealed class TemplateSummary
{
    public required string TemplateName { get; init; }

    public required string DisplayName { get; init; }

    public string? Description { get; init; }

    public string? Resource { get; init; }

    /// <summary>
    /// Infrastructure type. <see cref="InfrastructureType.None"/> means code-only,
    /// other values indicate the template includes infrastructure files and is azd-ready for deployment.
    /// </summary>
    public InfrastructureType Infrastructure { get; init; }
}
