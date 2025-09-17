// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Quota.Commands.Region;
using Azure.Mcp.Tools.Quota.Commands.Usage;
using Azure.Mcp.Tools.Quota.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Azure.Mcp.Tools.Quota;

public sealed class QuotaSetup : IAreaSetup
{
    public string Name => "quota";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddHttpClientServices();

        services.AddTransient<IQuotaService, QuotaService>();

        services.AddTransient<CheckCommand>();
        services.AddTransient<AvailabilityListCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var quota = new CommandGroup(Name, "Quota commands for getting the available regions of specific Azure resource types"
                    + " or checking Azure resource quota and usage");

        var usageGroup = new CommandGroup("usage", "Resource usage and quota operations");
        usageGroup.AddCommand("check", serviceProvider.GetRequiredService<CheckCommand>());
        quota.AddSubGroup(usageGroup);

        var regionGroup = new CommandGroup("region", "Region availability operations");
        var availabilityGroup = new CommandGroup("availability", "Region availability information");

        availabilityGroup.AddCommand("list", serviceProvider.GetRequiredService<AvailabilityListCommand>());
        regionGroup.AddSubGroup(availabilityGroup);
        quota.AddSubGroup(regionGroup);

        return quota;
    }
}
