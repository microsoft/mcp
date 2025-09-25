// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.SignalR.Commands.Runtime;
using Azure.Mcp.Tools.SignalR.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Azure.Mcp.Tools.SignalR;

public class SignalRSetup : IAreaSetup
{
    public string Name => "signalr";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ISignalRService, SignalRService>();

        services.AddSingleton<RuntimeGetCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var signalr = new CommandGroup(Name,
            "Azure SignalR operations - Commands for managing Azure SignalR Service resources. Includes operations for listing SignalR services.");

        var runtime = new CommandGroup("runtime",
            "Runtime operations - Commands for managing Azure SignalR Service resources.");
        signalr.AddSubGroup(runtime);

        var runtimeGet = serviceProvider.GetRequiredService<RuntimeGetCommand>();
        runtime.AddCommand(runtimeGet.Name, runtimeGet);

        return signalr;
    }
}
