// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Mcp.Tools.SreAgent.Models;
using Azure.Mcp.Tools.SreAgent.Options;
using Azure.Mcp.Tools.SreAgent.Options.Incidents;
using Azure.Mcp.Tools.SreAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.SreAgent.Commands.Incidents;

[CommandMetadata(Id = "234f234b-76fd-4874-909a-d16a30db6187", Name = "create", Title = "Create Incident", Description = "Create an incident investigation thread for an agent.", Destructive = false, Idempotent = false, OpenWorld = false, ReadOnly = false, Secret = false, LocalRequired = false)]
public sealed class IncidentsCreateCommand(ILogger<IncidentsCreateCommand> logger, ISreAgentService sreAgentService) : BaseSreAgentCommand<IncidentCreateOptions>
{
    private readonly ILogger<IncidentsCreateCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;
    protected override void RegisterOptions(Command command) { base.RegisterOptions(command); command.Options.Add(SreAgentPortedOptionDefinitions.Agent); command.Options.Add(SreAgentPortedOptionDefinitions.Severity); command.Options.Add(SreAgentPortedOptionDefinitions.Title); command.Options.Add(SreAgentPortedOptionDefinitions.Description); command.Options.Add(SreAgentPortedOptionDefinitions.Services); }
    protected override IncidentCreateOptions BindOptions(ParseResult parseResult) { var o = base.BindOptions(parseResult); o.Agent = parseResult.GetValueOrDefault<string>(SreAgentOptionDefinitions.AgentNameName); o.Severity = parseResult.GetValueOrDefault<string>(SreAgentPortedOptionDefinitions.SeverityName); o.Title = parseResult.GetValueOrDefault<string>(SreAgentPortedOptionDefinitions.TitleName); o.Description = parseResult.GetValueOrDefault<string>(SreAgentPortedOptionDefinitions.DescriptionName); o.Services = parseResult.GetValueOrDefault<string[]>(SreAgentPortedOptionDefinitions.ServicesName); return o; }
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid) return context.Response; var o = BindOptions(parseResult);
        try
        {
            var user = Environment.GetEnvironmentVariable("USER") ?? Environment.GetEnvironmentVariable("USERNAME") ?? "mcp-user";
            var prompt = string.Join('\n', new[] { $"🚨 INCIDENT: {o.Title}", $"Severity: {o.Severity!.ToUpperInvariant()}", o.Services?.Length > 0 ? $"Affected services: {string.Join(", ", o.Services)}" : string.Empty, string.Empty, o.Description!, string.Empty, "Investigate this incident. Identify root cause, assess impact, and recommend remediation steps.", "Check relevant incident response plans if available." }.Where(x => x.Length > 0));
            var payload = new JsonObject { ["startMessage"] = new JsonObject { ["text"] = prompt, ["userId"] = user, ["displayName"] = user == "mcp-user" ? "MCP User" : user, ["agent"] = o.Agent } };
            var json = await _sreAgentService.CallAgentDataPlaneAsync(o.Subscription!, o.Agent!, o.ResourceGroup, "/api/v1/threads", HttpMethod.Post, payload.ToJsonString(), o.Tenant, o.RetryPolicy, cancellationToken);
            var thread = JsonSerializer.Deserialize(json, SreAgentJsonContext.Default.IncidentThreadResponse);
            if (string.IsNullOrWhiteSpace(thread?.Id)) throw new InvalidOperationException("Incident thread created but no ID returned");
            SreAgentPortedCommandHelpers.SetTextResult(context.Response, $"✅ Incident created: {o.Title}\n\n- **Thread ID:** {thread.Id}\n- **Severity:** {o.Severity}\n- **Agent:** {o.Agent}\n{(o.Services?.Length > 0 ? $"- **Services:** {string.Join(", ", o.Services)}\n" : string.Empty)}\nThe agent is investigating. Use get_thread to check progress, or send_message to provide additional context.");
        }
        catch (Exception ex) { _logger.LogError(ex, "Error creating incident"); HandleException(context, ex); }
        return context.Response;
    }
}
