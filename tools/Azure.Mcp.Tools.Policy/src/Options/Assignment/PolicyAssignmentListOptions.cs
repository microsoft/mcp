// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Policy.Options.Assignment;

public class PolicyAssignmentListOptions : SubscriptionOptions
{
    [JsonPropertyName("scope")]
    public string? Scope { get; set; }
}
