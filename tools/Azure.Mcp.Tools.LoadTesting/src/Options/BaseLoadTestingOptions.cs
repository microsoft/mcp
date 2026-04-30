// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.LoadTesting.Options;

public class BaseLoadTestingOptions 
{
    public string? Subscription { get; set; }

    [Option(
        Name = "resourcegroup",  // overriding the default of `resource-group`
        Description = "Name of the resource group."
    )]
    public string? ResourceGroup { get; set; }

    [Option(Hidden = true)]
    public string? Tenant { get; set; }

    public AuthMethod? AuthMethod { get; set; }

    // rich objects are supported.e
    // their properties are filled from suffixed parameters. e.g.
    //   --retry-policy-delay-seconds 5
    // a local Option attribute can override the prefix
    // an option attribute in the RetryPolicyOptions can override the suffix and sub option settings
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
