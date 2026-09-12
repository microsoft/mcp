// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.IoTOperations.Commands.Instance;
using Azure.Mcp.Tools.IoTOperations.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.IoTOperations;

public class IoTOperationsSetup : IAreaSetup
{
    public string Name => "iotoperations";

    public string Title => "Manage Azure IoT Operations";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IIoTOperationsService, IoTOperationsService>();
        services.AddSingleton<InstanceListCommand>();
        services.AddSingleton<InstanceGetCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var iotOperations = new CommandGroup(Name,
            """
            Azure IoT Operations operations - Commands to manage Azure IoT Operations resources.
            Azure IoT Operations is a suite of data services that run on Azure Arc-enabled edge Kubernetes clusters.
            An instance is the top-level resource that acts as a logical container for a deployment of Azure IoT
            Operations. Supports listing and getting instances in your Azure subscription.
            """,
            Title);

        var instanceGroup = new CommandGroup("instance",
            "IoT Operations instance operations - Commands for listing and getting Azure IoT Operations instances.");
        iotOperations.AddSubGroup(instanceGroup);

        instanceGroup.AddCommand<InstanceListCommand>(serviceProvider);
        instanceGroup.AddCommand<InstanceGetCommand>(serviceProvider);

        return iotOperations;
    }
}
