// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.CostManagement.Commands.Query;
using Azure.Mcp.Tools.CostManagement.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.CostManagement;

public sealed class CostManagementSetup : IAreaSetup
{
    public string Name => "costmanagement";

    public string Title => "Azure Cost Management";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ICostManagementService, CostManagementService>();
        services.AddSingleton<QueryRunCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var costmanagement = new CommandGroup(
            Name,
            "Azure Cost Management operations - query actual Azure costs and usage by subscription or resource group.",
            Title);

        var query = new CommandGroup("query", "Cost queries.");
        costmanagement.AddSubGroup(query);

        query.AddCommand<QueryRunCommand>(serviceProvider);

        return costmanagement;
    }
}
