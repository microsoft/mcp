// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Policy.Options.Definition;

public class PolicyDefinitionGetOptions : SubscriptionOptions
{
    /// <summary>The name of the policy definition to retrieve.</summary>
    public string? DefinitionName { get; set; }

    /// <summary>The management group ID to retrieve the policy definition from.</summary>
    public string? ManagementGroup { get; set; }
}
