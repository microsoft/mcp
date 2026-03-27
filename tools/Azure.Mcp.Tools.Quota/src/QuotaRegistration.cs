// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Azure.Mcp.Tools.Quota.Commands.Region;
using Azure.Mcp.Tools.Quota.Commands.Usage;
using Azure.Mcp.Tools.Quota.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Commands.Descriptors;

namespace Azure.Mcp.Tools.Quota;

public sealed class QuotaRegistration : IAreaRegistration
{
    public static string AreaName => "quota";

    public static string AreaTitle => "Azure Quota Management";

    public static CommandCategory Category => CommandCategory.AzureServices;

    public static CommandGroupDescriptor GetCommandDescriptors() => new()
    {
        Name = AreaName,
        Description = "Quota commands for getting the available regions of specific Azure resource types",
        Title = AreaTitle,
        SubGroups =
        [
            new CommandGroupDescriptor
            {
                Name = "region",
                Description = "Region availability operations",
                SubGroups =
                [
                    new CommandGroupDescriptor
                    {
                        Name = "availability",
                        Description = "Region availability information",
                        Commands =
                        [
                            new CommandDescriptor
                            {
                                Id = "0b8902f5-3fd4-49d9-b73e-4cea88afdd62",
                                Name = "list",
                                Description = "Given a list of Azure resource types, this tool will return a list of regions where the resource types are available. Always get the user's subscription ID before calling this tool.",
                                Title = "List",
                                Annotations = new ToolAnnotations
                                {
                                    Destructive = false,
                                    Idempotent = true,
                                    OpenWorld = false,
                                    ReadOnly = true,
                                    LocalRequired = false,
                                    Secret = false,
                                },
                                Options =
                                [
                                    new OptionDescriptor
                                    {
                                        Name = "resource-types",
                                        Description = "Comma-separated list of Azure resource types to check available regions for. The valid Azure resource types. E.g. 'Microsoft.App/containerApps, Microsoft.Web/sites, Microsoft.CognitiveServices/accounts'.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "cognitive-service-model-name",
                                        Description = "Optional model name for cognitive services. Only needed when Microsoft.CognitiveServices is included in resource types.",
                                        TypeName = "string",
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "cognitive-service-model-version",
                                        Description = "Optional model version for cognitive services. Only needed when Microsoft.CognitiveServices is included in resource types.",
                                        TypeName = "string",
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "cognitive-service-deployment-sku-name",
                                        Description = "Optional deployment SKU name for cognitive services. Only needed when Microsoft.CognitiveServices is included in resource types.",
                                        TypeName = "string",
                                    },
                                ],
                                Kind = CommandKind.Subscription,
                                HandlerType = nameof(AvailabilityListCommand)
                            },
                        ],
                    },
                ],
            },
            new CommandGroupDescriptor
            {
                Name = "usage",
                Description = "Resource usage and quota operations",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "81f64603-5a56-4f74-90f8-395da69a99d3",
                        Name = "check",
                        Description = "This tool will check the usage and quota information for Azure resources in a region.",
                        Title = "Check",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = false,
                            Idempotent = true,
                            OpenWorld = false,
                            ReadOnly = true,
                            LocalRequired = false,
                            Secret = false,
                        },
                        Options =
                        [
                            new OptionDescriptor
                            {
                                Name = "region",
                                Description = "The valid Azure region where the resources will be deployed. E.g. 'eastus', 'westus', etc.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "resource-types",
                                Description = "The valid Azure resource types that are going to be deployed(comma-separated). E.g. 'Microsoft.App/containerApps,Microsoft.Web/sites,Microsoft.CognitiveServices/accounts', etc.",
                                TypeName = "string",
                                Required = true,
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(CheckCommand)
                    },
                ],
            },
        ],
    };

    public static void RegisterServices(IServiceCollection services)
    {
        services.AddTransient<IQuotaService, QuotaService>();
        services.AddTransient<CheckCommand>();
        services.AddTransient<AvailabilityListCommand>();
    }

    public static IBaseCommand ResolveHandler(string handlerTypeName, IServiceProvider serviceProvider) =>
        handlerTypeName switch
        {
            nameof(AvailabilityListCommand) => serviceProvider.GetRequiredService<AvailabilityListCommand>(),
            nameof(CheckCommand) => serviceProvider.GetRequiredService<CheckCommand>(),
            _ => throw new InvalidOperationException($"Unknown handler type '{{handlerTypeName}}' in quota area.")
        };
}
