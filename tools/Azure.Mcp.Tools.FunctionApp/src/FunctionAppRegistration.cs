// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Azure.Mcp.Tools.FunctionApp.Commands.FunctionApp;
using Azure.Mcp.Tools.FunctionApp.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Commands.Descriptors;

namespace Azure.Mcp.Tools.FunctionApp;

public sealed class FunctionAppRegistration : IAreaRegistration
{
    public static string AreaName => "functionapp";

    public static string AreaTitle => "Azure Functions";

    public static CommandCategory Category => CommandCategory.AzureServices;

    public static CommandGroupDescriptor GetCommandDescriptors() => new()
    {
        Name = AreaName,
        Description = "Function App operations - Commands for managing and accessing Azure Function App resources.",
        Title = AreaTitle,
        Commands =
        [
            new CommandDescriptor
            {
                Id = "5249839c-a3c6-4f9e-b62b-afde801d95a6",
                Name = "get",
                Description = "Gets Azure Function App details. Lists all Function Apps in the subscription or resource group. If function app name and resource group is specified, retrieves the details of that specific function app. Returns the details of Azure Function Apps, including its name, location, status, and app service plan name.",
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
                        Name = "resource-group",
                        Description = "The name of the Azure resource group. This is a logical container for Azure resources.",
                        TypeName = "string",
                    },
                    new OptionDescriptor
                    {
                        Name = "function-app",
                        Description = "The name of the Function App.",
                        TypeName = "string",
                    },
                ],
                Kind = CommandKind.Subscription,
                HandlerType = nameof(FunctionAppGetCommand)
            },
        ],
    };

    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IFunctionAppService, FunctionAppService>();
        services.AddSingleton<FunctionAppGetCommand>();
    }

    public static IBaseCommand ResolveHandler(string handlerTypeName, IServiceProvider serviceProvider) =>
        handlerTypeName switch
        {
            nameof(FunctionAppGetCommand) => serviceProvider.GetRequiredService<FunctionAppGetCommand>(),
            _ => throw new InvalidOperationException($"Unknown handler type '{{handlerTypeName}}' in functionapp area.")
        };
}
