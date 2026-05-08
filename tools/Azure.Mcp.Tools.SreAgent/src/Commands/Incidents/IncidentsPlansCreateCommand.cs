// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Nodes;
using Azure.Mcp.Tools.SreAgent.Options;
using Azure.Mcp.Tools.SreAgent.Options.Incidents;
using Azure.Mcp.Tools.SreAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.SreAgent.Commands.Incidents;

[CommandMetadata(Id = "84d958db-8de0-456d-a1d5-99d372f33c80", Name = "plans-create", Title = "Create Incident Response Plan", Description = "Create and enable an incident response plan with a filter and handler.", Destructive = false, Idempotent = false, OpenWorld = false, ReadOnly = false, Secret = false, LocalRequired = false)]
public sealed class IncidentsPlansCreateCommand(ILogger<IncidentsPlansCreateCommand> logger, ISreAgentService sreAgentService) : BaseSreAgentCommand<IncidentPlanCreateOptions>
{
    private static readonly Dictionary<string, string[]> SeverityToPriorities = new(StringComparer.OrdinalIgnoreCase) { ["critical"] = ["P1"], ["high"] = ["P1", "P2"], ["medium"] = ["P2", "P3"], ["low"] = ["P3", "P4"] };
    private readonly ILogger<IncidentsPlansCreateCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;
    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(SreAgentPortedOptionDefinitions.Agent); command.Options.Add(SreAgentPortedOptionDefinitions.Name); command.Options.Add(SreAgentPortedOptionDefinitions.Severity); command.Options.Add(SreAgentPortedOptionDefinitions.TriggerCondition); command.Options.Add(SreAgentPortedOptionDefinitions.Services); command.Options.Add(SreAgentPortedOptionDefinitions.Steps); command.Options.Add(SreAgentPortedOptionDefinitions.Escalation); command.Options.Add(SreAgentPortedOptionDefinitions.RunbookUrl); command.Options.Add(SreAgentPortedOptionDefinitions.AgentMode);
    }
    protected override IncidentPlanCreateOptions BindOptions(ParseResult parseResult)
    {
        var o = base.BindOptions(parseResult); o.Agent = parseResult.GetValueOrDefault<string>(SreAgentOptionDefinitions.AgentNameName); o.Name = parseResult.GetValueOrDefault<string>(SreAgentPortedOptionDefinitions.NameName); o.Severity = parseResult.GetValueOrDefault<string>(SreAgentPortedOptionDefinitions.SeverityName); o.TriggerCondition = parseResult.GetValueOrDefault<string>(SreAgentPortedOptionDefinitions.TriggerConditionName); o.Services = parseResult.GetValueOrDefault<string[]>(SreAgentPortedOptionDefinitions.ServicesName); o.Steps = parseResult.GetValueOrDefault<string[]>(SreAgentPortedOptionDefinitions.StepsName); o.Escalation = parseResult.GetValueOrDefault<string>(SreAgentPortedOptionDefinitions.EscalationName); o.RunbookUrl = parseResult.GetValueOrDefault<string>(SreAgentPortedOptionDefinitions.RunbookUrlName); o.AgentMode = parseResult.GetValueOrDefault<string>(SreAgentPortedOptionDefinitions.AgentModeName); return o;
    }
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid) return context.Response; var o = BindOptions(parseResult);
        try
        {
            var planId = SreAgentPortedCommandHelpers.SanitizeKebabCase(o.Name!); var filterId = planId; var handlerId = $"{planId}-handler";
            var priorities = SeverityToPriorities.TryGetValue(o.Severity!, out var p) ? p : [];
            var filterPayload = new JsonObject { ["Id"] = filterId, ["ImpactedService"] = o.Services![0], ["Priorities"] = new JsonArray(priorities.Select(x => JsonValue.Create(x)).ToArray()), ["TitleContains"] = o.TriggerCondition, ["AgentMode"] = o.AgentMode ?? "autonomous", ["HandlingAgent"] = o.Agent };
            await _sreAgentService.CallAgentDataPlaneAsync(o.Subscription!, o.Agent!, o.ResourceGroup, $"/api/v1/incidentplayground/filters/{Uri.EscapeDataString(filterId)}", HttpMethod.Put, filterPayload.ToJsonString(), o.Tenant, o.RetryPolicy, cancellationToken);
            var custom = $"Trigger condition: {o.TriggerCondition}\nSeverity: {o.Severity}\nServices: {string.Join(", ", o.Services!)}" + (string.IsNullOrWhiteSpace(o.Escalation) ? string.Empty : $"\n\nEscalation procedure: {o.Escalation}") + (string.IsNullOrWhiteSpace(o.RunbookUrl) ? string.Empty : $"\n\nRunbook: {o.RunbookUrl}");
            var handlerPayload = new JsonObject { ["id"] = handlerId, ["name"] = o.Name, ["description"] = $"Incident response plan for {string.Join(", ", o.Services!)} ({o.Severity} severity)", ["incidentFilterId"] = filterId, ["incidentProcessingGuide"] = new JsonArray(o.Steps!.Select(x => JsonValue.Create(x)).ToArray()), ["tools"] = new JsonArray(), ["incidents"] = new JsonArray(), ["customInstructions"] = custom };
            try { await _sreAgentService.CallAgentDataPlaneAsync(o.Subscription!, o.Agent!, o.ResourceGroup, $"/api/v1/incidentplayground/handlers/{Uri.EscapeDataString(handlerId)}", HttpMethod.Put, handlerPayload.ToJsonString(), o.Tenant, o.RetryPolicy, cancellationToken); }
            catch { await _sreAgentService.CallAgentDataPlaneAsync(o.Subscription!, o.Agent!, o.ResourceGroup, $"/api/v1/incidentplayground/filters/{Uri.EscapeDataString(filterId)}", HttpMethod.Delete, tenant: o.Tenant, retryPolicy: o.RetryPolicy, cancellationToken: cancellationToken); throw; }
            try { await _sreAgentService.CallAgentDataPlaneAsync(o.Subscription!, o.Agent!, o.ResourceGroup, $"/api/v1/incidentplayground/filters/{Uri.EscapeDataString(filterId)}/enable", HttpMethod.Post, tenant: o.Tenant, retryPolicy: o.RetryPolicy, cancellationToken: cancellationToken); }
            catch (Exception ex) { SreAgentPortedCommandHelpers.SetTextResult(context.Response, $"⚠️ Incident response plan '{o.Name}' created but the filter could not be auto-enabled: {ex.Message}\n\nEnable it manually in the portal or via the API: POST /api/v1/incidentplayground/filters/{filterId}/enable"); return context.Response; }
            SreAgentPortedCommandHelpers.SetTextResult(context.Response, $"✅ Incident response plan '{o.Name}' created and enabled.\n\n**Filter:** {filterId} (matches incidents with title containing \"{o.TriggerCondition}\", priorities: {string.Join(", ", priorities)}, service: {o.Services![0]})\n**Handler:** {handlerId} ({o.Steps!.Length} response steps)\n**Agent:** {o.Agent} (mode: {o.AgentMode ?? "autonomous"})\n\nIncoming incidents matching the filter will automatically trigger the '{o.Agent}' agent with the configured response steps. The plan is visible in the portal under Incident Management.");
        }
        catch (Exception ex) { _logger.LogError(ex, "Error creating incident response plan"); HandleException(context, ex); }
        return context.Response;
    }
}
