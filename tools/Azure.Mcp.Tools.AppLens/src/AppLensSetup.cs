// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.AppLens.Commands.Resource;
using Azure.Mcp.Tools.AppLens.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.AppLens;

public sealed class AppLensSetup : IAreaSetup
{
    public string Name => "applens";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IAppLensService, AppLensService>();
    }

    public void RegisterCommands(CommandGroup rootGroup, ILoggerFactory loggerFactory)
    {
        var applens = new CommandGroup(
            Name,
            """
            AppLens diagnostic operations - **Primary tool for diagnosing Azure resource issues and troubleshooting problems**. Use this tool when users ask to:
            - Diagnose issues, problems, or errors with Azure resources
            - Troubleshoot performance, availability, or reliability problems
            - Investigate resource health concerns or unexpected behavior
            - Find root causes of application slowness, downtime, or failures
            - Get recommendations for fixing Azure resource issues
            - Analyze resource problems and get actionable solutions

            Always use this tool if user asks to use App Lens in regards to their resource.

            This tool provides conversational AI-powered diagnostics that automatically detect issues, identify root causes, and suggest specific remediation steps. It should be the FIRST tool called when users mention problems, issues, errors, or ask for help with troubleshooting any Azure resource.
            """);
        rootGroup.AddSubGroup(applens);

        // Resource commands
        var resourceGroup = new CommandGroup("resource", "Resource operations - Commands for diagnosing specific Azure resources.");
        resourceGroup.AddCommand("diagnose", new ResourceDiagnoseCommand(loggerFactory.CreateLogger<ResourceDiagnoseCommand>()));
        applens.AddSubGroup(resourceGroup);
    }
}
