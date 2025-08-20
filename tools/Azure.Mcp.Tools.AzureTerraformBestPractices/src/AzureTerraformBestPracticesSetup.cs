// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AzureTerraformBestPractices.Commands;
using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.AzureTerraformBestPractices;

public class AzureTerraformBestPracticesSetup : IAreaSetup
{
    public void ConfigureServices(IServiceCollection services)
    {
    }

    public void RegisterCommands(CommandGroup rootGroup, ILoggerFactory loggerFactory)
    {
        // Register Azure Terraform Best Practices command at the root level
        var azureTerraformBestPractices = new CommandGroup(
            "azureterraformbestpractices",
            "Returns Terraform best practices for Azure. Call this before generating Terraform code for Azure Providers."
        );
        rootGroup.AddSubGroup(azureTerraformBestPractices);
        azureTerraformBestPractices.AddCommand(
            "get",
            new AzureTerraformBestPracticesGetCommand(loggerFactory.CreateLogger<AzureTerraformBestPracticesGetCommand>())
        );
    }
}
