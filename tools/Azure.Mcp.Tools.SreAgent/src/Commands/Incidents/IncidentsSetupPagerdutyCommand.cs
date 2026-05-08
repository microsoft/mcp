// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Azure.Mcp.Tools.SreAgent.Options;
using Azure.Mcp.Tools.SreAgent.Options.Incidents;
using Azure.Mcp.Tools.SreAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.SreAgent.Commands.Incidents;

[CommandMetadata(Id = "49de8921-8331-4328-9de2-f8b216af7dbf", Name = "setup-pagerduty", Title = "Set up PagerDuty Connector", Description = "Create a PagerDuty MCP connector using an API key from an environment variable.", Destructive = false, Idempotent = true, OpenWorld = false, ReadOnly = false, Secret = true, LocalRequired = false)]
public sealed class IncidentsSetupPagerdutyCommand(ILogger<IncidentsSetupPagerdutyCommand> logger, ISreAgentService sreAgentService) : BaseSreAgentCommand<IncidentConnectorPagerDutyOptions>
{
    private readonly ILogger<IncidentsSetupPagerdutyCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;
    protected override void RegisterOptions(Command command) { base.RegisterOptions(command); command.Options.Add(SreAgentPortedOptionDefinitions.Agent); command.Options.Add(SreAgentPortedOptionDefinitions.Name); command.Options.Add(SreAgentPortedOptionDefinitions.ApiKeyEnv); command.Options.Add(SreAgentPortedOptionDefinitions.Subdomain); }
    protected override IncidentConnectorPagerDutyOptions BindOptions(ParseResult parseResult) { var o = base.BindOptions(parseResult); o.Agent = parseResult.GetValueOrDefault<string>(SreAgentOptionDefinitions.AgentNameName); o.Name = parseResult.GetValueOrDefault<string>(SreAgentPortedOptionDefinitions.NameName); o.ApiKeyEnv = parseResult.GetValueOrDefault<string>(SreAgentPortedOptionDefinitions.ApiKeyEnvName); o.Subdomain = parseResult.GetValueOrDefault<string>(SreAgentPortedOptionDefinitions.SubdomainName); return o; }
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid) return context.Response; var o = BindOptions(parseResult);
        try
        {
            if (!string.IsNullOrWhiteSpace(o.Subdomain) && !Regex.IsMatch(o.Subdomain, "^[a-zA-Z0-9-]+$")) throw new ArgumentException("PagerDuty subdomain may only contain letters, numbers, and hyphens.");
            var apiKey = Environment.GetEnvironmentVariable(o.ApiKeyEnv!); if (string.IsNullOrWhiteSpace(apiKey)) throw new InvalidOperationException($"PagerDuty API key environment variable '{o.ApiKeyEnv}' is not set.");
            try { await _sreAgentService.CallAgentDataPlaneAsync(o.Subscription!, o.Agent!, o.ResourceGroup, $"/api/v2/extendedAgent/connectors/{Uri.EscapeDataString(o.Name!)}", HttpMethod.Get, tenant: o.Tenant, retryPolicy: o.RetryPolicy, cancellationToken: cancellationToken); SreAgentPortedCommandHelpers.SetTextResult(context.Response, $"Connector '{o.Name}' already exists. Use `connectors -> test` to verify, or `connectors -> delete` to recreate."); return context.Response; } catch (HttpRequestException) { }
            var endpoint = string.IsNullOrWhiteSpace(o.Subdomain) ? "https://api.pagerduty.com" : $"https://{o.Subdomain}.pagerduty.com";
            var payload = SreAgentPortedCommandHelpers.ConnectorPayload(o.Name!, endpoint, new JsonObject { ["type"] = "http", ["endpoint"] = $"{endpoint}/mcp", ["authType"] = "BearerToken", ["bearerToken"] = apiKey });
            await _sreAgentService.CallAgentDataPlaneAsync(o.Subscription!, o.Agent!, o.ResourceGroup, $"/api/v2/extendedAgent/connectors/{Uri.EscapeDataString(o.Name!)}", HttpMethod.Put, payload.ToJsonString(), o.Tenant, o.RetryPolicy, cancellationToken);
            SreAgentPortedCommandHelpers.SetTextResult(context.Response, $"✅ PagerDuty connector '{o.Name}' created (API key resolved from ${o.ApiKeyEnv}).\n\n**Next steps:**\n1. Run `connectors -> test` to verify the connection\n2. Add PagerDuty tools to your agent via `yaml -> apply`\n3. Create an incident response plan with `incidents -> create_plan`");
        }
        catch (Exception ex) { _logger.LogError(ex, "Error setting up PagerDuty connector"); HandleException(context, ex); }
        return context.Response;
    }
}
