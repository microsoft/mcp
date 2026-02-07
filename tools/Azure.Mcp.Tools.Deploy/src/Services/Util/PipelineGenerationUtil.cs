// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Azure.Mcp.Tools.Deploy.Models.Templates;
using Azure.Mcp.Tools.Deploy.Options.Pipeline;
using Azure.Mcp.Tools.Deploy.Services.Templates;
using Azure.ResourceManager.Network.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using OpenTelemetry;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Azure.Mcp.Tools.Deploy.Services.Util;

/// <summary>
/// Utility class for generating pipeline guidelines using embedded template resources.
/// </summary>
public static class PipelineGenerationUtil
{
    /// <summary>
    /// Generates pipeline guidelines based on the provided options.
    /// </summary>
    /// <param name="options">The guidance options containing pipeline configuration.</param>
    /// <returns>A formatted pipeline guidelines string.</returns>
    public static string GeneratePipelineGuidelines(GuidanceGetOptions options)
    {
        var parameters = CreatePipelineParameters(options);
        return TemplateService.ProcessTemplate("Pipeline/pipeline-to-azure", parameters.ToDictionary());
    }

    private static string GeneratePrerequisiteChecksPrompt(GuidanceGetOptions options)
    {
        var prompt = "";
        if (options.DeployOption == Models.DeployOption.DeployOnly)
        {
            prompt += "- When user confirms that Azure resources are ready for deployment, you need to know at least two things: the resource groups (with environments, e.g., dev, prod) and the hosting service TYPE(e.g., AKS, Azure Container Apps, App Service).\n";
        }
        else if (options.DeployOption == Models.DeployOption.ProvisionAndDeploy)
        {
            prompt += "- When user wants to include provisioning, check if there are available infra files. If not, first run Get Iac(Infrastructure as Code) Rules to create infra-provisioning files.\n";
        }
        if (options.IsAZDProject)
        {
            prompt += "- AZD IaC check: if Bicep is using resource group scope, resource group should be created in advance.\n";
        }
        return prompt;
    }


    private static string GeneratePipelineFilePrompts(GuidanceGetOptions options)
    {
        var prompt = "";
        if (options.PipelinePlatform == Models.PipelinePlatform.GitHubActions)
        {
            prompt += "- Use User-assigned Managed Identity with OIDC for login to Azure in the pipeline.\n" +
                "- Use azure/login@v2 action to set up OIDC authentication in Github Actions. Add 'id-token: write' permission.\n" +
                "- Use variables and secrets for specific values. Do NOT hardcode any values like service names.\n";
        }
        else if (options.PipelinePlatform == Models.PipelinePlatform.AzureDevOps)
        {
            prompt += "- Use Service Principal(app registration) with workflow identity federation to login to Azure in the pipeline.\n";
        }

        if (options.IsAZDProject)
        {
            prompt += "- Use 'azd deploy --no-prompt' to skip provisioning in CD pipeline.\n";
        }
        if (options.DeployOption == Models.DeployOption.ProvisionAndDeploy)
        {
            prompt += "- Add a SEPARATE infra-deploy pipeline file to provision Azure resources.\n";
        }
        return prompt;
    }

    private static string GenerateSetUpMethodPrompts(GuidanceGetOptions options)
    {
        var prompt = "";
        if (options.PipelinePlatform == Models.PipelinePlatform.GitHubActions)
        {
            prompt += "- Create a setup-azure-auth-for-pipeline.sh or setup-azure-auth-for-pipeline.ps1 script to automate the auth configuration. Include detailed commands or UI instructions according to the pipeline platform.\n";
        }
        return prompt;
    }

    private static string GenerateAzureAuthConfigPrompt(GuidanceGetOptions options)
    {
        var prompt = "";
        if (options.PipelinePlatform == Models.PipelinePlatform.GitHubActions)
        {
            prompt += "- Create a new **User-assigned Managed Identity** in a SEPARATE resource group.\n" +
                "- This Managed Identity works for the pipeline. DO NOT CONFUSE it with any existing Managed Identity used by the application.\n" +
                "- Set up Managed Identity with federated credentials and RBAC(e.g.contributor to resource groups and AcrPull) to authenticate(azure login with OIDC), push images and deploy applications to Azure in the pipeline for different environments.\n" +
                "- Federated credentials should be set with proper subject * *to different environments * *(instead of branch), issuer, and audience. Description is NOT a valid property.\n";
        }
        else if (options.PipelinePlatform == Models.PipelinePlatform.AzureDevOps)
        {
            prompt += "Set up Service Connection in Azure DevOps using app registration with workflow identity federation.\n" +
                "Use RBAC (e.g. contributor to resource groups and AcrPull) to the app registration to authenticate, push images and deploy applications to Azure in the pipeline for different environments.\n";
        }
        return prompt;
    }

    private static string GenerateEnvironmentSetupPrompt(GuidanceGetOptions options)
    {
        var prompt = "";
        if (options.PipelinePlatform == Models.PipelinePlatform.GitHubActions)
        {
            prompt += "- Create Github environments and set up approval checks in ALL environments.\n" +
                "- Configure the Github Actions variables and secrets needed for the deployment pipeline for *different environments*.\n" +
                "- Refer to the managed identity created in the 'Azure Authentication Configuration Guidance' section for OIDC setup.\n" +
                "- You can use Github CLI and Github CLI or guide user to use UI to finish the setup.\n";
        }
        else if (options.PipelinePlatform == Models.PipelinePlatform.AzureDevOps)
        {
            prompt += "- Configure the Azure DevOps pipeline variables and service connections needed for the deployment pipeline.\n" +
                "- Set up the ADO environment and set up deployment approvals and checks.\n";
        }
        if (options.IsAZDProject)
        {
            prompt += "- AZD IaC check: if Bicep is using resource group scope, AZURE_RESOURCE_GROUP variable should be set to the environment.\n";
        }
        return prompt;
    }

    /// <summary>
    /// Creates pipeline template parameters from the provided options.
    /// </summary>
    private static PipelineTemplateParameters CreatePipelineParameters(GuidanceGetOptions options)
    {
        options.PipelinePlatform ??= Models.PipelinePlatform.GitHubActions;
        options.DeployOption ??= Models.DeployOption.DeployOnly;

        return new PipelineTemplateParameters
        {
            DeploymentTool = options.IsAZDProject ? "AZD" : "Azure CLI",
            PipelinePlatform = options.PipelinePlatform.ToString(),
            PrerequisiteChecksPrompt = GeneratePrerequisiteChecksPrompt(options),
            PipelineFilePrompt = GeneratePipelineFilePrompts(options),
            SetupMethodPrompt = GenerateSetUpMethodPrompts(options),
            AzureAuthConfigPrompt = GenerateAzureAuthConfigPrompt(options),
            EnvironmentSetupPrompt = GenerateEnvironmentSetupPrompt(options)
        };
    }
}
