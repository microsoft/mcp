// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Communication.Commands.Email;
using Azure.Mcp.Tools.Communication.Commands.Sms;
using Azure.Mcp.Tools.Communication.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Communication;

public class CommunicationSetup : IAreaSetup
{
    public string Name => "communication";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ICommunicationService, CommunicationService>();
        services.AddSingleton<SmsSendCommand>();
        services.AddSingleton<EmailSendCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        // Create Communication command group
        var communication = new CommandGroup(Name,
            "Communication services operations - Commands for managing Azure Communication Services - supports sending SMS");
        // Create SMS subgroup
        var sms = new CommandGroup("sms", "SMS messaging operations - sending SMS messages to one or more recipients using Azure Communication Services.");
        communication.AddSubGroup(sms);
        // Register SMS commands        
        var smsSend = serviceProvider.GetRequiredService<SmsSendCommand>();
        sms.AddCommand(smsSend.Name, smsSend);

        var email = new CommandGroup("email", "Email messaging operations - sending email messages to one or more recipients using Azure Communication Services.");
        communication.AddSubGroup(email);
        var emailSend = serviceProvider.GetRequiredService<EmailSendCommand>();
        email.AddCommand(emailSend.Name, emailSend);
        return communication;
    }
}
