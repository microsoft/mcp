// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Functions.Commands.Language;
using Azure.Mcp.Tools.Functions.Commands.Project;
using Azure.Mcp.Tools.Functions.Commands.Template;
using Azure.Mcp.Tools.Functions.Options;
using Azure.Mcp.Tools.Functions.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.Functions;

public sealed class FunctionsSetup : IAreaSetup
{
    public string Name => "functions";

    public string Title => "Azure Functions";

    public void ConfigureServices(IServiceCollection services)
    {
        services.Configure<FunctionsOptions>(_ => { });
        services.AddSingleton<ILanguageMetadataProvider, LanguageMetadataProvider>();
        services.AddSingleton<IManifestService, ManifestService>();
        services.AddSingleton<IFunctionsService, FunctionsService>();
        services.AddSingleton<LanguageListCommand>();
        services.AddSingleton<ProjectGetCommand>();
        services.AddSingleton<TemplateGetCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var functions = new CommandGroup(
            Name,
            "Azure Functions code generation commands. Use these tools to generate functions code, " +
            "explore supported languages and runtime versions, and get composable function templates for Azure Functions development.",
            Title);

        var languageGroup = new CommandGroup(
            "language",
            "Commands for exploring Azure Functions language support and runtime versions.",
            "Language");

        languageGroup.AddCommand(serviceProvider.GetRequiredService<LanguageListCommand>());

        var projectGroup = new CommandGroup(
            "project",
            "Commands for retrieving Azure Functions project initialization templates.",
            "Project");

        projectGroup.AddCommand(serviceProvider.GetRequiredService<ProjectGetCommand>());

        var templateGroup = new CommandGroup(
            "template",
            "Commands for listing and retrieving Azure Functions function code templates.",
            "Template");

        templateGroup.AddCommand(serviceProvider.GetRequiredService<TemplateGetCommand>());

        functions.AddSubGroup(languageGroup);
        functions.AddSubGroup(projectGroup);
        functions.AddSubGroup(templateGroup);

        return functions;
    }
}
