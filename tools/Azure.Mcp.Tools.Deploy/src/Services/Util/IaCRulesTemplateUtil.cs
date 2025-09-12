// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Deploy.Models;
using Azure.Mcp.Tools.Deploy.Models.Templates;
using Azure.Mcp.Tools.Deploy.Services.Templates;

namespace Azure.Mcp.Tools.Deploy.Services.Util;

/// <summary>
/// Utility class for generating IaC rules using embedded templates.
/// </summary>
public static class IaCRulesTemplateUtil
{
    /// <summary>
    /// Generates IaC rules using embedded templates.
    /// </summary>
    /// <param name="deploymentTool">The deployment tool (azd, azcli).</param>
    /// <param name="iacType">The IaC type (bicep, terraform).</param>
    /// <param name="resourceTypes">Array of resource types.</param>
    /// <returns>A formatted IaC rules string.</returns>
    public static string GetIaCRules(string deploymentTool, string iacType, string[] resourceTypes)
    {
        var parameters = CreateTemplateParameters(deploymentTool, iacType, resourceTypes);
        var deploymentToolRules = GenerateDeploymentToolRules(parameters);
        if (deploymentTool.Equals(DeploymentTool.AzCli, StringComparison.OrdinalIgnoreCase))
        {
            return TemplateService.LoadTemplate("IaCRules/azcli-rules");
        }
        // Default values for optional parameters
        if (string.IsNullOrWhiteSpace(iacType))
        {
            iacType = "bicep";
        }
        var iacTypeRules = GenerateIaCTypeRules(parameters);
        var resourceSpecificRules = GenerateResourceSpecificRules(parameters);
        var finalInstructions = GenerateFinalInstructions(parameters);

        parameters.DeploymentToolRules = deploymentToolRules;
        parameters.IacTypeRules = iacTypeRules;
        parameters.ResourceSpecificRules = resourceSpecificRules;
        parameters.FinalInstructions = finalInstructions;
        parameters.RequiredTools = BuildRequiredTools(deploymentTool, resourceTypes);
        parameters.AdditionalNotes = BuildAdditionalNotes(deploymentTool, iacType);

        return TemplateService.ProcessTemplate("IaCRules/base-iac-rules", parameters.ToDictionary());
    }

    /// <summary>
    /// Creates template parameters from the provided inputs.
    /// </summary>
    private static IaCRulesTemplateParameters CreateTemplateParameters(
        string deploymentTool,
        string iacType,
        string[] resourceTypes)
    {
        var parameters = new IaCRulesTemplateParameters
        {
            DeploymentTool = deploymentTool,
            IacType = iacType,
            ResourceTypes = resourceTypes,
            ResourceTypesDisplay = string.Join(", ", resourceTypes)
        };

        // Set IaC type specific parameters
        SetIaCTypeSpecificParameters(parameters);

        return parameters;
    }

    /// <summary>
    /// Sets IaC type specific parameters.
    /// </summary>
    private static void SetIaCTypeSpecificParameters(IaCRulesTemplateParameters parameters)
    {
        parameters.OutputFileName = parameters.IacType == IacType.Bicep ? "main.bicep" : "outputs.tf";
        parameters.RoleAssignmentResource = parameters.IacType == IacType.Bicep
            ? "Microsoft.Authorization/roleAssignments"
            : "azurerm_role_assignment";
        parameters.ImageProperty = parameters.IacType == IacType.Bicep
            ? "properties.template.containers.image"
            : "azurerm_container_app.template.container.image";
        parameters.DiagnosticSettingsResource = parameters.IacType == IacType.Bicep
            ? "Microsoft.Insights/diagnosticSettings"
            : "azurerm_monitor_diagnostic_setting";

        // Set CORS configuration based on IaC type
        if (parameters.IacType == IacType.Bicep)
        {
            parameters.CorsConfiguration = "- Enable CORS via properties.configuration.ingress.corsPolicy.";
        }
        else if (parameters.IacType == IacType.Terraform)
        {
            parameters.CorsConfiguration = "- Create an ***azapi_resource_action*** resource using :type `Microsoft.App/containerApps`, method `PATCH`, and body `properties.configuration.ingress.corsPolicy` property to enable CORS for all origins, headers, and methods. Use 'azure/azapi' provider version *2.0*. DO NOT use jsonencode() for the body.";
        }

        // Set Log Analytics configuration based on IaC type
        if (parameters.IacType == IacType.Bicep)
        {
            parameters.LogAnalyticsConfiguration = "- Container App Environment must be connected to Log Analytics Workspace. Use logAnalyticsConfiguration -> customerId=logAnalytics.properties.customerId and sharedKey=logAnalytics.listKeys().primarySharedKey.";
        }
        else
        {
            parameters.LogAnalyticsConfiguration = "- Container App Environment must be connected to Log Analytics Workspace. Use logs_destination=\"log-analytics\" azurerm_container_app_environment.log_analytics_workspace_id = azurerm_log_analytics_workspace.<workspaceName>.id.";
        }
    }

