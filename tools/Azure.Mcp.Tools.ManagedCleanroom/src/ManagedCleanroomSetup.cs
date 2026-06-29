// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ManagedCleanroom.Commands.CollaborationArm;
using Azure.Mcp.Tools.ManagedCleanroom.Commands.Collaborations;
using Azure.Mcp.Tools.ManagedCleanroom.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.ManagedCleanroom;

public class ManagedCleanroomSetup : IAreaSetup
{
    internal const string DefaultHttpClientName = "ManagedCleanroom.Default";
    internal const string UnsafeHttpClientName = "ManagedCleanroom.Unsafe";

    public string Name => "managedcleanroom";

    public string Title => "Azure Managed Cleanroom";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddHttpClient(DefaultHttpClientName);
        services.AddHttpClient(UnsafeHttpClientName)
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            });
        services.AddSingleton<IManagedCleanroomServiceDataPlane, ManagedCleanroomDataPlaneService>();
        services.AddSingleton<IManagedCleanroomServiceControlPlane, ManagedCleanroomControlPlaneService>();
        services.AddSingleton<CollaborationsListCommand>();
        services.AddSingleton<CollaborationCreateCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var root = new CommandGroup(Name,
            "Azure Managed Cleanroom operations - Commands for interacting with the Azure Cleanroom Analytics Frontend, including listing and inspecting collaborations and analytics workloads.", Title);

        var collaborations = new CommandGroup("collaborations", "Cleanroom collaboration operations - Commands for listing and inspecting cleanroom collaborations.");
        root.AddSubGroup(collaborations);

        collaborations.AddCommand<CollaborationsListCommand>(serviceProvider);

        var collaborationArm = new CommandGroup("collaborationarm", "Cleanroom ARM management operations - Commands for creating and managing Azure Cleanroom collaboration ARM resources.");
        root.AddSubGroup(collaborationArm);

        collaborationArm.AddCommand<CollaborationCreateCommand>(serviceProvider);

        return root;
    }
}
