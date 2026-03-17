// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Pricing.Commands;
using Azure.Mcp.Tools.Pricing.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.Pricing;

/// <summary>
/// Setup class for the Pricing toolset.
/// </summary>
public class PricingSetup : IAreaSetup
{
    public string Name => "pricing";

    public string Title => "Azure Retail Pricing";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IPricingService, PricingService>();
        services.AddSingleton<PricingGetCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var pricing = new CommandGroup(
            Name,
            "Azure Retail Pricing operations - Get Azure retail pricing for SKUs, services, regions and compare pricing across different options. " +
            "For Bicep/ARM template cost estimation, read the template file, extract resource SKUs, then call 'pricing get' for each resource to build a cost estimate.",
            Title);

        var getCommand = serviceProvider.GetRequiredService<PricingGetCommand>();
        pricing.AddCommand(getCommand.Name, getCommand);

        return pricing;
    }
}
