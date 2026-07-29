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
    Id = "659a3697-9c8c-46e1-b568-9b929d637cb4",
    Name = "list",
    Title = "List Active Incidents",
    Description = "List active incidents on an SRE Agent. Returns open incident threads with title, status, affected services, and investigation details.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class IncidentsActiveListCommand(ILogger<IncidentsActiveListCommand> logger, ISreAgentService sreAgentService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<BaseSreAgentOptions, SreAgentTextResult>(subscriptionResolver)
{
    private readonly ILogger<IncidentsActiveListCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, BaseSreAgentOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var endpoint = await SreAgentCommandHelpers.ResolveAgentEndpointAsync(
                _sreAgentService,
                options,
                cancellationToken);
            var threads = await _sreAgentService.ListIncidentThreadsAsync(endpoint, options.Tenant, cancellationToken);
            var keywords = new[] { "incident", "🚨", "outage", "alert", "critical", "crash", "failure" };
            var incidents = threads.Where(t =>
                !string.IsNullOrWhiteSpace(t.Status?.IncidentStatus?.IncidentId)
                || !string.IsNullOrWhiteSpace(t.Status?.IncidentStatus?.Status)
                || t.Status?.ActionsStatus?.HasCriticalActions == true
                || keywords.Any(k => $"{t.Title ?? string.Empty} {t.StartMessage?.Text ?? string.Empty}".Contains(k, StringComparison.OrdinalIgnoreCase))).ToList();
            if (incidents.Count == 0)
            {
                SreAgentPortedCommandHelpers.SetTextResult(context.Response, "No active incidents found. Use create_incident to start an incident investigation.");
                return context.Response;
            }
            var lines = new List<string> { "# Active Incidents", string.Empty };
            foreach (var t in incidents)
            {
                var title = t.Title ?? (t.StartMessage?.Text?.Length > 80 ? t.StartMessage.Text[..80] : t.StartMessage?.Text) ?? "Untitled";
                var agent = t.StartMessage?.Author?.DisplayName ?? "unknown";
                var parts = new List<string>();
                if (t.Status?.ActionsStatus?.HasCriticalActions == true)
                    parts.Add("⚠️ Critical");
                if (t.Status?.ActionsStatus?.HasWarningActions == true)
                    parts.Add("⚡ Warning");
                if (!string.IsNullOrWhiteSpace(t.Status?.IncidentStatus?.Status))
                    parts.Add(t.Status.IncidentStatus.Status);
                var modified = DateTimeOffset.TryParse(t.ModifiedTimestamp, out var dto) ? $" | Updated: {dto.LocalDateTime}" : string.Empty;
                lines.Add($"- **{title}** ({t.Id})");
                lines.Add($"  Status: {(parts.Count > 0 ? string.Join(", ", parts) : "Active")} | Agent: {agent}{modified}");
            }
            lines.Add(string.Empty);
            lines.Add("Use get_thread to see full details, or send_message to provide updates.");
            SreAgentPortedCommandHelpers.SetTextResult(context.Response, string.Join('\n', lines));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing active incidents");
            HandleException(context, ex);
        }
        return context.Response;
    }
}
