// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Core.Commands;

/// <summary>
/// Identifies the API plane a tool acts against.
/// </summary>
/// <remarks>
/// Classify by the API the tool acts against to produce the result the user asked for. Calls made
/// only as setup, such as resolving a subscription or looking up a resource to read its endpoint,
/// do not count toward the plane. Nearly every data-plane tool resolves its target through ARM
/// first, so counting those lookups would make almost every tool <see cref="Both"/> and the
/// classification would stop discriminating.
/// See <c>docs/design/operation-plane-metadata.md</c>.
/// </remarks>
public enum ToolOperationPlane
{
    /// <summary>
    /// The tool has not been classified. This is an unset marker rather than a valid answer, and is
    /// a validation failure.
    /// </summary>
    Unspecified,

    /// <summary>
    /// The tool performs its action against a service data-plane API. Any ARM lookup the tool
    /// performs is only setup, such as reading a resource's endpoint before calling it.
    /// </summary>
    Data,

    /// <summary>
    /// The tool performs its action against Azure Resource Manager or another management-plane API,
    /// such as listing or creating resources.
    /// </summary>
    Control,

    /// <summary>
    /// The tool performs two distinct user-facing actions that fall on different planes, such as
    /// creating a resource and then performing a workload operation against it in one call. This is
    /// rare; a control-plane lookup that merely sets up a data-plane action is
    /// <see cref="Data"/>, not <see cref="Both"/>.
    /// </summary>
    Both,

    /// <summary>
    /// No service plane applies, because the tool calls no service. Examples include tools that
    /// return embedded documentation, generate content locally, or control the MCP server itself.
    /// Unlike <see cref="Unspecified"/>, this is a deliberate classification.
    /// </summary>
    NotApplicable
}
