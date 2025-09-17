using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Playwright.Commands.PlaywrightWorkspaces;
using Azure.Mcp.Tools.Playwright.Commands.PlaywrightQuotas;
using Azure.Mcp.Tools.Playwright.Commands.PlaywrightWorkspaceQuotas;
using Azure.Mcp.Tools.Playwright.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Playwright;

public class PlaywrightSetup : IAreaSetup
{
    public string Name => "playwright";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IPlaywrightService, PlaywrightService>();
    }

    public void RegisterCommands(CommandGroup rootGroup, ILoggerFactory loggerFactory)
    {
        var service = new CommandGroup(
            Name,
            "Playwright Service Management API - Commands for managing Playwright workspace resources and quotas.");
        rootGroup.AddSubGroup(service);

        var operations = new CommandGroup("operations", "Provider operations - List the provider operations for Playwright");
        service.AddSubGroup(operations);

        var workspaces = new CommandGroup("workspaces", "Playwright workspace operations - Manage Playwright workspace resources.");
        service.AddSubGroup(workspaces);
        workspaces.AddCommand("list", new PlaywrightWorkspaceListCommand(loggerFactory.CreateLogger<PlaywrightWorkspaceListCommand>()));
        workspaces.AddCommand("get", new PlaywrightWorkspaceGetCommand(loggerFactory.CreateLogger<PlaywrightWorkspaceGetCommand>()));
        workspaces.AddCommand("create", new PlaywrightWorkspaceCreateCommand(loggerFactory.CreateLogger<PlaywrightWorkspaceCreateCommand>()));
        workspaces.AddCommand("update", new PlaywrightWorkspaceUpdateCommand(loggerFactory.CreateLogger<PlaywrightWorkspaceUpdateCommand>()));
        workspaces.AddCommand("delete", new PlaywrightWorkspaceDeleteCommand(loggerFactory.CreateLogger<PlaywrightWorkspaceDeleteCommand>()));

        var quotas = new CommandGroup("quotas", "Subscription-level Playwright quotas - Lists and gets subscription-level quotas by location.");
        service.AddSubGroup(quotas);
        quotas.AddCommand("list", new PlaywrightQuotaListBySubscriptionCommand(loggerFactory.CreateLogger<PlaywrightQuotaListBySubscriptionCommand>()));
        quotas.AddCommand("get", new PlaywrightQuotaGetCommand(loggerFactory.CreateLogger<PlaywrightQuotaGetCommand>()));

        var workspaceQuotas = new CommandGroup("workspace-quotas", "Playwright workspace quotas - Lists and gets workspace-scoped quotas.");
        service.AddSubGroup(workspaceQuotas);
        workspaceQuotas.AddCommand("list", new PlaywrightWorkspaceQuotaListCommand(loggerFactory.CreateLogger<PlaywrightWorkspaceQuotaListCommand>()));
        workspaceQuotas.AddCommand("get", new PlaywrightWorkspaceQuotaGetCommand(loggerFactory.CreateLogger<PlaywrightWorkspaceQuotaGetCommand>()));
    }
}
