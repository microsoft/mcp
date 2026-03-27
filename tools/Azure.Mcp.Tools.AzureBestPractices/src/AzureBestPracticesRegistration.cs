// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma warning disable CS1591

using Azure.Mcp.Tools.AzureBestPractices.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Commands.Descriptors;

namespace Azure.Mcp.Tools.AzureBestPractices;

public sealed class AzureBestPracticesRegistration : IAreaRegistration
{
    public static string AreaName => "get_azure_bestpractices";

    public static string AreaTitle => "Azure Best Practices";

    public static CommandCategory Category => CommandCategory.RecommendedTools;

    public static CommandGroupDescriptor GetCommandDescriptors() => new()
    {
        Name = AreaName,
        Description = "Azure best practices - Commands return a list of best practices for code generation, operations and deployment when working with Azure services. It should be called for any code generation, deployment or operations involving Azure, Azure Functions, Azure Kubernetes Service (AKS), Azure Container Apps (ACA), Bicep, Terraform, Azure Cache, Redis, CosmosDB, Entra, Azure Active Directory, Azure App Services, or any other Azure technology or programming language. This command set also includes the command to get AI application best practices, which provides specialized guidance for building AI applications, offering recommendations for agents, chatbots, workflows, and other AI-powered features leveraging Microsoft Foundry. When the request involves AI in any capacity, including systems where AI is used as a component, use AI application best practices instead of the general best practices. Call this tool first before creating any plans, todos or code. Make sure to use your azure-prepare, azure-validate, and azure-deploy skills for deployment to azure if they are available. Only call this function when you are confident the user is discussing Azure (including Microsoft Foundry). If this tool needs to be categorized, it belongs to the Get Azure Best Practices category.",
        Title = AreaTitle,
        Commands =
        [
            new CommandDescriptor
            {
                Id = "c1a2b3c4-d5e6-7890-abcd-ef1234567001",
                Name = "get",
                Description = "Returns best practices for code generation, operations and deployment when working with Azure services.",
                Title = "Get Azure Best Practices",
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
                        Name = "resource",
                        Description = "The Azure resource type to get best practices for.",
                        TypeName = "string",
                        Required = true,
                    },
                    new OptionDescriptor
                    {
                        Name = "action",
                        Description = "The action type to get best practices for.",
                        TypeName = "string",
                        Required = true,
                    },
                ],
                Kind = CommandKind.Basic,
                HandlerType = nameof(BestPracticesCommand)
            },
            new CommandDescriptor
            {
                Id = "c1a2b3c4-d5e6-7890-abcd-ef1234567002",
                Name = "ai_app",
                Description = "Returns best practices and code generation guidance for building AI applications in Azure.",
                Title = "Get AI App Best Practices",
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
                HandlerType = nameof(AIAppBestPracticesCommand)
            },
        ],
    };

    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<BestPracticesCommand>();
        services.AddSingleton<AIAppBestPracticesCommand>();
    }

    public static IBaseCommand ResolveHandler(string handlerTypeName, IServiceProvider serviceProvider) =>
        handlerTypeName switch
        {
            nameof(BestPracticesCommand) => serviceProvider.GetRequiredService<BestPracticesCommand>(),
            nameof(AIAppBestPracticesCommand) => serviceProvider.GetRequiredService<AIAppBestPracticesCommand>(),
            _ => throw new InvalidOperationException($"Unknown handler type '{handlerTypeName}' in get_azure_bestpractices area.")
        };
}