    /// <summary>
    /// Generates deployment tool specific rules.
    /// </summary>
    private static string GenerateDeploymentToolRules(IaCRulesTemplateParameters parameters)
    {
        if (parameters.DeploymentTool.Equals(DeploymentTool.Azd, StringComparison.OrdinalIgnoreCase))
        {
            var containerRegistryOutput = parameters.ResourceTypes.Contains(AzureServiceNames.AzureContainerApp)
                ? "\n- Expected output in " + parameters.OutputFileName + ": AZURE_CONTAINER_REGISTRY_ENDPOINT representing the URI of the container registry endpoint."
                : string.Empty;

            var azdReplacements = new Dictionary<string, string>
            {
                { "IacType", parameters.IacType },
                { "OutputFileName", parameters.OutputFileName },
                { "ContainerRegistryOutput", containerRegistryOutput }
            };

            return TemplateService.ProcessTemplate("IaCRules/azd-rules", azdReplacements);
        }
        else if (parameters.DeploymentTool.Equals(DeploymentTool.AzCli, StringComparison.OrdinalIgnoreCase))
        {
            return TemplateService.LoadTemplate("IaCRules/azcli-rules");
        }

        return string.Empty;
    }

    /// <summary>
    /// Generates IaC type specific rules.
    /// </summary>
    private static string GenerateIaCTypeRules(IaCRulesTemplateParameters parameters)
    {
        return parameters.IacType switch
        {
            IacType.Bicep => TemplateService.LoadTemplate("IaCRules/bicep-rules"),
            IacType.Terraform => TemplateService.LoadTemplate("IaCRules/terraform-rules"),
            _ => string.Empty
        };
    }

    /// <summary>
    /// Generates resource specific rules.
    /// </summary>
    private static string GenerateResourceSpecificRules(IaCRulesTemplateParameters parameters)
    {
        var rules = new List<string>();

        if (parameters.ResourceTypes.Contains(AzureServiceNames.AzureContainerApp))
        {
            rules.Add(TemplateService.ProcessTemplate("IaCRules/containerapp-rules", parameters.ToDictionary()));
        }

        if (parameters.ResourceTypes.Contains(AzureServiceNames.AzureAppService))
        {
            rules.Add(TemplateService.ProcessTemplate("IaCRules/appservice-rules", parameters.ToDictionary()));
        }

        if (parameters.ResourceTypes.Contains(AzureServiceNames.AzureFunctionApp))
        {
            rules.Add(TemplateService.ProcessTemplate("IaCRules/functionapp-rules", parameters.ToDictionary()));
        }

        if (parameters.ResourceTypes.Contains(AzureServiceNames.AzureStorage))
        {
            rules.Add(TemplateService.ProcessTemplate("IaCRules/storage-rules", parameters.ToDictionary()));
        }

        return string.Join(Environment.NewLine, rules);
    }

    /// <summary>
    /// Generates final instructions for the IaC rules.
    /// </summary>
    private static string GenerateFinalInstructions(IaCRulesTemplateParameters parameters)
    {
        return TemplateService.ProcessTemplate("IaCRules/final-instructions", parameters.ToDictionary());
    }

    /// <summary>
    /// Builds the required tools list based on deployment tool and resource types.
    /// </summary>
    private static string BuildRequiredTools(string deploymentTool, string[] resourceTypes)
    {
        var tools = new List<string> { "az cli (az --version)" };

        if (deploymentTool == DeploymentTool.Azd)
        {
            tools.Add("azd (azd version)");
        }

        if (resourceTypes.Contains(AzureServiceNames.AzureContainerApp))
        {
            tools.Add("docker (docker --version)");
        }

        return string.Join(", ", tools) + ".";
    }

    /// <summary>
    /// Builds additional notes based on deployment tool and IaC type.
    /// </summary>
    private static string BuildAdditionalNotes(string deploymentTool, string iacType)
    {
        if (iacType == IacType.Terraform && deploymentTool == DeploymentTool.Azd)
        {
            return "Note: Do not use Terraform CLI.";
        }

        return string.Empty;
    }
}
