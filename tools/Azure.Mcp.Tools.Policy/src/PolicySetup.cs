// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Policy.Commands.Assignment;
using Azure.Mcp.Tools.Policy.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.Policy;

public sealed class PolicySetup : IAreaSetup
{
    public string Name => "policy";

    public string Title => "Azure Policy Management";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IPolicyService, PolicyService>();

        services.AddSingleton<PolicyAssignmentListCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        // Create Policy command group
        var policy = new CommandGroup(Name,
            "Manage Azure Policy assignments and definitions using Azure CLI. Retrieve policy assignments, view enforcement modes, and analyze policy compliance across subscriptions.",
            Title);

        // Create Assignment subgroup
        var assignment = new CommandGroup("assignment",
            "Policy assignment operations - Commands for getting and managing Azure Policy assignments.");
        policy.AddSubGroup(assignment);

        // Register assignment commands
        var listCommand = serviceProvider.GetRequiredService<PolicyAssignmentListCommand>();
        assignment.AddCommand(listCommand.Name, listCommand);

        return policy;
    }
}
