// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Insights.Commands;
using Azure.Mcp.Tools.Insights.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.Insights;

public class InsightsSetup : IAreaSetup
{
    public string Name => "insights";

    public string Title => "Derive Azure infrastructure insights";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IInsightsService, InsightsService>();
        services.AddSingleton<ISamplingService, SamplingService>();

        services.AddSingleton<InsightsGetCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var insights = new CommandGroup(Name,
            """
            Insights operations - Commands for deriving insights from existing Azure infrastructure.
            Aggregates Azure Resource Graph data and uses MCP sampling to surface dominant patterns
            (region, sku, security posture, tagging conventions, resource pairing).
            """,
            Title);

        var getCommand = serviceProvider.GetRequiredService<InsightsGetCommand>();
        insights.AddCommand(getCommand.Name, getCommand);

        return insights;
    }
}
