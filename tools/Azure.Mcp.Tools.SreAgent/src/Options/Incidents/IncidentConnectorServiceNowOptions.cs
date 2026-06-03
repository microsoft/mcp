// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.SreAgent.Options.Incidents;

public class IncidentConnectorServiceNowOptions : ISreAgentOption
{
    [Option("The name of the Azure SRE Agent resource to target.")]
    public string? Agent { get; set; }

    [Option(OptionDescriptions.ResourceGroup)]
    public string? ResourceGroup { get; set; }

    [Option(OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [Option(Name = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }

    [Option("The name of the SRE Agent item.")]
    public required string Name { get; set; }

    [Option("ServiceNow instance URL.")]
    public required string InstanceUrl { get; set; }

    [Option("The HTTP MCP connector authentication type.")]
    public string? AuthType { get; set; }

    [Option("Environment variable containing bearer token.")]
    public string? TokenEnv { get; set; }

    [Option("Environment variable containing username.")]
    public string? UsernameEnv { get; set; }

    [Option("Environment variable containing password.")]
    public string? PasswordEnv { get; set; }
}
