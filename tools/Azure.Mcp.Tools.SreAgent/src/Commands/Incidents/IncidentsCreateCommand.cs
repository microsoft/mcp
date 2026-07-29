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
    Id = "234f234b-76fd-4874-909a-d16a30db6187",
    Name = "create",
    Title = "Create Incident",
    Description = "Create an incident investigation thread for an agent.",
    Destructive = false,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class IncidentsCreateCommand(ILogger<IncidentsCreateCommand> logger, ISreAgentService sreAgentService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<IncidentCreateOptions, SreAgentTextResult>(subscriptionResolver)
{
    private readonly ILogger<IncidentsCreateCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, IncidentCreateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var endpoint = await SreAgentCommandHelpers.ResolveAgentEndpointAsync(
                _sreAgentService,
                options,
                cancellationToken);
            var user = Environment.GetEnvironmentVariable("USER") ?? Environment.GetEnvironmentVariable("USERNAME") ?? "mcp-user";
            var prompt = string.Join('\n', new[]
            {
                $"🚨 INCIDENT: {options.Title}",
                $"Severity: {options.Severity.ToUpperInvariant()}",
                options.Services.Length > 0 ? $"Affected services: {string.Join(", ", options.Services)}" : string.Empty,
                string.Empty,
                options.Description,
                string.Empty,
                "Investigate this incident. Identify root cause, assess impact, and recommend remediation steps.",
                "Check relevant incident response plans if available."
            }.Where(x => x.Length > 0));
            var request = new IncidentThreadCreateRequest
            {
                StartMessage = new IncidentThreadStartMessage
                {
                    Text = prompt,
                    UserId = user,
                    DisplayName = user == "mcp-user" ? "MCP User" : user,
                    Agent = options.Agent
                }
            };
            var thread = await _sreAgentService.CreateIncidentThreadAsync(endpoint, request, options.Tenant, cancellationToken);
            if (string.IsNullOrWhiteSpace(thread?.Id))
            {
                throw new InvalidOperationException("Incident thread created but no ID returned");
            }
            SreAgentPortedCommandHelpers.SetTextResult(context.Response, $"✅ Incident created: {options.Title}\n\n- **Thread ID:** {thread.Id}\n- **Severity:** {options.Severity}\n- **Agent:** {options.Agent}\n{(options.Services.Length > 0 ? $"- **Services:** {string.Join(", ", options.Services)}\n" : string.Empty)}\nThe agent is investigating. Use get_thread to check progress, or send_message to provide additional context.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating incident");
            HandleException(context, ex);
        }
        return context.Response;
    }
}
