// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Azure.Mcp.Core.Attributes;

/// <summary>
/// Indicates that this method is specifically designed for Azure Resource Graph queries only.
/// For direct resource operations, use ArmClient and SDK methods instead.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class ResourceGraphOnlyAttribute : Attribute
{
    /// <summary>
    /// Gets the reason why this method should only be used for Resource Graph queries.
    /// </summary>
    public string? Reason { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceGraphOnlyAttribute"/> class.
    /// </summary>
    public ResourceGraphOnlyAttribute() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceGraphOnlyAttribute"/> class with a specific reason.
    /// </summary>
    /// <param name="reason">The reason why this method should only be used for Resource Graph queries.</param>
    public ResourceGraphOnlyAttribute(string reason)
    {
        Reason = reason;
    }
}
