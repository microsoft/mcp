// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.SreAgent.Models;
using Azure.Mcp.Tools.SreAgent.Options;
using Azure.Mcp.Tools.SreAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.SreAgent.Commands.Incidents;

[CommandMetadata(
    Id = "ab471ff4-7b46-4a0c-a54a-9a0371dcdd01",
    Name = "plans_list",
    Title = "List Incident Response Plans",
    Description = "List incident response plans configured on an SRE Agent.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class IncidentsPlansListCommand(ILogger<IncidentsPlansListCommand> logger, ISreAgentService sreAgentService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<BaseSreAgentOptions, SreAgentTextResult>(subscriptionResolver)
{
    private readonly ILogger<IncidentsPlansListCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, BaseSreAgentOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var endpoint = await SreAgentCommandHelpers.ResolveAgentEndpointAsync(
                _sreAgentService,
                options,
                cancellationToken);
            var filters = (await _sreAgentService.ListIncidentFiltersAsync(endpoint, options.Tenant, cancellationToken))
                .Where(f => f.IsDeleted != true)
                .ToList();
            var handlers = await _sreAgentService.ListIncidentHandlersAsync(endpoint, options.Tenant, cancellationToken);
            SreAgentPortedCommandHelpers.SetTextResult(context.Response, FormatPlans(filters, handlers));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing incident response plans");
            HandleException(context, ex);
        }
        return context.Response;
    }

    private static string FormatPlans(List<IncidentFilter> filters, List<IncidentHandler> handlers)
    {
        if (filters.Count == 0 && handlers.Count == 0)
            return "No incident response plans found. Use `incidents -> create_plan` to create one.";
        var handlersByFilterId = handlers.Where(h => !string.IsNullOrWhiteSpace(h.IncidentFilterId)).GroupBy(h => h.IncidentFilterId!).ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
        var lines = new List<string> { "# Incident Response Plans", string.Empty };
        foreach (var filter in filters)
        {
            handlersByFilterId.TryGetValue(filter.Id ?? string.Empty, out var handler);
            var status = filter.IsEnabled == true ? "🟢 Enabled" : "🔴 Disabled";
            lines.Add($"## {handler?.Name ?? filter.Id} ({status})");
            lines.Add($"- **Filter ID:** {filter.Id}");
            if (!string.IsNullOrWhiteSpace(filter.ImpactedService))
                lines.Add($"- **Service:** {filter.ImpactedService}");
            if (filter.Priorities?.Count > 0)
                lines.Add($"- **Priorities:** {string.Join(", ", filter.Priorities)}");
            if (!string.IsNullOrWhiteSpace(filter.TitleContains))
                lines.Add($"- **Title match:** \"{filter.TitleContains}\"");
            if (!string.IsNullOrWhiteSpace(filter.HandlingAgent))
                lines.Add($"- **Agent:** {filter.HandlingAgent}");
            if (!string.IsNullOrWhiteSpace(filter.AgentMode))
                lines.Add($"- **Mode:** {filter.AgentMode}");
            lines.Add(handler is null ? "- **Handler:** ⚠️ None configured" : $"- **Handler:** {handler.Id} ({handler.IncidentProcessingGuide?.Count ?? 0} steps)");
            lines.Add(string.Empty);
        }
        var orphaned = handlers.Where(h => !filters.Any(f => string.Equals(f.Id, h.IncidentFilterId, StringComparison.OrdinalIgnoreCase))).ToList();
        if (orphaned.Count > 0)
        {
            lines.Add("## ⚠️ Orphaned Handlers (no matching filter)");
            lines.Add(string.Empty);
            foreach (var h in orphaned)
                lines.Add($"- **{h.Name ?? h.Id}** (filter: {h.IncidentFilterId})");
            lines.Add(string.Empty);
        }
        return string.Join('\n', lines);
    }
}
