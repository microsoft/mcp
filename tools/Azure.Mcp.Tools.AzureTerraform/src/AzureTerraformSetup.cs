// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AzureTerraform.Commands;
using Azure.Mcp.Tools.AzureTerraform.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.AzureTerraform;

public class AzureTerraformSetup : IAreaSetup
{
    public string Name => "azureterraform";

    public string Title => "Azure Terraform";

    public CommandCategory Category => CommandCategory.AzureServices;

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddHttpClient();
        services.AddSingleton<IAzureRMDocsService, AzureRMDocsService>();
        services.AddSingleton<IAzApiDocsService, AzApiDocsService>();
        services.AddSingleton<IAzApiExamplesService, AzApiExamplesService>();
        services.AddSingleton<IAvmDocsService, AvmDocsService>();
        services.AddSingleton<AzureRMDocsGetCommand>();
        services.AddSingleton<AzApiDocsGetCommand>();
        services.AddSingleton<AvmModuleListCommand>();
        services.AddSingleton<AvmVersionListCommand>();
        services.AddSingleton<AvmDocumentationGetCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var group = new CommandGroup(
            Name,
            "Azure Terraform tools - Retrieves AzureRM, AzAPI, and AVM (Azure Verified Modules) Terraform provider documentation including resource schemas, arguments, attributes, module versions, and usage examples.",
            Title
        );

        var azurermGroup = new CommandGroup(
            "azurerm",
            "AzureRM provider documentation tools",
            "AzureRM"
        );

        var docsCommand = serviceProvider.GetRequiredService<AzureRMDocsGetCommand>();
        azurermGroup.AddCommand(docsCommand.Name, docsCommand);

        group.AddSubGroup(azurermGroup);

        var azapiGroup = new CommandGroup(
            "azapi",
            "AzAPI provider documentation tools - Retrieves AzAPI resource schemas and Terraform examples using Azure Bicep type definitions",
            "AzAPI"
        );

        var azapiDocsCommand = serviceProvider.GetRequiredService<AzApiDocsGetCommand>();
        azapiGroup.AddCommand(azapiDocsCommand.Name, azapiDocsCommand);

        group.AddSubGroup(azapiGroup);

        var avmGroup = new CommandGroup(
            "avm",
            "Azure Verified Modules (AVM) documentation tools - Lists modules, versions, and retrieves module documentation",
            "AVM"
        );

        var avmListCommand = serviceProvider.GetRequiredService<AvmModuleListCommand>();
        avmGroup.AddCommand(avmListCommand.Name, avmListCommand);

        var avmVersionsCommand = serviceProvider.GetRequiredService<AvmVersionListCommand>();
        avmGroup.AddCommand(avmVersionsCommand.Name, avmVersionsCommand);

        var avmDocsCommand = serviceProvider.GetRequiredService<AvmDocumentationGetCommand>();
        avmGroup.AddCommand(avmDocsCommand.Name, avmDocsCommand);

        group.AddSubGroup(avmGroup);

        return group;
    }
}
