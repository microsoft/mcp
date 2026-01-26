// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.AzureMigrate.Options.PlatformLandingZone;
using Azure.Mcp.Tools.AzureMigrate.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AzureMigrate.Commands.PlatformLandingZone;

/// <summary>
/// Command to get platform landing zone modification guidance and recommendations.
/// </summary>
public sealed class GetGuidanceCommand(ILogger<GetGuidanceCommand> logger)
    : BaseAzureMigrateCommand<GetGuidanceOptions>()
{
    private const string CommandTitle = "Get Platform Landing Zone Modification Guidance";

    /// <inheritdoc/>
    public override string Id => "d4e8c9b2-5f3a-4d1c-8b7e-2a9f1c6d5e4b";

    /// <inheritdoc/>
    public override string Name => "getguidance";

    /// <inheritdoc/>
    public override string Description =>
        """
        Get how-to guidance for modifying, configuring, or customizing an existing Platform Landing Zone.
        Use this tool when user asks "how do I", "show me how to", "get guidance for", or asks about 
        disabling, enabling, turning off, changing, or modifying Landing Zone settings.
        
        **Use this tool for questions about:**
        - How to turn off or disable Bastion, DDoS, DNS, gateways, Defender, or monitoring
        - How to change IP addresses, CIDR ranges, network topology, or regions
        - How to modify policies, enable zero trust, or update management groups
        - How to change resource naming patterns or conventions
        - Finding or searching for specific policies within a Landing Zone
        - Listing all available policies by archetype
        
        **Available scenarios:**
        - bastion: Turn off Bastion host
        - ddos: Enable or disable DDoS protection plan
        - dns: Turn off Private DNS zones and resolvers
        - gateways: Turn off Virtual Network Gateways (VPN/ExpressRoute)
        - ip-addresses: Adjust CIDR ranges and IP address space
        - regions: Add or remove secondary regions
        - resource-names: Update resource naming prefixes and suffixes
        - management-groups: Customize management group names and IDs
        - policy-enforcement: Change policy enforcement mode to DoNotEnforce
        - policy-assignment: Remove or disable a policy assignment
        - ama: Turn off Azure Monitoring Agent
        - amba: Deploy Azure Monitoring Baseline Alerts
        - defender: Turn off Defender Plans
        - zero-trust: Implement Zero Trust Networking
        - slz: Implement Sovereign Landing Zone controls
        
        **For policy searches:**
        - Use policy-name to search for a specific policy
        - Use list-policies=true to list ALL policies by archetype
        """;

    /// <inheritdoc/>
    public override string Title => CommandTitle;

    /// <inheritdoc/>
    public override ToolMetadata Metadata => new()
    {
        Destructive = true,
        Idempotent = true,
        OpenWorld = true,
        ReadOnly = false,
        LocalRequired = true,
        Secret = false
    };

    /// <inheritdoc/>
    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(PlatformLandingZoneOptionDefinitions.Scenario);
        command.Options.Add(PlatformLandingZoneOptionDefinitions.PolicyName);
        command.Options.Add(PlatformLandingZoneOptionDefinitions.ListPolicies);
    }

    /// <inheritdoc/>
    protected override GetGuidanceOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Scenario = parseResult.GetValueOrDefault(PlatformLandingZoneOptionDefinitions.Scenario);
        options.PolicyName = parseResult.GetValueOrDefault(PlatformLandingZoneOptionDefinitions.PolicyName);
        options.ListPolicies = parseResult.GetValueOrDefault(PlatformLandingZoneOptionDefinitions.ListPolicies);
        return options;
    }

    /// <inheritdoc/>
    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        ParseResult parseResult,
        CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            return context.Response;

        var options = BindOptions(parseResult);

        try
        {
            IPlatformLandingZoneGuidanceService service = context.GetService<IPlatformLandingZoneGuidanceService>();
            var response = new StringBuilder();

            var guidance = await service.GetGuidanceAsync(options.Scenario!, cancellationToken);
            response.AppendLine(guidance);

            if (options.ListPolicies)
            {
                Dictionary<string, List<string>> allPolicies = await service.GetAllPoliciesAsync(cancellationToken);
                response.AppendLine("\n--- All Policies by Archetype ---");
                foreach (var (archetype, policies) in allPolicies.OrderBy(kv => kv.Key))
                {
                    response.AppendLine($"\n{archetype}:");
                    foreach (var policy in policies.OrderBy(p => p))
                        response.AppendLine($"  - {policy}");
                }
            }

            if (!string.IsNullOrWhiteSpace(options.PolicyName) &&
                options.Scenario is "policy-enforcement" or "policy-assignment")
            {
                List<PlatformLandingZoneGuidanceService.PolicyLocationResult> locations = await service.SearchPoliciesAsync(options.PolicyName, cancellationToken);
                if (locations.Count > 0)
                {
                    response.AppendLine("\n--- Matching Policies ---");
                    foreach (var loc in locations)
                    {
                        response.AppendLine($"Policy: {loc.PolicyName}");
                        response.AppendLine($"  Found in archetypes: {string.Join(", ", loc.Archetypes)}");
                        response.AppendLine($"  Override file: config/lib/archetype_definitions/{{archetype}}_alz_archetype_override.yml");
                    }
                }
                else
                {
                    response.AppendLine($"\nNo policies matching '{options.PolicyName}' found. Use 'list-policies' parameter to see all available policies.");
                }
            }

            context.Response.Results = ResponseResult.Create(
                new GetGuidanceCommandResult(response.ToString()),
                AzureMigrateJsonContext.Default.GetGuidanceCommandResult);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching guidance for scenario: {Scenario}", options.Scenario);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record GetGuidanceCommandResult(
        [property: JsonPropertyName("guidance")] string Guidance);
}
