// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.FileShares.Models;

/// <summary>
/// Represents the result of a name availability check operation.
/// </summary>
public class NameAvailabilityResult
{
    /// <summary>
    /// Gets or sets a value indicating whether the requested name is available.
    /// </summary>
    public bool Available { get; set; }

    /// <summary>
    /// Gets or sets a message describing why the name is not available (if applicable).
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Gets or sets the reason why the name is not available (AlreadyExists, Invalid).
    /// </summary>
    public string? Reason { get; set; }
}
