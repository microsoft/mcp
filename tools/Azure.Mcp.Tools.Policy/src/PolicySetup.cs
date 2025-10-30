// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Policy.Commands.Assignment;
using Azure.Mcp.Tools.Policy.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Policy;

public sealed class PolicySetup : IAreaSetup
{
    public string Name => "policy";

    public string Title => "Azure Policy";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IPolicyService, PolicyService>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

        var policy = new CommandGroup(
            Name,
            "Manage Azure Policy assignments and definitions using Azure CLI. Retrieve policy assignments, view enforcement modes, and analyze policy compliance across subscriptions.",
            Title);

        var assignment = new CommandGroup(
            "assignment",
            "Commands for managing Azure Policy assignments. List all assignments or get details of specific policy assignments.");
        policy.AddSubGroup(assignment);

        var assignmentGet = new AssignmentGetCommand(loggerFactory.CreateLogger<AssignmentGetCommand>());
        assignment.AddCommand(assignmentGet.Name, assignmentGet);

        return policy;
    }
}
