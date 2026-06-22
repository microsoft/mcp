// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Microsoft.Mcp.Core.Models.Option;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Authorization.Options;

public class RoleAssignmentApprovalApproveOptions : SubscriptionOptions
{
    [JsonPropertyName(OptionDefinitions.Authorization.ScopeName)]
    public string? Scope { get; set; }

    [JsonPropertyName(AuthorizationOptionDefinitions.ApprovalName)]
    public string? Approval { get; set; }

    [JsonPropertyName(AuthorizationOptionDefinitions.StageName)]
    public string? Stage { get; set; }

    [JsonPropertyName(AuthorizationOptionDefinitions.JustificationName)]
    public string? Justification { get; set; }
}
