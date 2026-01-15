// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Policy.Models;

/// <summary>
/// Represents a policy definition model reserved for future policy-related commands and functionality.
/// This class is intentionally defined but may not yet be referenced by the current implementation.
/// </summary>
public class PolicyDefinition
{
    /// <summary>The ID of the policy definition.</summary>
    public string? Id { get; set; }

    /// <summary>The name of the policy definition.</summary>
    public string? Name { get; set; }

    /// <summary>The type of the policy definition.</summary>
    public string? Type { get; set; }

    /// <summary>The display name of the policy definition.</summary>
    public string? DisplayName { get; set; }

    /// <summary>The description of the policy definition.</summary>
    public string? Description { get; set; }

    /// <summary>The policy type (e.g., BuiltIn, Custom, Static).</summary>
    public string? PolicyType { get; set; }

    /// <summary>The policy mode (e.g., All, Indexed).</summary>
    public string? Mode { get; set; }

    /// <summary>The policy rule as JSON string.</summary>
    public string? PolicyRule { get; set; }

    /// <summary>The parameters of the policy definition as JSON string.</summary>
    public string? Parameters { get; set; }

    /// <summary>The metadata of the policy definition as JSON string.</summary>
    public string? Metadata { get; set; }
}
