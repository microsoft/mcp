// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Azure.Mcp.Tools.AzureMigrate.Commands.PlatformLandingZone;
using Azure.Mcp.Tools.AzureMigrate.Helpers;
using Azure.Mcp.Tools.AzureMigrate.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Commands.Descriptors;

namespace Azure.Mcp.Tools.AzureMigrate;

public sealed class AzureMigrateRegistration : IAreaRegistration
{
    public static string AreaName => "azuremigrate";

    public static string AreaTitle => "Azure Landing Zone Generation and Guidance";

    public static CommandCategory Category => CommandCategory.AzureServices;

    public static CommandGroupDescriptor GetCommandDescriptors() => new()
    {
        Name = AreaName,
        Description = "Azure Landing Zone Generation and Guidance operations.",
        Title = AreaTitle,
        SubGroups =
        [
            new CommandGroupDescriptor
            {
                Name = "platformlandingzone",
                Description = "Platform Landing Zone operations - Commands for generating new platform landing zones and providing guidance on configuration and customization.",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "d4e8c9b2-5f3a-4d1c-8b7e-2a9f1c6d5e4b",
                        Name = "getguidance",
                        Description = "Get how-to guidance for modifying, configuring, or customizing an existing Platform Landing Zone. Use this tool when user asks \"how do I\", \"show me how to\", \"get guidance for\", or asks about disabling, enabling, turning off, changing, or modifying Landing Zone settings. **Use this tool for questions about:** - How to turn off or disable Bastion, DDoS, DNS, gateways, Defender, or monitoring - How to change IP addresses, CIDR ranges, network topology, or regions - How to modify policies, enable zero trust, or update management groups - How to change resource naming patterns or conventions - Finding or searching for specific policies within a Landing Zone - Listing all available policies by archetype **Available scenarios:** - bastion: Turn off Bastion host - ddos: Enable or disable DDoS protection plan - dns: Turn off Private DNS zones and resolvers - gateways: Turn off Virtual Network Gateways (VPN/ExpressRoute) - ip-addresses: Adjust CIDR ranges and IP address space - regions: Add or remove secondary regions - resource-names: Update resource naming prefixes and suffixes - management-groups: Customize management group names and IDs - policy-enforcement: Change policy enforcement mode to DoNotEnforce - policy-assignment: Remove or disable a policy assignment - ama: Turn off Azure Monitoring Agent - amba: Deploy Azure Monitoring Baseline Alerts - defender: Turn off Defender Plans - zero-trust: Implement Zero Trust Networking - slz: Implement Sovereign Landing Zone controls **For policy searches:** - Use policy-name to search for a specific policy - Use list-policies=true to list ALL policies by archetype",
                        Title = "Getguidance",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = true,
                            Idempotent = true,
                            OpenWorld = true,
                            ReadOnly = false,
                            LocalRequired = true,
                            Secret = false,
                        },
                        Options =
                        [
                            new OptionDescriptor
                            {
                                Name = "scenario",
                                Description = "The modification scenario key. Valid values: resource-names, management-groups, ddos, bastion, dns, gateways, regions, ip-addresses, policy-enforcement, policy-assignment, ama, amba, defender, zero-trust, slz.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "policy-name",
                                Description = "The policy assignment name to look up (e.g., 'Enable-DDoS-VNET'). Used with policy-enforcement or policy-assignment scenarios.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "list-policies",
                                Description = "Set to true to list all available policies organized by archetype. Useful for finding the exact policy name.",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Global,
                        HandlerType = nameof(GetGuidanceCommand)
                    },
                    new CommandDescriptor
                    {
                        Id = "a7f3b8c1-9e2d-4f6a-8b3c-5d1e7f9a2c4b",
                        Name = "request",
                        Description = "Generate and download platform landing zone configurations for Azure Migrate projects. Updates parameters, check existing landing zones, and view parameters status. **Actions:** - createmigrateproject: Create a new Azure Migrate project if one doesn't exist (requires location parameter) - check: Check if a platform landing zone already exists - update: Update all parameters for generation (collect ALL params in one call) - generate: Generate the platform landing zone - download: Download generated files to local workspace - status: View cached parameters **Context (required for most actions):** - subscription, resourceGroup, migrateProjectName **Create Azure Migrate Parameters (for 'createmigrateproject' action):** - subscription, resourceGroup, migrateProjectName, location **Generation Parameters (for 'update' action - collect ALL at once from user):** | Parameter | Options | Default | |-----------|---------|----------| | regionType | single, multi | single | | firewallType | azurefirewall, nva | azurefirewall | | networkArchitecture | hubspoke, vwan | hubspoke | | versionControlSystem | local, github, azuredevops | local | | regions | comma-separated (e.g., eastus,westus) | eastus | | environmentName | any string | prod | | organizationName | any string | contoso | | identitySubscriptionId | GUID | (uses main subscription) | | managementSubscriptionId | GUID | (uses main subscription) | | connectivitySubscriptionId | GUID | (uses main subscription) | **Workflow:** 1. Ask the user if they want to create a new Azure Migrate project or use an existing one. If creating, collect location parameter and create the project. 2. action='createmigrateproject' - Create a new Azure Migrate project only if the user doesn't have one already. Requires location parameter. 3. action='check' - See if one already exists 4. action='update' with ALL parameters - Ask user to confirm defaults or provide values 5. action='generate' - Create the landing zone 6. action='download' - Get the files 7. Extract zip to workspace root **IMPORTANT:** When using 'update', collect ALL parameters from the user in ONE call. Show them the defaults and ask which ones they want to change.",
                        Title = "Request",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = true,
                            Idempotent = true,
                            OpenWorld = false,
                            ReadOnly = false,
                            LocalRequired = true,
                            Secret = false,
                        },
                        Options =
                        [
                            new OptionDescriptor
                            {
                                Name = "resource-group",
                                Description = "The name of the Azure resource group. This is a logical container for Azure resources.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "action",
                                Description = "The action to perform: 'update' (set parameters), 'check' (check existing platform landing zone), 'generate' (generate platform landing zone), 'download' (get download instructions), 'status' (view parameter status).",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "region-type",
                                Description = "The region type for the Platform Landing Zone. Valid values: 'single', 'multi'.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "firewall-type",
                                Description = "The firewall type for the Platform Landing Zone. Valid values: 'azurefirewall', 'nva'.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "network-architecture",
                                Description = "The network architecture for the Platform Landing Zone. Valid values: 'hubspoke', 'vwan'.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "identity-subscription-id",
                                Description = "The Azure subscription ID for the identity management group in Platform Landing Zone (GUID format).",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "management-subscription-id",
                                Description = "The Azure subscription ID for the management group in Platform Landing Zone (GUID format).",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "connectivity-subscription-id",
                                Description = "The Azure subscription ID for the connectivity group in Platform Landing Zone (GUID format).",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "security-subscription-id",
                                Description = "The Azure subscription ID for security resources in Platform Landing Zone (GUID format).",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "regions",
                                Description = "Comma-separated list of Azure regions for Platform Landing Zone (e.g., 'eastus,westus2').",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "environment-name",
                                Description = "The environment name for the Platform Landing Zone.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "version-control-system",
                                Description = "The version control system for the Platform Landing Zone. Valid values: 'local', 'github', 'azuredevops'.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "organization-name",
                                Description = "The organization name for the Platform Landing Zone.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "migrate-project-name",
                                Description = "The Azure Migrate project name for Platform Landing Zone generation context.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "migrate-project-resource-id",
                                Description = "The full resource ID of the Azure Migrate project for Platform Landing Zone (alternative to subscription/resourceGroup/migrateProjectName).",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "location",
                                Description = "The Azure region location for creating new resources (e.g., 'eastus', 'westus2'). Required for 'createmigrateproject' action.",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(RequestCommand)
                    },
                ],
            },
        ],
    };

    public static void RegisterServices(IServiceCollection services)
    {
        // Register shared helpers
        services.AddSingleton<AzureHttpHelper>();
        services.AddSingleton<AzureMigrateProjectHelper>();
        // Register guidance service and command
        services.AddSingleton<IPlatformLandingZoneGuidanceService, PlatformLandingZoneGuidanceService>();
        services.AddHttpClient<PlatformLandingZoneGuidanceService>();
        services.AddSingleton<GetGuidanceCommand>();
        // Register platform landing zone service and command
        services.AddSingleton<IPlatformLandingZoneService, PlatformLandingZoneService>();
        services.AddSingleton<RequestCommand>();
    }

    public static IBaseCommand ResolveHandler(string handlerTypeName, IServiceProvider serviceProvider) =>
        handlerTypeName switch
        {
            nameof(GetGuidanceCommand) => serviceProvider.GetRequiredService<GetGuidanceCommand>(),
            nameof(RequestCommand) => serviceProvider.GetRequiredService<RequestCommand>(),
            _ => throw new InvalidOperationException($"Unknown handler type '{{handlerTypeName}}' in azuremigrate area.")
        };
}
