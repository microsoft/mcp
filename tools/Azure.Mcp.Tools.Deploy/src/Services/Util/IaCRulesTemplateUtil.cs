// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Deploy.Models;
using Azure.Mcp.Tools.Deploy.Models.Templates;
using Azure.Mcp.Tools.Deploy.Services.Templates;
using Microsoft.AspNetCore.DataProtection;
using ModelContextProtocol.Protocol;
using YamlDotNet.Core.Tokens;

namespace Azure.Mcp.Tools.Deploy.Services.Util;

/// <summary>
/// Utility class for generating IaC rules using embedded templates.
/// </summary>
public static class IaCRulesTemplateUtil
{
    private static string databaseCommonRules = TemplateService.LoadTemplate("IaCRules/database-common-rules");

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
        // Default values for optional parameters
        if (deploymentTool.Equals(DeploymentTool.Azd, StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(iacType))
        {
            iacType = "bicep";
        }

        parameters.DeploymentToolRules = GenerateDeploymentToolRules(parameters);
        parameters.IacTypeRules = GenerateIaCTypeRules(parameters);
        parameters.ResourceSpecificRules = GenerateResourceSpecificRules(parameters);
        parameters.FinalInstructions = GenerateFinalInstructions(parameters);
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

        return parameters;
    }

    /// <summary>
    /// Generates deployment tool specific rules.
    /// </summary>
    private static string GenerateDeploymentToolRules(IaCRulesTemplateParameters parameters)
    {
        if (parameters.DeploymentTool.Equals(DeploymentTool.Azd, StringComparison.OrdinalIgnoreCase))
        {

            return "Agent must call tool #mcp_azure_mcp_azd with input command='iac_generation_rules' to get rules for AZD.";
        }
        else if (parameters.DeploymentTool.Equals(DeploymentTool.AzCli, StringComparison.OrdinalIgnoreCase))
        {
            var kubernetesYamlNamingRule = "- Kubernetes (K8s) YAML naming: only Lowercase letters (a-z), digits (0-9), hyphens (-) is allowed. Must start and end with a letter or digit. Less than 20 characters.";
            return TemplateService.ProcessTemplate("IaCRules/azcli-rules", new Dictionary<string, string>
            {
                { "KubernetesYamlNamingRule", kubernetesYamlNamingRule },
                { "AzCliScriptRules", TemplateService.LoadTemplate("IaCRules/azcli-script-rules") }
            });
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
            _ => "No IaC is used. Review the rules for Az CLI scripts."
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
            rules.Add(GenerateContainerAppRules(parameters));
        }

        if (parameters.ResourceTypes.Contains(AzureServiceNames.AzureAppService))
        {
            rules.Add(GenerateAppServiceRules(parameters));
        }

        if (parameters.ResourceTypes.Contains(AzureServiceNames.AzureFunctionApp))
        {
            rules.Add(GenerateFunctionAppRules(parameters));
        }

        if (parameters.ResourceTypes.Contains(AzureServiceNames.AzureKubernetesService))
        {
            rules.Add(GenerateAKSRules(parameters));
        }

        if (parameters.ResourceTypes.Contains(AzureServiceNames.AzureDatabaseForPostgreSql))
        {
            rules.Add(GeneratePostgreSqlRules(parameters));
        }

        if (parameters.ResourceTypes.Contains(AzureServiceNames.AzureDatabaseForMySql))
        {
            rules.Add(GenerateMySqlRules(parameters));
        }

        if (parameters.ResourceTypes.Contains(AzureServiceNames.AzureCosmosDb))
        {
            rules.Add(GenerateCosmosDbRules(parameters));
        }

        if (parameters.ResourceTypes.Contains(AzureServiceNames.AzureStorageAccount))
        {
            rules.Add(GenerateStorageRules(parameters));
        }

        rules.Add(GenerateKeyVaultRules(parameters));

        return string.Join(Environment.NewLine, rules);
    }

    private static string GetToolSpecificResourceRules(string iacType, string? bicepRules, string? tfRules, string? cliRules)
    {
        return iacType switch
        {
            IacType.Bicep => bicepRules ?? string.Empty,
            IacType.Terraform => tfRules ?? string.Empty,
            _ => cliRules ?? string.Empty,
        };
    }

    private static string GenerateContainerAppRules(IaCRulesTemplateParameters parameters)
    {
        var bicepRules = TemplateService.LoadTemplate("IaCRules/containerapp-bicep-rules");
        var tfRules = TemplateService.LoadTemplate("IaCRules/containerapp-tf-rules");
        return TemplateService.ProcessTemplate("IaCRules/containerapp-rules", new Dictionary<string, string> {
            { "ToolSpecificRules", GetToolSpecificResourceRules(parameters.IacType, bicepRules, tfRules, null)}
        });
    }

