// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;
using System.Text.Json.Nodes;
using Azure.Mcp.Tools.SreAgent.Options;
using Azure.Mcp.Tools.SreAgent.Options.Incidents;
using Azure.Mcp.Tools.SreAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.SreAgent.Commands.Incidents;

[CommandMetadata(Id = "78c3a0cc-9185-44bf-93db-11be7f39a9b4", Name = "setup-servicenow", Title = "Set up ServiceNow Connector", Description = "Create a ServiceNow MCP connector using credentials from environment variables.", Destructive = false, Idempotent = true, OpenWorld = false, ReadOnly = false, Secret = true, LocalRequired = false)]
public sealed class IncidentsSetupServicenowCommand(ILogger<IncidentsSetupServicenowCommand> logger, ISreAgentService sreAgentService) : BaseSreAgentCommand<IncidentConnectorServiceNowOptions>
{
    private readonly ILogger<IncidentsSetupServicenowCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;
    protected override void RegisterOptions(Command command) { base.RegisterOptions(command); command.Options.Add(SreAgentPortedOptionDefinitions.Agent); command.Options.Add(SreAgentPortedOptionDefinitions.Name); command.Options.Add(SreAgentPortedOptionDefinitions.InstanceUrl); command.Options.Add(SreAgentPortedOptionDefinitions.AuthType); command.Options.Add(SreAgentPortedOptionDefinitions.TokenEnv); command.Options.Add(SreAgentPortedOptionDefinitions.UsernameEnv); command.Options.Add(SreAgentPortedOptionDefinitions.PasswordEnv); }
    protected override IncidentConnectorServiceNowOptions BindOptions(ParseResult parseResult) { var o = base.BindOptions(parseResult); o.Agent = parseResult.GetValueOrDefault<string>(SreAgentOptionDefinitions.AgentNameName); o.Name = parseResult.GetValueOrDefault<string>(SreAgentPortedOptionDefinitions.NameName); o.InstanceUrl = parseResult.GetValueOrDefault<string>(SreAgentPortedOptionDefinitions.InstanceUrlName); o.AuthType = parseResult.GetValueOrDefault<string>(SreAgentPortedOptionDefinitions.AuthTypeName); o.TokenEnv = parseResult.GetValueOrDefault<string>(SreAgentPortedOptionDefinitions.TokenEnvName); o.UsernameEnv = parseResult.GetValueOrDefault<string>(SreAgentPortedOptionDefinitions.UsernameEnvName); o.PasswordEnv = parseResult.GetValueOrDefault<string>(SreAgentPortedOptionDefinitions.PasswordEnvName); return o; }
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid) return context.Response; var o = BindOptions(parseResult);
        try
        {
            if (!Uri.TryCreate(o.InstanceUrl, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps || !uri.Host.EndsWith(".service-now.com", StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("ServiceNow instance URL must be an https URL on *.service-now.com.");
            try { await _sreAgentService.CallAgentDataPlaneAsync(o.Subscription!, o.Agent!, o.ResourceGroup, $"/api/v2/extendedAgent/connectors/{Uri.EscapeDataString(o.Name!)}", HttpMethod.Get, tenant: o.Tenant, retryPolicy: o.RetryPolicy, cancellationToken: cancellationToken); SreAgentPortedCommandHelpers.SetTextResult(context.Response, $"Connector '{o.Name}' already exists. Use `connectors -> test` to verify, or `connectors -> delete` to recreate."); return context.Response; } catch (HttpRequestException) { }
            var normalized = o.InstanceUrl!.TrimEnd('/');
            var props = new JsonObject { ["type"] = "http", ["endpoint"] = $"{normalized}/api/sn_mcp/mcp" };
            if (string.Equals(o.AuthType, "BearerToken", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(o.TokenEnv)) throw new ArgumentException("tokenEnv is required for BearerToken auth");
                var token = Environment.GetEnvironmentVariable(o.TokenEnv); if (string.IsNullOrWhiteSpace(token)) throw new InvalidOperationException($"ServiceNow bearer token environment variable '{o.TokenEnv}' is not set.");
                props["authType"] = "BearerToken"; props["bearerToken"] = token;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(o.UsernameEnv) || string.IsNullOrWhiteSpace(o.PasswordEnv)) throw new ArgumentException("usernameEnv and passwordEnv are required for BasicAuth");
                var username = Environment.GetEnvironmentVariable(o.UsernameEnv); var password = Environment.GetEnvironmentVariable(o.PasswordEnv); if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password)) throw new InvalidOperationException("ServiceNow username/password environment variables are not set.");
                props["authType"] = "CustomHeaders"; props["headers"] = new JsonObject { ["Authorization"] = $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"))}" };
            }
            var payload = SreAgentPortedCommandHelpers.ConnectorPayload(o.Name!, normalized, props);
            await _sreAgentService.CallAgentDataPlaneAsync(o.Subscription!, o.Agent!, o.ResourceGroup, $"/api/v2/extendedAgent/connectors/{Uri.EscapeDataString(o.Name!)}", HttpMethod.Put, payload.ToJsonString(), o.Tenant, o.RetryPolicy, cancellationToken);
            SreAgentPortedCommandHelpers.SetTextResult(context.Response, $"✅ ServiceNow connector '{o.Name}' created ({normalized}).\n\n**Next steps:**\n1. Run `connectors -> test` to verify the connection\n2. Add ServiceNow tools to your agent via `yaml -> apply`\n3. Create an incident response plan with `incidents -> create_plan`");
        }
        catch (Exception ex) { _logger.LogError(ex, "Error setting up ServiceNow connector"); HandleException(context, ex); }
        return context.Response;
    }
}
