// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Azure.Mcp.Tools.Pricing.Commands;
using Azure.Mcp.Tools.Pricing.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Commands.Descriptors;

namespace Azure.Mcp.Tools.Pricing;

public sealed class PricingRegistration : IAreaRegistration
{
    public static string AreaName => "pricing";

    public static string AreaTitle => "Azure Retail Pricing";

    public static CommandCategory Category => CommandCategory.AzureServices;

    public static CommandGroupDescriptor GetCommandDescriptors() => new()
    {
        Name = AreaName,
        Description = "Azure Retail Pricing operations - Get Azure retail pricing for SKUs, services, regions and compare pricing across different options.",
        Title = AreaTitle,
        Commands =
        [
            new CommandDescriptor
            {
                Id = "c5a8f7d2-9e3b-4a1c-8d6f-2b5e9c4a7d3e",
                Name = "get",
                Description = "Get Azure retail pricing information. CRITICAL/MANDATORY: Do NOT call this tool if the user only specifies a broad service name (e.g., 'Virtual Machines', 'Storage', 'SQL Database') without a specific SKU. Instead, FIRST ask the user which specific SKU or tier they want pricing for. If the user asks to compare pricing across regions or SKUs without specifying exact ARM SKU names, ask them which specific SKUs they want to compare. Do NOT assume or pick default SKUs. Only call this tool AFTER the user provides a specific SKU (--sku) or confirms they want all pricing for that service. Requires at least one filter: --sku, --service, --region, --service-family, or --filter. SAVINGS PLAN: 'SavingsPlan' is NOT a valid --price-type. Use --include-savings-plan flag instead. Valid --price-type values: Consumption, Reservation, DevTestConsumption. When --include-savings-plan is true, Consumption items include nested 'savingsPlan' array with 1-year/3-year pricing (mainly Linux VMs). FOR BICEP/ARM COST ESTIMATION: When user asks to estimate costs from a Bicep or ARM template file, read the file, extract each resource's type and SKU, call this tool for each resource and aggregate the monthly costs (hourly price * 730 hours/month).",
                Title = "Get",
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
                        Name = "currency",
                        Description = "Currency code for pricing (e.g., USD, EUR). Default is USD.",
                        TypeName = "string",
                    },
                    new OptionDescriptor
                    {
                        Name = "sku",
                        Description = "ARM SKU name (e.g., Standard_D4s_v5, Standard_E64-16ds_v4)",
                        TypeName = "string",
                    },
                    new OptionDescriptor
                    {
                        Name = "service",
                        Description = "Azure service name (e.g., Virtual Machines, Storage, SQL Database)",
                        TypeName = "string",
                    },
                    new OptionDescriptor
                    {
                        Name = "region",
                        Description = "Azure region (e.g., eastus, westeurope, westus2)",
                        TypeName = "string",
                    },
                    new OptionDescriptor
                    {
                        Name = "service-family",
                        Description = "Service family (e.g., Compute, Storage, Databases, Networking)",
                        TypeName = "string",
                    },
                    new OptionDescriptor
                    {
                        Name = "price-type",
                        Description = "Price type filter (Consumption, Reservation, DevTestConsumption)",
                        TypeName = "string",
                    },
                    new OptionDescriptor
                    {
                        Name = "include-savings-plan",
                        Description = "Include savings plan pricing information (uses preview API version)",
                        TypeName = "string",
                    },
                    new OptionDescriptor
                    {
                        Name = "filter",
                        Description = "Raw OData filter expression for advanced queries (e.g., \"meterId eq 'abc-123'\")",
                        TypeName = "string",
                    },
                ],
                Kind = CommandKind.Global,
                HandlerType = nameof(PricingGetCommand)
            },
        ],
    };

    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IPricingService, PricingService>();
        services.AddSingleton<PricingGetCommand>();
    }

    public static IBaseCommand ResolveHandler(string handlerTypeName, IServiceProvider serviceProvider) =>
        handlerTypeName switch
        {
            nameof(PricingGetCommand) => serviceProvider.GetRequiredService<PricingGetCommand>(),
            _ => throw new InvalidOperationException($"Unknown handler type '{{handlerTypeName}}' in pricing area.")
        };
}
