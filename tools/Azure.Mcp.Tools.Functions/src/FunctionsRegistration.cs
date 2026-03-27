// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Azure.Mcp.Tools.Functions.Commands.Language;
using Azure.Mcp.Tools.Functions.Commands.Project;
using Azure.Mcp.Tools.Functions.Commands.Template;
using Azure.Mcp.Tools.Functions.Options;
using Azure.Mcp.Tools.Functions.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Commands.Descriptors;

namespace Azure.Mcp.Tools.Functions;

public sealed class FunctionsRegistration : IAreaRegistration
{
    public static string AreaName => "functions";

    public static string AreaTitle => "Azure Functions";

    public static CommandCategory Category => CommandCategory.AzureServices;

    public static CommandGroupDescriptor GetCommandDescriptors() => new()
    {
        Name = AreaName,
        Description = "Azure Functions code generation commands. Use these tools to generate functions code,",
        Title = AreaTitle,
        SubGroups =
        [
            new CommandGroupDescriptor
            {
                Name = "language",
                Description = "Commands for exploring Azure Functions language support and runtime versions.",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "f7c8d9e0-a1b2-4c3d-8e5f-6a7b8c9d0e1f",
                        Name = "list",
                        Description = "List supported programming languages for Azure Functions development. Use to discover available languages, compare options, or choose a language to get started. Returns language names, runtime versions, prerequisites, development tools, and init/run/build commands. Start here before using functions project get and functions template get.",
                        Title = "List Supported Languages",
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
                        HandlerType = nameof(LanguageListCommand)
                    },
                ],
            },
            new CommandGroupDescriptor
            {
                Name = "project",
                Description = "Commands for retrieving Azure Functions project initialization templates.",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "b2c3d4e5-f6a7-8901-bcde-f12345678901",
                        Name = "get",
                        Description = "Get project scaffolding information for a new Azure Functions app. Use for getting project structure, setup instructions, and file list for initializing serverless projects. Returns project structure overview and setup instructions that agents use to create files. Use after functions language list and before functions template get.",
                        Title = "Get Project Template",
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
                                Name = "language",
                                Description = "Programming language for the Azure Functions project. Valid values: python, typescript, javascript, java, csharp, powershell.",
                                TypeName = "string",
                                Required = true,
                            },
                        ],
                        Kind = CommandKind.Basic,
                        HandlerType = nameof(ProjectGetCommand)
                    },
                ],
            },
            new CommandGroupDescriptor
            {
                Name = "template",
                Description = "Commands for listing and retrieving Azure Functions function code templates.",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "c3d4e5f6-a7b8-9012-cdef-234567890123",
                        Name = "get",
                        Description = "Generate Azure Functions code from templates including triggers, bindings, AI agents, Durable Functions, and MCP servers or list available templates. Use for code generation for serverless functions with triggers and bindings. Without --template, lists available templates. With --template, generates function code with the specified trigger and optional input/output bindings. Select one trigger (required) and zero or more bindings. Use after functions language list and functions project get.",
                        Title = "Get Project Template",
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
                                Name = "language",
                                Description = "Programming language for the Azure Functions project. Valid values: python, typescript, javascript, java, csharp, powershell.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "template",
                                Description = "Name of the function template to retrieve (e.g., HttpTrigger, BlobTrigger). Omit to list all available templates for the specified language.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "runtime-version",
                                Description = "Optional runtime version for Java or TypeScript/JavaScript. When provided, template placeholders like {{javaVersion}} or {{nodeVersion}} are replaced automatically. See 'functions language list' for supported versions.",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Basic,
                        HandlerType = nameof(ProjectGetCommand)
                    },
                ],
            },
        ],
    };

    public static void RegisterServices(IServiceCollection services)
    {
        services.Configure<FunctionsOptions>(_ => { });
        services.AddSingleton<ILanguageMetadataProvider, LanguageMetadataProvider>();
        services.AddSingleton<IManifestService, ManifestService>();
        services.AddSingleton<IFunctionsService, FunctionsService>();
        services.AddSingleton<LanguageListCommand>();
        services.AddSingleton<ProjectGetCommand>();
        services.AddSingleton<TemplateGetCommand>();
    }

    public static IBaseCommand ResolveHandler(string handlerTypeName, IServiceProvider serviceProvider) =>
        handlerTypeName switch
        {
            nameof(LanguageListCommand) => serviceProvider.GetRequiredService<LanguageListCommand>(),
            nameof(ProjectGetCommand) => serviceProvider.GetRequiredService<ProjectGetCommand>(),
            _ => throw new InvalidOperationException($"Unknown handler type '{{handlerTypeName}}' in functions area.")
        };
}
