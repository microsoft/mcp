// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.SreAgent.Models;
using Azure.Mcp.Tools.SreAgent.Options;
using Azure.Mcp.Tools.SreAgent.Options.Incidents;
using Azure.Mcp.Tools.SreAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.SreAgent.Commands.Incidents;

[CommandMetadata(Id = "84d958db-8de0-456d-a1d5-99d372f33c80", Name = "plans_create", Title = "Create Incident Response Plan", Description = "Create and enable an incident response plan with a filter and handler.", Destructive = false, Idempotent = false, OpenWorld = false, ReadOnly = false, Secret = false, LocalRequired = false)]
public sealed class IncidentsPlansCreateCommand(ILogger<IncidentsPlansCreateCommand> logger, ISreAgentService sreAgentService) : SreAgentDataPlaneCommand<IncidentPlanCreateOptions>
{
    private static readonly Dictionary<string, string[]> SeverityToPriorities = new(StringComparer.OrdinalIgnoreCase)
    {
        ["critical"] = ["P1"],
        ["high"] = ["P1", "P2"],
        ["medium"] = ["P2", "P3"],
        ["low"] = ["P3", "P4"]
    };
    private readonly ILogger<IncidentsPlansCreateCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(SreAgentOptionDefinitions.Name);
        command.Options.Add(SreAgentOptionDefinitions.Severity);
        command.Options.Add(SreAgentOptionDefinitions.TriggerCondition);
        command.Options.Add(SreAgentOptionDefinitions.Services);
        command.Options.Add(SreAgentOptionDefinitions.Steps);
        command.Options.Add(SreAgentOptionDefinitions.Escalation);
        command.Options.Add(SreAgentOptionDefinitions.RunbookUrl);
        command.Options.Add(SreAgentOptionDefinitions.AgentMode);
    }

    protected override IncidentPlanCreateOptions BindOptions(ParseResult parseResult)
    {
        var o = base.BindOptions(parseResult);
        o.Name = parseResult.GetValueOrDefault(SreAgentOptionDefinitions.Name) ?? string.Empty;
        o.Severity = parseResult.GetValueOrDefault(SreAgentOptionDefinitions.Severity);
        o.TriggerCondition = parseResult.GetValueOrDefault(SreAgentOptionDefinitions.TriggerCondition);
        o.Services = parseResult.GetValueOrDefault(SreAgentOptionDefinitions.Services);
        o.Steps = parseResult.GetValueOrDefault(SreAgentOptionDefinitions.Steps);
        o.Escalation = parseResult.GetValueOrDefault(SreAgentOptionDefinitions.Escalation);
        o.RunbookUrl = parseResult.GetValueOrDefault(SreAgentOptionDefinitions.RunbookUrl);
        o.AgentMode = parseResult.GetValueOrDefault(SreAgentOptionDefinitions.AgentMode);
        return o;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            return context.Response;
        var o = BindOptions(parseResult);
        try
        {
            var endpoint = await ResolveEndpointAsync(_sreAgentService, o, cancellationToken);
            var planId = SreAgentPortedCommandHelpers.SanitizeKebabCase(o.Name);
            var filterId = planId;
            var handlerId = $"{planId}-handler";
            var priorities = SeverityToPriorities.TryGetValue(o.Severity!, out var p) ? p : [];

            var filterPayload = new IncidentFilterPayload
            {
                Id = filterId,
                ImpactedService = o.Services![0],
                Priorities = [.. priorities],
                TitleContains = o.TriggerCondition,
                AgentMode = o.AgentMode ?? "autonomous",
                HandlingAgent = o.Agent
            };
            await _sreAgentService.CreateOrUpdateIncidentFilterAsync(endpoint, filterId, filterPayload, o.Tenant, cancellationToken);

            var custom = $"Trigger condition: {o.TriggerCondition}\nSeverity: {o.Severity}\nServices: {string.Join(", ", o.Services!)}"
                + (string.IsNullOrWhiteSpace(o.Escalation) ? string.Empty : $"\n\nEscalation procedure: {o.Escalation}")
                + (string.IsNullOrWhiteSpace(o.RunbookUrl) ? string.Empty : $"\n\nRunbook: {o.RunbookUrl}");
            var handlerPayload = new IncidentHandler
            {
                Id = handlerId,
                Name = o.Name,
                Description = $"Incident response plan for {string.Join(", ", o.Services!)} ({o.Severity} severity)",
                IncidentFilterId = filterId,
                IncidentProcessingGuide = [.. o.Steps!],
                Tools = [],
                Incidents = [],
                CustomInstructions = custom
            };
            try
            {
                await _sreAgentService.CreateOrUpdateIncidentHandlerAsync(endpoint, handlerId, handlerPayload, o.Tenant, cancellationToken);
            }
            catch
            {
                await _sreAgentService.DeleteIncidentFilterAsync(endpoint, filterId, o.Tenant, cancellationToken);
                throw;
            }

            try
            {
                await _sreAgentService.EnableIncidentFilterAsync(endpoint, filterId, o.Tenant, cancellationToken);
            }
            catch (Exception ex)
            {
                SreAgentPortedCommandHelpers.SetTextResult(context.Response, $"⚠️ Incident response plan '{o.Name}' created but the filter could not be auto-enabled: {ex.Message}\n\nEnable it manually in the portal or via the API: POST /api/v1/incidentplayground/filters/{filterId}/enable");
                return context.Response;
            }

            SreAgentPortedCommandHelpers.SetTextResult(context.Response, $"✅ Incident response plan '{o.Name}' created and enabled.\n\n**Filter:** {filterId} (matches incidents with title containing \"{o.TriggerCondition}\", priorities: {string.Join(", ", priorities)}, service: {o.Services![0]})\n**Handler:** {handlerId} ({o.Steps!.Length} response steps)\n**Agent:** {o.Agent} (mode: {o.AgentMode ?? "autonomous"})\n\nIncoming incidents matching the filter will automatically trigger the '{o.Agent}' agent with the configured response steps. The plan is visible in the portal under Incident Management.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating incident response plan");
            HandleException(context, ex);
        }
        return context.Response;
    }
}
