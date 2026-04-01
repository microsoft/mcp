// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Microsoft.Mcp.Core.Models.Option;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Authorization.Options;

public class RoleAssignmentListOptions : SubscriptionOptions
{
    [JsonPropertyName(OptionDefinitions.Authorization.ScopeName)]
    public string? Scope { get; set; }
}
