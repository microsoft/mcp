// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Support.Commands.ProblemClassification;
using Azure.Mcp.Tools.Support.Commands.Service;
using Azure.Mcp.Tools.Support.Commands.Ticket;
using Azure.Mcp.Tools.Support.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.Support;

public class SupportSetup : IAreaSetup
{
    public string Name => "support";
    public string Title => "Manage Azure Support Tickets";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ISupportService, SupportService>();

        services.AddSingleton<TicketListCommand>();
        services.AddSingleton<TicketGetCommand>();
        services.AddSingleton<TicketCreateCommand>();
        services.AddSingleton<ServiceListCommand>();
        services.AddSingleton<ProblemClassificationListCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var support = new CommandGroup(Name,
            "Azure support operations - List, view, and create Azure support tickets (support requests) and look up the " +
            "Azure services and problem classifications needed to file them. Use when a user wants to check the status " +
            "of an existing support case or open a new one. Before creating a ticket, run available diagnostics (resource " +
            "health, AppLens, Azure Monitor, storage disk diagnose) because they often resolve the problem without a " +
            "ticket. Not for Azure Advisor recommendations, service health incidents, or quota increase requests.",
            Title);

        var ticket = new CommandGroup("ticket", "Azure support ticket operations - Commands for listing, viewing, and creating Azure support tickets in a subscription.");
        support.AddSubGroup(ticket);
        var service = new CommandGroup("service", "Azure support service operations - Commands for listing the Azure services that support tickets can be opened for.");
        support.AddSubGroup(service);
        var problemClassification = new CommandGroup("problemclassification", "Azure support problem classification operations - Commands for listing the problem classifications of an Azure support service.");
        support.AddSubGroup(problemClassification);

        ticket.AddCommand<TicketListCommand>(serviceProvider);
        ticket.AddCommand<TicketGetCommand>(serviceProvider);
        ticket.AddCommand<TicketCreateCommand>(serviceProvider);

        service.AddCommand<ServiceListCommand>(serviceProvider);

        problemClassification.AddCommand<ProblemClassificationListCommand>(serviceProvider);

        return support;
    }
}
