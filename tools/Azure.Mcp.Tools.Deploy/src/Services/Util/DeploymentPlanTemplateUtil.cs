// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Deploy.Models;
using Azure.Mcp.Tools.Deploy.Models.Templates;
using Azure.Mcp.Tools.Deploy.Services.Templates;

namespace Azure.Mcp.Tools.Deploy.Services.Util;

/// <summary>
/// Refactored utility class for generating deployment plan templates using embedded resources.
/// </summary>
public static class DeploymentPlanTemplateUtil
{
    /// <summary>
    /// Generates a deployment plan template using embedded template resources.
    /// </summary>
    /// <param name="projectName">The name of the project. Can be null or empty.</param>
    /// <param name="targetAppService">The target Azure service.</param>
    /// <param name="provisioningTool">The provisioning tool.</param>
    /// <param name="iacOptions">The Infrastructure as Code options for AZD.</param>
    /// <returns>A formatted deployment plan template string.</returns>
    public static string GetPlanTemplate(string projectName, string targetAppService, string provisioningTool, string sourceType, string deployOption, string? iacOptions, string? subscriptionId, string? resourceGroupName)
    {
        // Default values for optional parameters
        if (provisioningTool == "azd" && string.IsNullOrWhiteSpace(iacOptions))
        {
            iacOptions = "bicep";
        }

        DeploymentPlanTemplateParameters parameters = CreateTemplateParameters(projectName, targetAppService, provisioningTool, sourceType, deployOption, iacOptions, subscriptionId, resourceGroupName);
        var resourceInfo = GenerateResourceInfo(parameters);
        var executionSteps = GenerateExecutionSteps(parameters);

        parameters.ExecutionSteps = executionSteps;
        parameters.ResourceInfo = resourceInfo;

        return TemplateService.ProcessTemplate("Plan/deployment-plan-base", parameters.ToDictionary());
    }

    /// <summary>
    /// Creates template parameters from the provided inputs.
    /// </summary>
    private static DeploymentPlanTemplateParameters CreateTemplateParameters(
        string projectName,
        string targetAppService,
        string provisioningTool,
        string sourceType,
        string deployOption,
        string? iacOptions,
        string? subscriptionId,
        string? resourceGroupName)
    {
        var azureComputeHost = GetAzureComputeHost(targetAppService);
        var title = string.IsNullOrWhiteSpace(projectName)
            ? "Azure Deployment Plan"
            : $"Azure Deployment Plan for {projectName} Project";

        if (deployOption == DeployOption.DeployOnly)
        {
            provisioningTool = DeploymentTool.AzCli;
            sourceType = SourceType.FromAzure;
        }

        if (deployOption == DeployOption.ProvisionOnly)
        {
            provisioningTool = DeploymentTool.AzCli;
        }

        var fallbackIaCTypeDescription = "";
        if (string.IsNullOrEmpty(iacOptions) && (deployOption == DeployOption.ProvisionOnly || deployOption == DeployOption.ProvisionAndDeploy))
        {
            iacOptions = targetAppService.ToLowerInvariant() == "aks" ? IacType.Terraform : IacType.Bicep;
            fallbackIaCTypeDescription = $" Since the IaC option is not specified, we will use ${iacOptions} as the IaC option based on the target app services.";
        }

        var goal = sourceType == SourceType.FromAzure ?
            $"Based on the project to provide a plan to deploy the project to Azure ${targetAppService} in resource group ${resourceGroupName ?? "YOUR RG"} and subscription ${subscriptionId ?? "YOUR SUBSCRIPTION"} with tool ${provisioningTool.ToUpperInvariant()}.${fallbackIaCTypeDescription}" :
            $"Based on the project to provide a plan to deploy the project to Azure using ${provisioningTool.ToUpperInvariant()}.${fallbackIaCTypeDescription}";

        if (deployOption == DeployOption.ProvisionOnly)
        {
            goal = $"Provide a plan to provision Azure resources for the project with ${provisioningTool.ToUpperInvariant()}${(string.IsNullOrEmpty(iacOptions) ? "" : " and " + iacOptions)}.${fallbackIaCTypeDescription}";
        }

        var sampleMermaid = "";
        if (targetAppService.ToLowerInvariant() == "aks")
        {
            sampleMermaid = TemplateService.LoadTemplate("Plan/sample-aks-mermaid");
        }
        else
        {
            sampleMermaid = TemplateService.LoadTemplate("Plan/sample-app-mermaid");
        }

        return new DeploymentPlanTemplateParameters
        {
            Title = title,
            ProjectName = projectName,
            TargetAppService = targetAppService,
            ProvisioningTool = provisioningTool,
            IacType = iacOptions ?? "",
            AzureComputeHost = azureComputeHost,
            SourceType = sourceType,
            DeployOption = deployOption,
            Goal = goal,
            SampleMermaid = sampleMermaid
        };
    }

    /// <summary>
    /// Gets the Azure compute host display name from the target app service.
    /// </summary>
    private static string GetAzureComputeHost(string targetAppService)
    {
        return targetAppService.ToLowerInvariant() switch
        {
            "containerapp" => "Azure Container Apps",
            "webapp" => "Azure Web App Service",
            "functionapp" => "Azure Functions",
            "aks" => "Azure Kubernetes Service",
            _ => "Azure Container Apps"
        };
    }

