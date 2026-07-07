// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;
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
    Id = "78c3a0cc-9185-44bf-93db-11be7f39a9b4",
    Name = "setup_servicenow",
    Title = "Setup ServiceNow Connector",
    Description = "Connect an SRE Agent to ServiceNow. Creates a ServiceNow MCP connector to enable incident management integration using credentials from environment variables.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = true,
    LocalRequired = false)]
public sealed class IncidentsSetupServicenowCommand(ILogger<IncidentsSetupServicenowCommand> logger, ISreAgentService sreAgentService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<IncidentConnectorServiceNowOptions, SreAgentTextResult>(subscriptionResolver)
{
    private readonly ILogger<IncidentsSetupServicenowCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    public override void ValidateOptions(IncidentConnectorServiceNowOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (!Uri.TryCreate(options.InstanceUrl, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps ||
            !(uri.Host.EndsWith(".service-now.com", StringComparison.OrdinalIgnoreCase) ||
            uri.Host.EndsWith(".servicenowservices.com", StringComparison.OrdinalIgnoreCase)))
        {
            validationResult.Errors.Add("ServiceNow instance URL must be an https URL on *.service-now.com or *.servicenowservices.com.");
        }

        if (string.Equals(options.AuthType, "BearerToken", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(options.TokenEnv))
            {
                validationResult.Errors.Add("tokenEnv is required for BearerToken auth");
            }
            else
            {
                if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(options.TokenEnv)))
                {
                    validationResult.Errors.Add($"ServiceNow bearer token environment variable '{options.TokenEnv}' is not set.");
                }
            }
        }
        else
        {
            if (string.IsNullOrWhiteSpace(options.UsernameEnv) || string.IsNullOrWhiteSpace(options.PasswordEnv))
            {
                validationResult.Errors.Add("usernameEnv and passwordEnv are required for BasicAuth");
            }
            else
            {
                if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(options.UsernameEnv)) ||
                    string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(options.PasswordEnv)))
                {
                    validationResult.Errors.Add("ServiceNow username/password environment variables are not set.");
                }
            }
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, IncidentConnectorServiceNowOptions options, CancellationToken cancellationToken)
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
            catch (HttpRequestException) { }

            var normalized = options.InstanceUrl.TrimEnd('/');
            var ext = new Dictionary<string, object>
            {
                ["type"] = "http",
                ["endpoint"] = $"{normalized}/api/sn_mcp/mcp"
            };
            if (string.Equals(options.AuthType, "BearerToken", StringComparison.OrdinalIgnoreCase))
            {
                ext["authType"] = "BearerToken";
                ext["bearerToken"] = Environment.GetEnvironmentVariable(options.TokenEnv!)!;
            }
            else
            {
                var username = Environment.GetEnvironmentVariable(options.UsernameEnv!)!;
                var password = Environment.GetEnvironmentVariable(options.PasswordEnv!)!;
                ext["authType"] = "CustomHeaders";
                ext["headers"] = new Dictionary<string, object>
                {
                    ["Authorization"] = $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"))}"
                };
            }

            var connector = new AgentConnectorEnvelope
            {
                Name = options.Name,
                Properties = new AgentConnector
                {
                    Name = options.Name,
                    DataConnectorType = "Mcp",
                    DataSource = normalized,
                    Identity = string.Empty,
                    ExtendedProperties = ext
                }
            };
            await _sreAgentService.CreateOrUpdateConnectorAsync(options.Subscription!, resourceGroup, options.Agent, options.Name, connector, options.Tenant, cancellationToken);
            SreAgentPortedCommandHelpers.SetTextResult(context.Response, $"✅ ServiceNow connector '{options.Name}' created ({normalized}).\n\n**Next steps:**\n1. Run `connectors -> test` to verify the connection\n2. Add ServiceNow tools to your agent via `yaml -> apply`\n3. Create an incident response plan with `incidents -> create_plan`");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting up ServiceNow connector");
            HandleException(context, ex);
        }
        return context.Response;
    }
}
