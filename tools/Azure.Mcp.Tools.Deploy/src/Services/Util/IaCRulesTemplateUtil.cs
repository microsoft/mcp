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
}