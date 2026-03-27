// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Azure.Mcp.Tools.BicepSchema.Commands;
using Azure.Mcp.Tools.BicepSchema.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Commands.Descriptors;

namespace Azure.Mcp.Tools.BicepSchema;

public sealed class BicepSchemaRegistration : IAreaRegistration
{
    public static string AreaName => "bicepschema";

    public static string AreaTitle => "Azure Bicep Schema";

    public static CommandCategory Category => CommandCategory.AzureServices;

    public static CommandGroupDescriptor GetCommandDescriptors() => new()
    {
        Name = AreaName,
        Description = "Bicep schema operations - Commands for working with Azure Bicep Infrastructure as Code (IaC) generation and schema management. Includes operations for retrieving Bicep schemas, templates, and resource definitions to support infrastructure deployment automation.",
        Title = AreaTitle,
        Commands =
        [
            new CommandDescriptor
            {
                Id = "553c003a-7cdf-4382-b833-94fe8bbb7386",
                Name = "get",
                Description = "Provides the Bicep schema definition of any Azure resource type (latest service version). Use this to get the schema needed to write Bicep IaC (infrasturcture as code) for Azure resources such as AI models, storage accounts, databases, virtual machines, app services, key vaults, and more. Do not use this tool for resource deployment, deployment guidelines, or getting best practices.",
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
                Options =
                [
                    new OptionDescriptor
                    {
                        Name = "resource-type",
                        Description = "The name of the Bicep Resource Type and must be in the full Azure Resource Manager format '{ResourceProvider}/{ResourceType}'. (e.g., 'Microsoft.KeyVault/vaults', 'Microsoft.Storage/storageAccounts', 'Microsoft.Compute/virtualMachines')(e.g., Microsoft.Storage/storageAccounts).",
                        TypeName = "string",
                        Required = true,
                    },
                ],
                Kind = CommandKind.Global,
                HandlerType = nameof(BicepSchemaGetCommand)
            },
        ],
    };

    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IBicepSchemaService, BicepSchemaService>();
        services.AddSingleton<BicepSchemaGetCommand>();
    }

    public static IBaseCommand ResolveHandler(string handlerTypeName, IServiceProvider serviceProvider) =>
        handlerTypeName switch
        {
            nameof(BicepSchemaGetCommand) => serviceProvider.GetRequiredService<BicepSchemaGetCommand>(),
            _ => throw new InvalidOperationException($"Unknown handler type '{{handlerTypeName}}' in bicepschema area.")
        };
}
