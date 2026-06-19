// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.RegularExpressions;
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
    Id = "49de8921-8331-4328-9de2-f8b216af7dbf",
    Name = "setup_pagerduty",
    Title = "Setup PagerDuty Connector",
    Description = "Connect an SRE Agent to PagerDuty. Creates a PagerDuty MCP connector to enable incident alerting and management integration using an API key from an environment variable.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = true,
    LocalRequired = false)]
public sealed partial class IncidentsSetupPagerdutyCommand(ILogger<IncidentsSetupPagerdutyCommand> logger, ISreAgentService sreAgentService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<IncidentConnectorPagerDutyOptions, SreAgentTextResult>(subscriptionResolver)
{
    private readonly ILogger<IncidentsSetupPagerdutyCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    public override void ValidateOptions(IncidentConnectorPagerDutyOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (!string.IsNullOrWhiteSpace(options.Subdomain) && !MyRegex().IsMatch(options.Subdomain))
        {
            validationResult.Errors.Add("PagerDuty subdomain may only contain letters, numbers, and hyphens.");
        }

        var apiKey = Environment.GetEnvironmentVariable(options.ApiKeyEnv);
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            validationResult.Errors.Add($"PagerDuty API key environment variable '{options.ApiKeyEnv}' is not set.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, IncidentConnectorPagerDutyOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var resourceGroup = await SreAgentCommandHelpers.ResolveAgentResourceGroupAsync(
                _sreAgentService,
                options,
                cancellationToken);
            try
            {
                await _sreAgentService.GetConnectorAsync(options.Subscription!, resourceGroup, options.Agent, options.Name, options.Tenant, cancellationToken);
                SreAgentPortedCommandHelpers.SetTextResult(context.Response, $"Connector '{options.Name}' already exists. Use `connectors -> test` to verify, or `connectors -> delete` to recreate.");
                return context.Response;
            }
            catch (HttpRequestException)
            {
            }

            var dataSource = string.IsNullOrWhiteSpace(options.Subdomain) ? "https://api.pagerduty.com" : $"https://{options.Subdomain}.pagerduty.com";
            var connector = new AgentConnectorEnvelope
            {
                Name = options.Name,
                Properties = new AgentConnector
                {
                    Name = options.Name,
                    DataConnectorType = "Mcp",
                    DataSource = dataSource,
                    Identity = string.Empty,
                    ExtendedProperties = new Dictionary<string, object>
                    {
                        ["type"] = "http",
                        ["endpoint"] = $"{dataSource}/mcp",
                        ["authType"] = "BearerToken",
                        ["bearerToken"] = Environment.GetEnvironmentVariable(options.ApiKeyEnv)!
                    }
                }
            };
            await _sreAgentService.CreateOrUpdateConnectorAsync(options.Subscription!, resourceGroup, options.Agent, options.Name, connector, options.Tenant, cancellationToken);
            SreAgentPortedCommandHelpers.SetTextResult(context.Response, $"✅ PagerDuty connector '{options.Name}' created (API key resolved from ${options.ApiKeyEnv}).\n\n**Next steps:**\n1. Run `connectors -> test` to verify the connection\n2. Add PagerDuty tools to your agent via `yaml -> apply`\n3. Create an incident response plan with `incidents -> create_plan`");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting up PagerDuty connector");
            HandleException(context, ex);
        }
        return context.Response;
    }

    [GeneratedRegex("^[a-zA-Z0-9-]+$")]
    private static partial Regex MyRegex();
}
