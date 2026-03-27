// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Azure.Mcp.Tools.Grafana.Commands.Workspace;
using Azure.Mcp.Tools.Grafana.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Commands.Descriptors;

namespace Azure.Mcp.Tools.Grafana;

public sealed class GrafanaRegistration : IAreaRegistration
{
    public static string AreaName => "grafana";

    public static string AreaTitle => "Azure Managed Grafana";

    public static CommandCategory Category => CommandCategory.AzureServices;

    public static CommandGroupDescriptor GetCommandDescriptors() => new()
    {
        Name = AreaName,
        Description = "Grafana workspace operations - Commands for managing and accessing Azure Managed Grafana resources and monitoring dashboards. Includes operations for listing Grafana workspaces and managing data visualization and monitoring capabilities.",
        Title = AreaTitle,
        Commands =
        [
            new CommandDescriptor
            {
                Id = "7a47b562-f219-47de-80f6-12e19367b61d",
                Name = "list",
                Description = "List all Grafana workspace resources in a specified subscription. Returns an array of Grafana workspace details. Use this command to explore which Grafana workspace resources are available in your subscription.",
                Title = "List",
                Annotations = new ToolAnnotations
                {
                    Destructive = false,
                    Idempotent = true,
                    OpenWorld = false,
                    ReadOnly = true,
                    LocalRequired = false,
                    Secret = false,
                },
                Options = [],
                Kind = CommandKind.Subscription,
                HandlerType = nameof(WorkspaceListCommand)
            },
        ],
    };

    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IGrafanaService, GrafanaService>();
        services.AddSingleton<WorkspaceListCommand>();
    }

    public static IBaseCommand ResolveHandler(string handlerTypeName, IServiceProvider serviceProvider) =>
        handlerTypeName switch
        {
            nameof(WorkspaceListCommand) => serviceProvider.GetRequiredService<WorkspaceListCommand>(),
            _ => throw new InvalidOperationException($"Unknown handler type '{{handlerTypeName}}' in grafana area.")
        };
}
