// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Authorization.Options;

public class RoleAssignmentListOptions : SubscriptionOptions
{
    [JsonPropertyName(OptionDefinitions.Authorization.ScopeName)]
    public string? Scope { get; set; }
}