    private static string GenerateAppServiceRules(IaCRulesTemplateParameters parameters)
    {
        var bicepRules = TemplateService.LoadTemplate("IaCRules/appservice-bicep-rules");
        var tfRules = TemplateService.LoadTemplate("IaCRules/appservice-tf-rules");
        return TemplateService.ProcessTemplate("IaCRules/appservice-rules", new Dictionary<string, string> {
            { "ToolSpecificRules", GetToolSpecificResourceRules(parameters.IacType, bicepRules, tfRules, null)}
        });
    }

    private static string GenerateFunctionAppRules(IaCRulesTemplateParameters parameters)
    {
        var bicepRules = TemplateService.LoadTemplate("IaCRules/functionapp-bicep-rules");
        var tfRules = TemplateService.LoadTemplate("IaCRules/functionapp-tf-rules");
        return TemplateService.ProcessTemplate("IaCRules/functionapp-rules", new Dictionary<string, string> {
            { "ToolSpecificRules", GetToolSpecificResourceRules(parameters.IacType, bicepRules, tfRules, null)}
        });
    }

    private static string GenerateAKSRules(IaCRulesTemplateParameters parameters)
    {
        var bicepRules = TemplateService.LoadTemplate("IaCRules/aks-bicep-rules");
        var tfRules = TemplateService.LoadTemplate("IaCRules/aks-tf-rules");
        var cliRules = TemplateService.LoadTemplate("IaCRules/aks-cli-rules");
        return TemplateService.ProcessTemplate("IaCRules/aks-rules", new Dictionary<string, string> {
            { "ToolSpecificRules", GetToolSpecificResourceRules(parameters.IacType, bicepRules, tfRules, cliRules)},
            { "KeyvaultIntegrationRules", TemplateService.LoadTemplate("IaCRules/aks-kv-integration-rules") }
        });
    }

    private static string GeneratePostgreSqlRules(IaCRulesTemplateParameters parameters)
    {
        var versionRules = parameters.IacType.Equals(IacType.Terraform, StringComparison.OrdinalIgnoreCase)
            ? "- PostgreSQL SKU name format: B_Standard_B1ms(Burstable tier), GP_Standard_D2s_v3(GeneralPurpose), MO_Standard_E4s_v3(MemoryOptimized)\n- For version, prefer to use '16'."
            : "For version, use '17' or higher.";
        var cliRules = "- If PostgreSQL server uses Azure AD authentication, use '--microsoft-entra-auth Enabled' when creating.\n- Azure CLI uses parameters '--name <server-name> --rule-name <rule-name>' for firewall rules creation.\n- IMPORTANT: **If using Azure AD authentication, you MUST ADD a database USER for the managed identity and GRANT all privileges to make the connection work.** Use 'az postgres flexible-server execute' command to run SQL commands to create the user and grant privileges.";
        return TemplateService.ProcessTemplate("IaCRules/postgresql-rules", new Dictionary<string, string> {
            { "VersionRules",  versionRules },
            { "DatabaseCommonRules", databaseCommonRules},
            { "ToolSpecificRules", GetToolSpecificResourceRules(parameters.IacType, null, null, cliRules)}
        });
    }

    private static string GenerateMySqlRules(IaCRulesTemplateParameters parameters)
    {
        var databaseCommonRules = TemplateService.LoadTemplate("IaCRules/database-common-rules");
        return TemplateService.ProcessTemplate("IaCRules/mysql-rules", new Dictionary<string, string> { { "DatabaseCommonRules", databaseCommonRules } });
    }

    private static string GenerateCosmosDbRules(IaCRulesTemplateParameters parameters)
    {
        return TemplateService.ProcessTemplate("IaCRules/cosmosdb-rules", new Dictionary<string, string> { { "ToolSpecificRules", GetToolSpecificResourceRules(parameters.IacType, null, null, null) } });
    }

    private static string GenerateStorageRules(IaCRulesTemplateParameters parameters)
    {
        var tfRules = "- Add `storage_use_azuread = true` in azurerm provider.";
        return TemplateService.ProcessTemplate("IaCRules/storage-rules", new Dictionary<string, string> { { "ToolSpecificRules", GetToolSpecificResourceRules(parameters.IacType, null, tfRules, null) } });
    }

    private static string GenerateKeyVaultRules(IaCRulesTemplateParameters parameters)
    {
        var bicepRules = "- Allow public access from all networks(set publicNetworkAccess = Enabled).";
        var tfRules = "- Assign role 'Key Vault Secrets Officer (b86a8fe4-44ce-4948-aee5-eccb2c155cd7)' to current user.This is the dependency for key vault secret creation.";
        var cliRules = "- IMPORTANT: Assign Key Vault Secrets Officer to current user. Add delay after RBAC role assignment to allow propagation before creating secrets.";

        return TemplateService.ProcessTemplate("IaCRules/key-vault-rules", new Dictionary<string, string> { { "ToolSpecificRules", GetToolSpecificResourceRules(parameters.IacType, bicepRules, tfRules, cliRules) } });
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
