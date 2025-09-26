// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Core.Commands;

/// <summary>
/// Marks a command as essential functionality that should always be available
/// regardless of namespace filtering or server mode. Essential commands
/// are required for basic Azure MCP functionality.
/// </summary>
/// <remarks>
/// Essential commands include:
/// - Subscription management (subscription_list, subscription_get)
/// - Resource group management (group_list, group_create, etc.)
/// - Server management commands
///
/// These commands are always included even when namespace filtering is applied
/// because other commands depend on them for basic functionality.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class EssentialAttribute : Attribute
{
    /// <summary>
    /// Gets or sets a description of why this command is considered essential.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EssentialAttribute"/> class.
    /// </summary>
    public EssentialAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EssentialAttribute"/> class
    /// with a description.
    /// </summary>
    /// <param name="description">A description of why this command is essential.</param>
    public EssentialAttribute(string description)
    {
        Description = description;
    }
}
