// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.SreAgent.Options;

namespace Azure.Mcp.Tools.SreAgent.Options.Incidents;

public class IncidentPlanCreateOptions : BaseSreAgentOptions
{
    public string? Name { get; set; }
    public string? Severity { get; set; }
    public string? TriggerCondition { get; set; }
    public string[]? Services { get; set; }
    public string[]? Steps { get; set; }
    public string? Escalation { get; set; }
    public string? RunbookUrl { get; set; }
    public string? AgentMode { get; set; }
}

public class IncidentRemoteOptions : BaseSreAgentOptions;

public class IncidentConnectorPagerDutyOptions : BaseSreAgentOptions
{
    public string? Name { get; set; }
    public string? ApiKeyEnv { get; set; }
    public string? Subdomain { get; set; }
}

public class IncidentConnectorServiceNowOptions : BaseSreAgentOptions
{
    public string? Name { get; set; }
    public string? InstanceUrl { get; set; }
    public string? AuthType { get; set; }
    public string? TokenEnv { get; set; }
    public string? UsernameEnv { get; set; }
    public string? PasswordEnv { get; set; }
}

public class IncidentCreateOptions : BaseSreAgentOptions
{
    public string? Severity { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string[]? Services { get; set; }
}