    /// <summary>
    /// Generates execution steps based on the deployment parameters.
    /// </summary>
    private static string GenerateExecutionSteps(DeploymentPlanTemplateParameters parameters)
    {
        var steps = new List<string>();
        var isAks = parameters.TargetAppService.ToLowerInvariant() == "aks";

        if (parameters.ProvisioningTool.ToLowerInvariant() == "azd")
        {
            steps.AddRange(GenerateAzdSteps(parameters, isAks));
        }
        else if (parameters.ProvisioningTool.Equals(DeploymentTool.AzCli, StringComparison.OrdinalIgnoreCase))
        {
            steps.AddRange(GenerateAzCliSteps(parameters, isAks));
        }

        return string.Join(Environment.NewLine, steps);
    }

    /// <summary>
    /// Generates AZD-specific execution steps.
    /// </summary>
    private static List<string> GenerateAzdSteps(DeploymentPlanTemplateParameters parameters, bool isAks)
    {
        var steps = new List<string>();

        var deployTitle = isAks ? "" : " And Deploy the Application";
        var checkLog = isAks ? "" : "6. Check the application log with tool `azd-app-log-get` to ensure the services are running.";
        var iacRuleTool = parameters.IacType.ToLowerInvariant() == "bicep" ? "Then call tool #mcp_azure_mcp_azd with input command='infrastructure_generation' to get instructions for generating modular Bicep infrastructure templates following Azure security and operational best practices for azd projects." : "Get the IaC rules from the tool `iac-rules-get`.";
        var azdStepReplacements = new Dictionary<string, string>
        {
            { "DeployTitle", deployTitle },
            { "IacType", parameters.IacType },
            { "CheckLog", checkLog },
            { "IaCRuleTool", iacRuleTool}
        };

        var azdSteps = TemplateService.ProcessTemplate("Plan/azd-steps", azdStepReplacements);
        steps.Add(azdSteps);

        if (isAks)
        {
            steps.Add(TemplateService.LoadTemplate("Plan/aks-steps"));
            steps.Add(TemplateService.ProcessTemplate("Plan/summary-steps", new Dictionary<string, string> { { "StepNumber", "4" } }));
        }
        else
        {
            steps.Add(TemplateService.ProcessTemplate("Plan/summary-steps", new Dictionary<string, string> { { "StepNumber", "2" } }));
        }

        return steps;
    }

    /// <summary>
    /// Generates Azure CLI-specific execution steps.
    /// </summary>
    private static List<string> GenerateAzCliSteps(DeploymentPlanTemplateParameters parameters, bool isAks)
    {
        var steps = new List<string>();
        if (isAks)
        {
            if (parameters.DeployOption == DeployOption.DeployOnly)
            {
                steps.Add(TemplateService.LoadTemplate("Plan/aks-deploy-only-steps"));
            }
            else
            {
                steps.Add(TemplateService.LoadTemplate("Plan/aks-provision-steps"));
                if (parameters.DeployOption == DeployOption.ProvisionAndDeploy)
                {
                    steps.Add(TemplateService.LoadTemplate("Plan/aks-deployment-steps"));
                }
            }
        }
        else
        {
            var cliDeploymentSteps = "";
            if (parameters.TargetAppService.ToLowerInvariant() == "containerapp" && parameters.DeployOption != DeployOption.ProvisionOnly)
            {
                steps.Add(TemplateService.LoadTemplate("Plan/containerization-steps"));
                cliDeploymentSteps = "1. Create deploy script (build + push image to ACR, deploy to Azure Container App).\n2. Run the script and fix it until it works.";
            }
            else
            {
                cliDeploymentSteps = "1. Create deploy script to deploy the application with Azure CLI.";
            }

            var cliParameters = new CLIExecutionStepsTemplateParameters
            {
                IaCType = parameters.IacType.ToLowerInvariant(),
                AzureComputeHost = parameters.AzureComputeHost,
                TargetAppCommandTitle = parameters.TargetAppService,
                DeploymentSteps = cliDeploymentSteps,
                ACRDependencyCheck = parameters.TargetAppService.ToLowerInvariant() == "containerapp"
                    ? "- Check Azure Container Registry:\n- login server: <>. Check with \'az acr show -o json\'."
                    : ""
            };

            if (parameters.DeployOption == DeployOption.DeployOnly)
            {
                steps.Add(TemplateService.ProcessTemplate("Plan/azcli-deploy-only-steps", cliParameters.ToDictionary()));
            }
            else
            {
                steps.Add(TemplateService.ProcessTemplate("Plan/azcli-provision-steps", cliParameters.ToDictionary()));
                if (parameters.DeployOption == DeployOption.ProvisionAndDeploy)
                {
                    steps.Add(TemplateService.ProcessTemplate("Plan/azcli-deployment-steps", cliParameters.ToDictionary()));
                }
            }
        }

        steps.Add(TemplateService.LoadTemplate("Plan/summary-steps"));

        return steps;

    }

    private static string GenerateResourceInfo(DeploymentPlanTemplateParameters parameters)
    {
        if (parameters.DeployOption == DeployOption.DeployOnly)
        {
            return TemplateService.LoadTemplate("Plan/existing-resource-info");
        }
        else
        {
            return TemplateService.ProcessTemplate("Plan/provision-info", new Dictionary<string, string>
            {
                { "ProjectName", parameters.ProjectName },
                { "AzureComputeHost", parameters.AzureComputeHost }
            });
        }
    }
}
