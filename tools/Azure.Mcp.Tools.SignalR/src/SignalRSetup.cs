// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.SignalR.Commands.Identity;
using Azure.Mcp.Tools.SignalR.Commands.Key;
using Azure.Mcp.Tools.SignalR.Commands.NetworkRule;
using Azure.Mcp.Tools.SignalR.Commands.Runtime;
using Azure.Mcp.Tools.SignalR.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.SignalR;

public class SignalRSetup : IAreaSetup
{
    public string Name => "signalr";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ISignalRService, SignalRService>();
    }

    public void RegisterCommands(CommandGroup rootGroup, ILoggerFactory loggerFactory)
    {
        var signalr = new CommandGroup("signalr",
            "Azure SignalR operations - Commands for managing Azure SignalR Service resources. Includes operations for listing SignalR services, managing hubs, configuring access keys, and scaling SignalR instances.");
        rootGroup.AddSubGroup(signalr);

        var service = new CommandGroup("runtime",
            "SignalR service operations - Commands for managing Azure SignalR Service resources.");
        signalr.AddSubGroup(service);

        service.AddCommand("list",
            new RuntimeListCommand(loggerFactory.CreateLogger<RuntimeListCommand>()));
        service.AddCommand("show", new RuntimeShowCommand(loggerFactory.CreateLogger<RuntimeShowCommand>()));

        var key = new CommandGroup("key",
            "SignalR key operations - Commands for managing access keys in Azure SignalR Service resources.");
        signalr.AddSubGroup(key);

        key.AddCommand("list", new KeyListCommand(loggerFactory.CreateLogger<KeyListCommand>()));

        var identity = new CommandGroup("identity",
            "SignalR identity operations - Commands for managing managed identity configuration in Azure SignalR Service resources.");
        signalr.AddSubGroup(identity);

        identity.AddCommand("show", new IdentityShowCommand(loggerFactory.CreateLogger<IdentityShowCommand>()));

        // Network rule commands
        var networkRule = new CommandGroup(
            "network-rule",
            "SignalR network rule operations");
        signalr.AddSubGroup(networkRule);

        networkRule.AddCommand("list", new NetworkRuleListCommand(
            loggerFactory.CreateLogger<NetworkRuleListCommand>()));
    }
}
