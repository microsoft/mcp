// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Deploy.Commands.App;
using Azure.Mcp.Tools.Deploy.Commands.Architecture;
using Azure.Mcp.Tools.Deploy.Commands.Infrastructure;
using Azure.Mcp.Tools.Deploy.Commands.Pipeline;
using Azure.Mcp.Tools.Deploy.Commands.Plan;
using Azure.Mcp.Tools.Deploy.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.Deploy;

public sealed class DeploySetup : IAreaSetup
{
    public string Name => "deploy";

    public string Title => "Azure Deployment";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IDeployService, DeployService>();

        services.AddSingleton<LogsGetCommand>();
        services.AddSingleton<RulesGetCommand>();
        services.AddSingleton<GuidanceGetCommand>();
        services.AddSingleton<GetCommand>();
        services.AddSingleton<DiagramGenerateCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var deploy = new CommandGroup(Name, "Deploy operations – Commands for deploying applications to Azure. Provides sub-commands to generate deployment plans, offer infrastructure-as-code (Bicep/Terraform) guidance, fetch application logs, generate CI/CD pipeline guidance, and produce Azure architecture diagrams based on application topology.", Title);

        // Application-specific commands
        // This command will be deprecated when 'azd cli' supports the same functionality
        var appGroup = new CommandGroup("app", "Application-specific deployment tools");
        var logsGroup = new CommandGroup("logs", "Application logs management");
        logsGroup.AddCommand(serviceProvider.GetRequiredService<LogsGetCommand>());
        appGroup.AddSubGroup(logsGroup);
        deploy.AddSubGroup(appGroup);

        // Infrastructure as Code commands
        var iacGroup = new CommandGroup("iac", "Infrastructure as Code operations");
        var rulesGroup = new CommandGroup("rules", "Infrastructure as Code rules and guidelines");
        rulesGroup.AddCommand(serviceProvider.GetRequiredService<RulesGetCommand>());
        iacGroup.AddSubGroup(rulesGroup);
        deploy.AddSubGroup(iacGroup);

        // CI/CD Pipeline commands
        var pipelineGroup = new CommandGroup("pipeline", "CI/CD pipeline operations");
        var guidanceGroup = new CommandGroup("guidance", "CI/CD pipeline guidance");
        guidanceGroup.AddCommand(serviceProvider.GetRequiredService<GuidanceGetCommand>());
        pipelineGroup.AddSubGroup(guidanceGroup);
        deploy.AddSubGroup(pipelineGroup);

        // Deployment planning commands
        var planGroup = new CommandGroup("plan", "Deployment planning operations");
        planGroup.AddCommand(serviceProvider.GetRequiredService<GetCommand>());
        deploy.AddSubGroup(planGroup);

        // Architecture diagram commands
        var architectureGroup = new CommandGroup("architecture", "Architecture diagram operations");
        var diagramGroup = new CommandGroup("diagram", "Architecture diagram generation");
        diagramGroup.AddCommand(serviceProvider.GetRequiredService<DiagramGenerateCommand>());
        architectureGroup.AddSubGroup(diagramGroup);
        deploy.AddSubGroup(architectureGroup);

        return deploy;
    }
}
