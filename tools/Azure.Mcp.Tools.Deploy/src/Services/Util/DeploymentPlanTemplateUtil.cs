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
    /// <param name="azdIacOptions">The Infrastructure as Code options for AZD.</param>
    /// <returns>A formatted deployment plan template string.</returns>
    public static string GetPlanTemplate(string projectName, string targetAppService, string provisioningTool, string? azdIacOptions = "")
    {
        // Default values for optional parameters
        if (provisioningTool == "azd" && string.IsNullOrWhiteSpace(azdIacOptions))
        {
            azdIacOptions = "bicep";
        }

        DeploymentPlanTemplateParameters parameters = CreateTemplateParameters(projectName, targetAppService, provisioningTool, azdIacOptions);
        var executionSteps = GenerateExecutionSteps(parameters);

        parameters.ExecutionSteps = executionSteps;

        return TemplateService.ProcessTemplate("Plan/deployment-plan-base", parameters.ToDictionary());
    }

    /// <summary>
    /// Creates template parameters from the provided inputs.
    /// </summary>
    private static DeploymentPlanTemplateParameters CreateTemplateParameters(
        string projectName,
        string targetAppService,
        string provisioningTool,
        string? azdIacOptions)
    {
        var azureComputeHost = GetAzureComputeHost(targetAppService);
        var title = string.IsNullOrWhiteSpace(projectName)
            ? "Azure Deployment Plan"
            : $"Azure Deployment Plan for {projectName} Project";

        return new DeploymentPlanTemplateParameters
        {
            Title = title,
            ProjectName = projectName,
            TargetAppService = targetAppService,
            ProvisioningTool = provisioningTool,
            IacType = azdIacOptions ?? "bicep",
            AzureComputeHost = azureComputeHost,
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

        steps.Add(TemplateService.LoadTemplate("Plan/azcli-steps"));

        if (isAks)
        {
            steps.Add(TemplateService.LoadTemplate("Plan/aks-steps"));
        }
        else
        {
            var isContainerApp = parameters.TargetAppService.ToLowerInvariant() == "containerapp";
            if (isContainerApp)
            {
                var containerAppReplacements = new Dictionary<string, string>
                {
                    { "AzureComputeHost", parameters.AzureComputeHost }
                };
                steps.Add(TemplateService.ProcessTemplate("Plan/containerapp-steps", containerAppReplacements));
            }
            else
            {
                // For other app services, generate basic deployment steps
                var basicSteps = $"""
                2. Build and Deploy the Application:
                    1. Deploy to {parameters.AzureComputeHost}: Use Azure CLI command to deploy the application
                3. Validation:
                    1. Verify command output to ensure the application is deployed successfully
                """;
                steps.Add(basicSteps);
            }
        }

        steps.Add(TemplateService.ProcessTemplate("Plan/summary-steps", new Dictionary<string, string> { { "StepNumber", "4" } }));

        return steps;
    }
}
