// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Policy.Models;

public class PolicyAssignment
{
<<<<<<< HEAD
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("policyDefinitionId")]
    public string? PolicyDefinitionId { get; set; }

    [JsonPropertyName("scope")]
    public string? Scope { get; set; }

    [JsonPropertyName("notScopes")]
    public List<string>? NotScopes { get; set; }

    [JsonPropertyName("parameters")]
    public JsonElement? Parameters { get; set; }

    [JsonPropertyName("enforcementMode")]
    public string? EnforcementMode { get; set; }

    [JsonPropertyName("metadata")]
    public JsonElement? Metadata { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("identity")]
    public JsonElement? Identity { get; set; }
=======
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
>>>>>>> main
}
