// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Core.Commands;

/// <summary>
/// Marks a command as an extension tool that provides additional functionality
/// beyond core Azure services. Extension tools are only included when explicitly
/// requested through configuration.
/// </summary>
/// <remarks>
/// Extension commands include:
/// - Azure Quick Review (azqr) tools
/// - Third-party integrations
/// - Experimental or preview functionality
///
/// These commands are excluded by default in namespace mode unless the "extension"
/// namespace is explicitly requested.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class ExtensionAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the category of extension (e.g., "analysis", "integration", "experimental").
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Gets or sets a description of what this extension provides.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtensionAttribute"/> class.
    /// </summary>
    public ExtensionAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtensionAttribute"/> class
    /// with a category.
    /// </summary>
    /// <param name="category">The category of extension.</param>
    public ExtensionAttribute(string category)
    {
        Category = category;
    }
}
