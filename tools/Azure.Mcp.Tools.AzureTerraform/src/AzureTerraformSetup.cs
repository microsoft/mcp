// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AzureTerraform.Commands;
using Azure.Mcp.Tools.AzureTerraform.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.AzureTerraform;

public sealed class AzureTerraformSetup : IAreaSetup
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
        services.AddSingleton<IAztfexportService, AztfexportService>();
        services.AddSingleton<IConftestService, ConftestService>();
        services.AddSingleton<AzureRMDocsGetCommand>();
        services.AddSingleton<AzApiDocsGetCommand>();
        services.AddSingleton<AvmModuleListCommand>();
        services.AddSingleton<AvmVersionListCommand>();
        services.AddSingleton<AvmDocumentationGetCommand>();
        services.AddSingleton<AztfexportResourceCommand>();
        services.AddSingleton<AztfexportResourceGroupCommand>();
        services.AddSingleton<AztfexportQueryCommand>();
        services.AddSingleton<ConftestWorkspaceValidationCommand>();
        services.AddSingleton<ConftestPlanValidationCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var group = new CommandGroup(
            Name,
            "Azure Terraform tools - Retrieves AzureRM, AzAPI, and AVM (Azure Verified Modules) Terraform provider documentation including resource schemas, arguments, attributes, module versions, usage examples, aztfexport command generation, and conftest policy validation.",
            Title
        );

        var azurermGroup = new CommandGroup(
            "azurerm",
            "AzureRM provider documentation tools",
            "AzureRM"
        );

        azurermGroup.AddCommand(serviceProvider.GetRequiredService<AzureRMDocsGetCommand>());

        group.AddSubGroup(azurermGroup);

        var azapiGroup = new CommandGroup(
            "azapi",
            "AzAPI provider documentation tools - Retrieves AzAPI resource schemas and Terraform examples using Azure Bicep type definitions",
            "AzAPI"
        );

        azapiGroup.AddCommand(serviceProvider.GetRequiredService<AzApiDocsGetCommand>());

        group.AddSubGroup(azapiGroup);

        var avmGroup = new CommandGroup(
            "avm",
            "Azure Verified Modules (AVM) documentation tools - Lists modules, versions, and retrieves module documentation",
            "AVM"
        );

        avmGroup.AddCommand(serviceProvider.GetRequiredService<AvmModuleListCommand>());
        avmGroup.AddCommand(serviceProvider.GetRequiredService<AvmVersionListCommand>());
        avmGroup.AddCommand(serviceProvider.GetRequiredService<AvmDocumentationGetCommand>());

        group.AddSubGroup(avmGroup);

        var aztfexportGroup = new CommandGroup(
            "aztfexport",
            "Azure Export for Terraform (aztfexport) tools - Generates aztfexport commands to export Azure resources, resource groups, or query-matched resources to Terraform configuration",
            "aztfexport"
        );

        aztfexportGroup.AddCommand(serviceProvider.GetRequiredService<AztfexportResourceCommand>());
        aztfexportGroup.AddCommand(serviceProvider.GetRequiredService<AztfexportResourceGroupCommand>());
        aztfexportGroup.AddCommand(serviceProvider.GetRequiredService<AztfexportQueryCommand>());

        group.AddSubGroup(aztfexportGroup);

        var conftestGroup = new CommandGroup(
            "conftest",
            "Conftest validation tools - Generates conftest commands to validate Terraform configurations and plans against Azure policies using the Open Policy Agent (OPA) framework",
            "Conftest"
        );

        conftestGroup.AddCommand(serviceProvider.GetRequiredService<ConftestWorkspaceValidationCommand>());
        conftestGroup.AddCommand(serviceProvider.GetRequiredService<ConftestPlanValidationCommand>());

        group.AddSubGroup(conftestGroup);

        return group;
    }
}
