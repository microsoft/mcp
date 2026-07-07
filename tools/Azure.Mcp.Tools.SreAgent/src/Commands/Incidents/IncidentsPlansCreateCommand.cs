// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.SreAgent.Models;
using Azure.Mcp.Tools.SreAgent.Options.Incidents;
using Azure.Mcp.Tools.SreAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.SreAgent.Commands.Incidents;

[CommandMetadata(
    Id = "84d958db-8de0-456d-a1d5-99d372f33c80",
    Name = "plans_create",
    Title = "Create Incident Response Plan",
    Description = "Create and enable an incident response plan with a filter and handler.",
    Destructive = false,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class IncidentsPlansCreateCommand(ILogger<IncidentsPlansCreateCommand> logger, ISreAgentService sreAgentService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<IncidentPlanCreateOptions, SreAgentTextResult>(subscriptionResolver)
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

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, IncidentPlanCreateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var endpoint = await SreAgentCommandHelpers.ResolveAgentEndpointAsync(
                _sreAgentService,
                options,
                cancellationToken);
            var planId = SreAgentPortedCommandHelpers.SanitizeKebabCase(options.Name);
            var filterId = planId;
            var handlerId = $"{planId}-handler";
            var priorities = SeverityToPriorities.TryGetValue(options.Severity, out var p) ? p : [];

            var filterPayload = new IncidentFilterPayload
            {
                Id = filterId,
                ImpactedService = options.Services[0],
                Priorities = [.. priorities],
                TitleContains = options.TriggerCondition,
                AgentMode = options.AgentMode ?? "autonomous",
                HandlingAgent = options.Agent
            };
            await _sreAgentService.CreateOrUpdateIncidentFilterAsync(endpoint, filterId, filterPayload, options.Tenant, cancellationToken);

            var custom = $"Trigger condition: {options.TriggerCondition}\nSeverity: {options.Severity}\nServices: {string.Join(", ", options.Services)}"
                + (string.IsNullOrWhiteSpace(options.Escalation) ? string.Empty : $"\n\nEscalation procedure: {options.Escalation}")
                + (string.IsNullOrWhiteSpace(options.RunbookUrl) ? string.Empty : $"\n\nRunbook: {options.RunbookUrl}");
            var handlerPayload = new IncidentHandler
            {
                Id = handlerId,
                Name = options.Name,
                Description = $"Incident response plan for {string.Join(", ", options.Services)} ({options.Severity} severity)",
                IncidentFilterId = filterId,
                IncidentProcessingGuide = [.. options.Steps],
                Tools = [],
                Incidents = [],
                CustomInstructions = custom
            };
            try
            {
                await _sreAgentService.CreateOrUpdateIncidentHandlerAsync(endpoint, handlerId, handlerPayload, options.Tenant, cancellationToken);
            }
            catch
            {
                await _sreAgentService.DeleteIncidentFilterAsync(endpoint, filterId, options.Tenant, cancellationToken);
                throw;
            }

            try
            {
                await _sreAgentService.EnableIncidentFilterAsync(endpoint, filterId, options.Tenant, cancellationToken);
            }
            catch (Exception ex)
            {
                SreAgentPortedCommandHelpers.SetTextResult(context.Response, $"⚠️ Incident response plan '{options.Name}' created but the filter could not be auto-enabled: {ex.Message}\n\nEnable it manually in the portal or via the API: POST /api/v1/incidentplayground/filters/{filterId}/enable");
                return context.Response;
            }

            SreAgentPortedCommandHelpers.SetTextResult(context.Response, $"✅ Incident response plan '{options.Name}' created and enabled.\n\n**Filter:** {filterId} (matches incidents with title containing \"{options.TriggerCondition}\", priorities: {string.Join(", ", priorities)}, service: {options.Services[0]})\n**Handler:** {handlerId} ({options.Steps.Length} response steps)\n**Agent:** {options.Agent} (mode: {options.AgentMode ?? "autonomous"})\n\nIncoming incidents matching the filter will automatically trigger the '{options.Agent}' agent with the configured response steps. The plan is visible in the portal under Incident Management.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating incident response plan");
            HandleException(context, ex);
        }
        return context.Response;
    }
}
