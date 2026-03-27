// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Extension.Commands;
using Azure.Mcp.Tools.Extension.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Areas.Server.Options;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Commands.Descriptors;

namespace Azure.Mcp.Tools.Extension;

public sealed class ExtensionRegistration : IAreaRegistration
{
    public static string AreaName => "extension";

    public static string AreaTitle => "Azure VM Extensions";

    public static CommandCategory Category => CommandCategory.RecommendedTools;

    public static CommandGroupDescriptor GetCommandDescriptors() => new()
    {
        Name = AreaName,
        Description = "Azure VM Extensions operations.",
        Title = AreaTitle,
        Commands =
        [
            new CommandDescriptor
            {
                Id = "e7ef18a3-2730-4300-bad3-dc766f47dd2a",
                Name = "azqr",
                Description = "Runs Azure Quick Review CLI (azqr) commands to generate compliance and security reports for Azure resources, identifying non-compliant configurations or areas for improvement. Requires a subscription id and optionally a resource group name. Returns the generated report file path. Note: azqr is different from Azure CLI (az).",
                Title = "Azqr",
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
                ],
                Kind = CommandKind.Subscription,
                HandlerType = nameof(AzqrCommand)
            },
        ],
        SubGroups =
        [
            new CommandGroupDescriptor
            {
                Name = "cli",
                Description = "Commands for helping users to use CLI tools for Azure services operations. Includes operations for generating Azure CLI commands and getting installation instructions for Azure CLI (az), Azure Developer CLI (azd), and Azure Core Function Tools CLI (func).",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "3de4ef37-90bf-41f1-8385-5e870c3ae911",
                        Name = "generate",
                        Description = "Generate Azure CLI (az) commands used to accomplish a goal described by the user. This tool incorporates CLI knowledge beyond what you know. Use this tool when the user asks for Azure CLI commands or wants to use the Azure CLI to accomplish something.",
                        Title = "Generate",
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
                                Name = "intent",
                                Description = "The user intent of the task to be solved by using the CLI tool. This user intent will be used to generate the appropriate CLI command to accomplish the desirable goal.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "cli-type",
                                Description = "The type of CLI tool to use. Supported values are 'az' for Azure CLI.",
                                TypeName = "string",
                                Required = true,
                            },
                        ],
                        Kind = CommandKind.Global,
                        HandlerType = nameof(CliGenerateCommand)
                    },
                    new CommandDescriptor
                    {
                        Id = "464626d0-b9be-4a3b-9f29-858637ab8c10",
                        Name = "install",
                        Description = "Provide installation instructions for Azure CLI (az), Azure Developer CLI (azd), and Azure Functions Core Tools CLI (func). This tool incorporates CLI knowledge beyond what you know. Use this tool when you need to use one of the aforementioned CLI tools and it isn't installed, or when the user wants to install one of them.",
                        Title = "Install",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = false,
                            Idempotent = true,
                            OpenWorld = false,
                            ReadOnly = true,
                            LocalRequired = true,
                            Secret = false,
                        },
                        Options =
                        [
                            new OptionDescriptor
                            {
                                Name = "cli-type",
                                Description = "The type of CLI tool to use. Supported values are 'az' for Azure CLI, 'azd' for Azure Developer CLI, and 'func' for Azure Functions Core Tools CLI.",
                                TypeName = "string",
                                Required = true,
                            },
                        ],
                        Kind = CommandKind.Global,
                        HandlerType = nameof(CliInstallCommand)
                    },
                ],
            },
        ],
    };

    public static void RegisterServices(IServiceCollection services)
    {
        services.AddHttpClientServices();
        services.AddSingleton<ICliGenerateService, CliGenerateService>();
        services.AddSingleton<AzqrCommand>();
        services.AddSingleton<CliGenerateCommand>();
        services.AddSingleton<ICliInstallService, CliInstallService>();
        services.AddSingleton<CliInstallCommand>();
    }

    public static IBaseCommand ResolveHandler(string handlerTypeName, IServiceProvider serviceProvider) =>
        handlerTypeName switch
        {
            nameof(AzqrCommand) => serviceProvider.GetRequiredService<AzqrCommand>(),
            nameof(CliGenerateCommand) => serviceProvider.GetRequiredService<CliGenerateCommand>(),
            nameof(CliInstallCommand) => serviceProvider.GetRequiredService<CliInstallCommand>(),
            _ => throw new InvalidOperationException($"Unknown handler type '{{handlerTypeName}}' in extension area.")
        };
}
