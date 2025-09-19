// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Deploy.Commands.App;
using Azure.Mcp.Tools.Deploy.Commands.Architecture;
using Azure.Mcp.Tools.Deploy.Commands.Infrastructure;
using Azure.Mcp.Tools.Deploy.Commands.Pipeline;
using Azure.Mcp.Tools.Deploy.Commands.Plan;
using Azure.Mcp.Tools.Deploy.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Azure.Mcp.Tools.Deploy;

public sealed class DeploySetup : IAreaSetup
{
    public string Name => "deploy";

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
        var deploy = new CommandGroup(Name, "Deploy commands for deploying applications to Azure, including sub commands: "
            + "- plan get: generates a deployment plan to construct the infrastructure and deploy the application on Azure. Agent should read its output and generate a deploy plan in '.azure/plan.copilotmd' for execution steps, recommended azure services based on the information agent detected from project. Before calling this tool, please scan this workspace to detect the services to deploy and their dependent services; "
            + "- iac rules get: offers guidelines for creating Bicep/Terraform files to deploy applications on Azure; "
            + "- app logs get: fetch logs from log analytics workspace for Container Apps, App Services, function apps that were deployed through azd; "
            + "- pipeline guidance get: guidance to create a CI/CD pipeline which provision Azure resources and build and deploy applications to Azure; "
            + "- architecture diagram generate: generates an azure service architecture diagram for the application based on the provided app topology; ");

        // Application-specific commands
        // This command will be deprecated when 'azd cli' supports the same functionality
        var appGroup = new CommandGroup("app", "Application-specific deployment tools");
        var logsGroup = new CommandGroup("logs", "Application logs management");
        var logsGet = serviceProvider.GetRequiredService<LogsGetCommand>();
        logsGroup.AddCommand(logsGet.Name, logsGet);
        appGroup.AddSubGroup(logsGroup);
        deploy.AddSubGroup(appGroup);

        // Infrastructure as Code commands
        var iacGroup = new CommandGroup("iac", "Infrastructure as Code operations");
        var rulesGroup = new CommandGroup("rules", "Infrastructure as Code rules and guidelines");
        var rulesGet = serviceProvider.GetRequiredService<RulesGetCommand>();
        rulesGroup.AddCommand(rulesGet.Name, rulesGet);
        iacGroup.AddSubGroup(rulesGroup);
        deploy.AddSubGroup(iacGroup);

        // CI/CD Pipeline commands
        var pipelineGroup = new CommandGroup("pipeline", "CI/CD pipeline operations");
        var guidanceGroup = new CommandGroup("guidance", "CI/CD pipeline guidance");
        var guidanceGet = serviceProvider.GetRequiredService<GuidanceGetCommand>();
        guidanceGroup.AddCommand(guidanceGet.Name, guidanceGet);
        pipelineGroup.AddSubGroup(guidanceGroup);
        deploy.AddSubGroup(pipelineGroup);

        // Deployment planning commands
        var planGroup = new CommandGroup("plan", "Deployment planning operations");
        var getPlan = serviceProvider.GetRequiredService<GetCommand>();
        planGroup.AddCommand(getPlan.Name, getPlan);
        deploy.AddSubGroup(planGroup);

        // Architecture diagram commands
        var architectureGroup = new CommandGroup("architecture", "Architecture diagram operations");
        var diagramGroup = new CommandGroup("diagram", "Architecture diagram generation");
        var diagramGenerate = serviceProvider.GetRequiredService<DiagramGenerateCommand>();
        diagramGroup.AddCommand(diagramGenerate.Name, diagramGenerate);
        architectureGroup.AddSubGroup(diagramGroup);
        deploy.AddSubGroup(architectureGroup);

        return deploy;
    }
}
