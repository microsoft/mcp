// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Azure.Mcp.Tools.AzureTerraformBestPractices.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Commands.Descriptors;

namespace Azure.Mcp.Tools.AzureTerraformBestPractices;

public sealed class AzureTerraformBestPracticesRegistration : IAreaRegistration
{
    public static string AreaName => "azureterraformbestpractices";

    public static string AreaTitle => "Azure Terraform Best Practices";

    public static CommandCategory Category => CommandCategory.RecommendedTools;

    public static CommandGroupDescriptor GetCommandDescriptors() => new()
    {
        Name = AreaName,
        Description = "Returns Terraform best practices for Azure. Call this before generating Terraform code for Azure Providers. If this tool needs to be categorized, it belongs to the Azure Best Practices category.",
        Title = AreaTitle,
        Commands =
        [
            new CommandDescriptor
            {
                Id = "5bd36575-6313-4bf4-aa26-a79fe0fa32a8",
                Name = "get",
                Description = "Returns Terraform best practices for Azure. Call this command and follow its guidance before generating or suggesting any Terraform code specific to Azure. If this tool needs to be categorized, it belongs to the Azure Best Practices category.",
                Title = "Get",
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
                Kind = CommandKind.Basic,
                HandlerType = nameof(AzureTerraformBestPracticesGetCommand)
            },
        ],
    };

    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<AzureTerraformBestPracticesGetCommand>();
    }

    public static IBaseCommand ResolveHandler(string handlerTypeName, IServiceProvider serviceProvider) =>
        handlerTypeName switch
        {
            nameof(AzureTerraformBestPracticesGetCommand) => serviceProvider.GetRequiredService<AzureTerraformBestPracticesGetCommand>(),
            _ => throw new InvalidOperationException($"Unknown handler type '{{handlerTypeName}}' in azureterraformbestpractices area.")
        };
}
