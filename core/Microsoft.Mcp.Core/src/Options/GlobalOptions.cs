// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Models.Option;

namespace Microsoft.Mcp.Core.Options;

public class GlobalOptions
{
    [JsonPropertyName(OptionDefinitions.Common.ResourceGroupName)]
    public string? ResourceGroup { get; set; }

    [JsonPropertyName(OptionDefinitions.Common.TenantName)]
    public string? Tenant { get; set; }

    [JsonPropertyName(OptionDefinitions.Common.AuthMethodName)]
    public AuthMethod? AuthMethod { get; set; }

    public RetryPolicyOptions? RetryPolicy { get; set; }
}
