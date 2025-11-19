// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Policy.Models;

public class PolicyAssignment
{
    /// <summary>The ID of the policy assignment.</summary>
    public string? Id { get; set; }

    /// <summary>The name of the policy assignment.</summary>
    public string? Name { get; set; }

    /// <summary>The type of the policy assignment.</summary>
    public string? Type { get; set; }

    /// <summary>The display name of the policy assignment.</summary>
    public string? DisplayName { get; set; }

    /// <summary>The policy definition ID.</summary>
    public string? PolicyDefinitionId { get; set; }

    /// <summary>The scope of the policy assignment.</summary>
    public string? Scope { get; set; }

    /// <summary>The enforcement mode of the policy assignment.</summary>
    public string? EnforcementMode { get; set; }

    /// <summary>The description of the policy assignment.</summary>
    public string? Description { get; set; }

    /// <summary>The metadata of the policy assignment.</summary>
    public string? Metadata { get; set; }

    /// <summary>The parameters of the policy assignment.</summary>
    public string? Parameters { get; set; }

    /// <summary>The identity of the policy assignment.</summary>
    public string? Identity { get; set; }

    /// <summary>The location of the managed identity.</summary>
    public string? Location { get; set; }
}
