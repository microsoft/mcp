// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AzureTerraformBestPractices.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Areas;

namespace Azure.Mcp.Tools.AzureTerraformBestPractices;

public class AzureTerraformBestPracticesSetup : IAreaSetup
{
    public string Name => "azureterraformbestpractices";

    public void ConfigureServices(IServiceCollection services)
    {
    }

    public void RegisterCommands(CommandGroup rootGroup, ILoggerFactory loggerFactory)
    {
        // Register Azure Terraform Best Practices command at the root level
        var azureTerraformBestPractices = new CommandGroup(
            Name,
            @"Returns Terraform best practices for Azure. Call this before generating Terraform code for Azure Providers. 
            If this tool needs to be categorized, it belongs to the Azure Best Practices category."
        );
        rootGroup.AddSubGroup(azureTerraformBestPractices);
        azureTerraformBestPractices.AddCommand(
            "get",
            new AzureTerraformBestPracticesGetCommand(loggerFactory.CreateLogger<AzureTerraformBestPracticesGetCommand>())
        );
    }
}
