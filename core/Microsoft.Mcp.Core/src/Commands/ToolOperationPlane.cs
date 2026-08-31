// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Core.Commands;

/// <summary>
/// Identifies the Azure API plane a tool targets.
/// </summary>
/// <remarks>
/// Classify by the tool's <em>deliverable</em>: the call that produces what the user asked for.
/// Calls made only to address or identify the target, such as resolving a subscription or looking
/// up a resource to read its endpoint, do not count toward the plane. Nearly every data-plane tool
/// resolves its target through ARM first, so counting those lookups would make almost every tool
/// <see cref="Both"/> and the classification would stop discriminating.
/// See <c>docs/design/operation-plane-metadata.md</c>.
/// </remarks>
public enum ToolOperationPlane
{
    /// <summary>
    /// The tool has not been classified. This is an unset marker rather than a valid answer, and is
    /// a validation failure for Azure service tools.
    /// </summary>
    Unspecified,

    /// <summary>
    /// The deliverable is a workload call against an Azure service data-plane API. Any ARM lookup the
    /// tool performs is only addressing, such as reading a resource's endpoint before calling it.
    /// </summary>
    Data,

    /// <summary>
    /// The deliverable is the Azure Resource Manager call itself, such as listing or creating
    /// resources.
    /// </summary>
    Control,

    /// <summary>
    /// The tool has two genuine user-facing deliverables that fall on different planes, such as
    /// creating a resource and then performing a workload operation against it in one call. This is
    /// rare; a control-plane lookup that merely supports a data-plane deliverable is
    /// <see cref="Data"/>, not <see cref="Both"/>.
    /// </summary>
    Both,

    /// <summary>
    /// No Azure plane applies, because the tool calls no Azure service. Examples include tools that
    /// return embedded documentation, generate content locally, or control the MCP server itself.
    /// Unlike <see cref="Unspecified"/>, this is a deliberate classification.
    /// </summary>
    NotApplicable
}
