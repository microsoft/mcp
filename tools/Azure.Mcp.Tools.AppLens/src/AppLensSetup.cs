// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AppLens.Commands.Resource;
using Azure.Mcp.Tools.AppLens.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.AppLens;

public sealed class AppLensSetup : IAreaSetup
{
    public string Name => "applens";

    public string Title => "Azure AppLens Diagnostics";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IAppLensService, AppLensService>();

        services.AddSingleton<ResourceDiagnoseCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var applens = new CommandGroup(
            Name,
            """
            AppLens diagnostic operations – Primary tool for diagnosing and
            troubleshooting Azure resource issues. Uses conversational AI-powered
            diagnostics to detect problems, identify root causes, and recommend
            remediation steps. This tool should be the first choice when users
            report errors, performance issues, availability problems, or unexpected
            Azure resource behavior.
            """,
            Title);

        // Resource commands
        var resourceGroup = new CommandGroup("resource", "Resource operations - Commands for diagnosing specific Azure resources.");

        var diagnose = serviceProvider.GetRequiredService<ResourceDiagnoseCommand>();
        resourceGroup.AddCommand(diagnose.Name, diagnose);

        applens.AddSubGroup(resourceGroup);

        return applens;
    }
}
