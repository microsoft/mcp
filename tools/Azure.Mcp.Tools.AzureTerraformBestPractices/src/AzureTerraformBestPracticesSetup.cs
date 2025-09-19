// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.AzureTerraformBestPractices.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace Azure.Mcp.Tools.AzureTerraformBestPractices;

public class AzureTerraformBestPracticesSetup : IAreaSetup
{
    public string Name => "azureterraformbestpractices";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<AzureTerraformBestPracticesGetCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        // Register Azure Terraform Best Practices command at the root level
        var azureTerraformBestPractices = new CommandGroup(
            Name,
            @"Returns Terraform best practices for Azure. Call this before generating Terraform code for Azure Providers. 
            If this tool needs to be categorized, it belongs to the Azure Best Practices category."
        );

        var practices = serviceProvider.GetRequiredService<AzureTerraformBestPracticesGetCommand>();
        azureTerraformBestPractices.AddCommand(practices.Name, practices);

        return azureTerraformBestPractices;
    }
}
