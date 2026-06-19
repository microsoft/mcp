// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.SreAgent.Options.Incidents;

public sealed class IncidentConnectorServiceNowOptions : ISubscriptionOption
{
    [Option(SreAgentOptionDefinitions.AgentDescription)]
    public required string Agent { get; set; }

    [Option(SreAgentOptionDefinitions.NameDescription)]
    public required string Name { get; set; }

    [Option("ServiceNow instance URL.")]
    public required string InstanceUrl { get; set; }

    [Option(SreAgentOptionDefinitions.AuthTypeDescription)]
    public required string AuthType { get; set; }

    [Option("Environment variable containing bearer token.")]
    public string? TokenEnv { get; set; }

    [Option("Environment variable containing username.")]
    public string? UsernameEnv { get; set; }

    [Option("Environment variable containing password.")]
    public string? PasswordEnv { get; set; }

    [Option(OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [Option(OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(OptionDescriptions.ResourceGroup)]
    public string? ResourceGroup { get; set; }

    [Option(Name = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
