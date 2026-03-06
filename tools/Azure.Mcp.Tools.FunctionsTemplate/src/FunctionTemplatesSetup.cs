// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.FunctionsTemplate.Commands.Language;
using Azure.Mcp.Tools.FunctionsTemplate.Commands.Project;
using Azure.Mcp.Tools.FunctionsTemplate.Commands.Template;
using Azure.Mcp.Tools.FunctionsTemplate.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.FunctionsTemplate;

public sealed class FunctionTemplatesSetup : IAreaSetup
{
    public string Name => "functiontemplates";

    public string Title => "Azure Functions Templates";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IFunctionTemplatesService, FunctionTemplatesService>();
        services.AddHttpClient<FunctionTemplatesService>();
        services.AddSingleton<LanguageListCommand>();
        services.AddSingleton<ProjectGetCommand>();
        services.AddSingleton<TemplateGetCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var functionsTemplates = new CommandGroup(
            Name,
            "Azure Functions template discovery commands. Use these tools to explore supported " +
            "languages, runtime versions, and capabilities for Azure Functions development.",
            Title);

        var languageGroup = new CommandGroup(
            "language",
            "Commands for exploring Azure Functions language support and runtime versions.",
            "Language");

        var listCommand = serviceProvider.GetRequiredService<LanguageListCommand>();
        languageGroup.AddCommand(listCommand.Name, listCommand);

        var projectGroup = new CommandGroup(
            "project",
            "Commands for retrieving Azure Functions project initialization templates.",
            "Project");

        var projectGetCommand = serviceProvider.GetRequiredService<ProjectGetCommand>();
        projectGroup.AddCommand(projectGetCommand.Name, projectGetCommand);

        var templateGroup = new CommandGroup(
            "template",
            "Commands for listing and retrieving Azure Functions function templates.",
            "Template");

        var templateGetCommand = serviceProvider.GetRequiredService<TemplateGetCommand>();
        templateGroup.AddCommand(templateGetCommand.Name, templateGetCommand);

        functionsTemplates.AddSubGroup(languageGroup);
        functionsTemplates.AddSubGroup(projectGroup);
        functionsTemplates.AddSubGroup(templateGroup);

        return functionsTemplates;
    }
}
