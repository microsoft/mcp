// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Communication.Commands.Sms;
using Azure.Mcp.Tools.Communication.Services;
using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Communication;

public class CommunicationSetup : IAreaSetup
{
    public string Name => "communication";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ICommunicationService, CommunicationService>();
    }

    public void RegisterCommands(CommandGroup rootGroup, ILoggerFactory loggerFactory)
    {
        // Create Communication command group
        var communication = new CommandGroup("communication", 
            "Communication services operations - Commands for managing Azure Communication Services including SMS messaging, email, and voice calling capabilities.");
        rootGroup.AddSubGroup(communication);

        // Create SMS subgroup
        var sms = new CommandGroup("sms", "SMS messaging operations - Commands for sending SMS messages using Azure Communication Services.");
        communication.AddSubGroup(sms);

        // Register SMS commands
        sms.AddCommand("send", new SmsSendCommand(loggerFactory.CreateLogger<SmsSendCommand>()));
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        // Not used in this implementation
        throw new NotImplementedException();
    }
}