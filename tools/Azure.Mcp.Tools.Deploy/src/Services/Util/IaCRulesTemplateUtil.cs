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
    public static string GetIaCRules(string deploymentTool, string iacType, string[] resourceTypes,
        bool enablePublicNetworkAccess = false, bool allowAzureServices = false,
        bool allowPrivilegedRoles = false, bool useConnectionStrings = false)
    {
        if (allowAzureServices && !enablePublicNetworkAccess)
        {
            throw new ArgumentException("--allow-azure-services requires --enable-public-network-access.");
        }
        var parameters = CreateTemplateParameters(deploymentTool, iacType, resourceTypes);
        parameters.EnablePublicNetworkAccess = enablePublicNetworkAccess;
        parameters.AllowAzureServices = allowAzureServices;
        parameters.AllowPrivilegedRoles = allowPrivilegedRoles;
        parameters.UseConnectionStrings = useConnectionStrings;
        // Default values for optional parameters
        if (deploymentTool.Equals(DeploymentTool.Azd, StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(iacType))
        {
            iacType = "bicep";
            parameters.IacType = iacType;
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
        string[] resourceTypes) => new()
        {
            DeploymentTool = deploymentTool,
            IacType = iacType,
            ResourceTypes = resourceTypes,
            ResourceTypesDisplay = string.Join(", ", resourceTypes)
        };

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
        var normalizedIacType = (parameters.IacType ?? string.Empty).ToLowerInvariant();

        return normalizedIacType switch
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

        if (parameters.ResourceTypes.Contains(AzureServiceNames.AzureContainerApp, StringComparer.OrdinalIgnoreCase))
        {
            rules.Add(GenerateContainerAppRules(parameters));
        }

        if (parameters.ResourceTypes.Contains(AzureServiceNames.AzureAppService, StringComparer.OrdinalIgnoreCase))
        {
            rules.Add(GenerateAppServiceRules(parameters));
        }

        if (parameters.ResourceTypes.Contains(AzureServiceNames.AzureFunctionApp, StringComparer.OrdinalIgnoreCase))
        {
            rules.Add(GenerateFunctionAppRules(parameters));
        }

        if (parameters.ResourceTypes.Contains(AzureServiceNames.AzureKubernetesService, StringComparer.OrdinalIgnoreCase))
        {
            rules.Add(GenerateAKSRules(parameters));
        }

        if (parameters.ResourceTypes.Contains(AzureServiceNames.AzureDatabaseForPostgreSql, StringComparer.OrdinalIgnoreCase))
        {
            rules.Add(GeneratePostgreSqlRules(parameters));
        }

        if (parameters.ResourceTypes.Contains(AzureServiceNames.AzureDatabaseForMySql, StringComparer.OrdinalIgnoreCase))
        {
            rules.Add(GenerateMySqlRules(parameters));
        }

        if (parameters.ResourceTypes.Contains(AzureServiceNames.AzureCosmosDb, StringComparer.OrdinalIgnoreCase))
        {
            rules.Add(GenerateCosmosDbRules(parameters));
        }

        if (parameters.ResourceTypes.Contains(AzureServiceNames.AzureStorageAccount, StringComparer.OrdinalIgnoreCase))
        {
            rules.Add(GenerateStorageRules(parameters));
        }

        rules.Add(GenerateKeyVaultRules(parameters));

        return string.Join(Environment.NewLine, rules);
    }

    private static string GetToolSpecificResourceRules(string iacType, string? bicepRules, string? tfRules, string? cliRules)
    {
        var normalizedIacType = (iacType ?? string.Empty).ToLowerInvariant();
        return normalizedIacType switch
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
            { "AuthenticationRules", parameters.UseConnectionStrings
                ? "- Secret-based connection strings are explicitly permitted. Store credentials in Key Vault and consume them through secret references; never plaintext configuration."
                : "- Use Azure Workload Identity for AKS connections to Azure services. Enable the OIDC issuer, workload identity, and a dedicated Kubernetes service account with federated credentials and least-privilege data roles." },
            { "KeyvaultIntegrationRules", parameters.UseConnectionStrings
                ? TemplateService.LoadTemplate("IaCRules/aks-kv-integration-rules")
                : "- Access Key Vault using the workload identity and Key Vault Secrets User permissions. Do not share node identity credentials with application pods." }
        });
    }

    private static string GeneratePostgreSqlRules(IaCRulesTemplateParameters parameters)
    {
        var versionRules = parameters.IacType.Equals(IacType.Terraform, StringComparison.OrdinalIgnoreCase)
            ? "- PostgreSQL SKU name format: B_Standard_B1ms(Burstable tier), GP_Standard_D2s_v3(GeneralPurpose), MO_Standard_E4s_v3(MemoryOptimized)\n- For version, prefer to use '16'."
            : "For version, use '17' or higher.";
        var cliRules = parameters.AllowPrivilegedRoles
            ? "- Elevated database privileges are explicitly permitted. Create the managed identity database user and grant the requested privileges only within the application's database."
            : "- Create a database user for the managed identity. Grant only CONNECT, required schema USAGE, and the table operations required by the application; do not grant ALL privileges by default.";
        return TemplateService.ProcessTemplate("IaCRules/postgresql-rules", new Dictionary<string, string> {
            { "VersionRules",  versionRules },
            { "DatabaseCommonRules", GenerateDatabaseCommonRules(parameters)},
            { "ToolSpecificRules", GetToolSpecificResourceRules(parameters.IacType, null, null, cliRules)}
        });
    }

    private static string GenerateMySqlRules(IaCRulesTemplateParameters parameters) =>
        TemplateService.ProcessTemplate("IaCRules/mysql-rules", new Dictionary<string, string> { { "DatabaseCommonRules", GenerateDatabaseCommonRules(parameters) } });

    private static string GenerateDatabaseCommonRules(IaCRulesTemplateParameters parameters) =>
        TemplateService.ProcessTemplate("IaCRules/database-common-rules", new Dictionary<string, string> { { "NetworkRules", GenerateNetworkRules(parameters) } });

    private static string GenerateNetworkRules(IaCRulesTemplateParameters parameters) =>
        !parameters.EnablePublicNetworkAccess
            ? "- Disable public network access. Configure supported private endpoints or virtual network integration, private DNS, and connectivity for the application and deployment runner."
            : parameters.AllowAzureServices
                ? "- Public access and the all-Azure-services firewall exception (0.0.0.0) are explicitly permitted for databases. This includes other customers' subscriptions and is not tenant isolation."
                : "- Public network access is explicitly permitted. Restrict firewall access to explicitly approved client IPs or subnets; do not add an all-Azure-services exception.";

    private static string GenerateCosmosDbRules(IaCRulesTemplateParameters parameters) =>
        TemplateService.ProcessTemplate("IaCRules/cosmosdb-rules", new Dictionary<string, string> { { "NetworkRules", GenerateNetworkRules(parameters) } });

    private static string GenerateStorageRules(IaCRulesTemplateParameters parameters)
    {
        var tfRules = "- Add `storage_use_azuread = true` in azurerm provider.";
        return TemplateService.ProcessTemplate("IaCRules/storage-rules", new Dictionary<string, string> {
            { "ToolSpecificRules", GetToolSpecificResourceRules(parameters.IacType, null, tfRules, null) },
            { "NetworkRules", parameters.EnablePublicNetworkAccess
                ? "- Public network access is explicitly permitted; restrict access to approved networks."
                : "- Disable public network access and configure private endpoints and private DNS." },
            { "AuthenticationRules", parameters.UseConnectionStrings
                ? "- Key authentication is explicitly permitted when required for secret-based connections. Store keys in Key Vault."
                : "- Disable storage account local auth (key access); use Microsoft Entra ID." }
        });
    }

    private static string GenerateKeyVaultRules(IaCRulesTemplateParameters parameters)
    {
        var bicepRules = string.Empty;
        var tfRules = "- Assign role 'Key Vault Secrets Officer (b86a8fe4-44ce-4948-aee5-eccb2c155cd7)' to current user.This is the dependency for key vault secret creation.";
        var cliRules = "- IMPORTANT: Assign Key Vault Secrets Officer to current user. Add delay after RBAC role assignment to allow propagation before creating secrets.";

        return TemplateService.ProcessTemplate("IaCRules/key-vault-rules", new Dictionary<string, string> {
            { "ToolSpecificRules", GetToolSpecificResourceRules(parameters.IacType, bicepRules, tfRules, cliRules) },
            { "RuntimeRole", parameters.AllowPrivilegedRoles ? "Key Vault Secrets Officer" : "Key Vault Secrets User" },
            { "NetworkRules", parameters.EnablePublicNetworkAccess
                ? "- Public network access is explicitly permitted for Key Vault; restrict its firewall to approved networks."
                : "- Set publicNetworkAccess to Disabled for Key Vault. Configure private endpoints and private DNS for both the application and deployment runner." }
        });
    }

    /// <summary>
    /// Generates final instructions for the IaC rules.
    /// </summary>
    private static string GenerateFinalInstructions(IaCRulesTemplateParameters parameters) =>
        TemplateService.ProcessTemplate("IaCRules/final-instructions", parameters.ToDictionary());

    /// <summary>
    /// Builds the required tools list based on deployment tool and resource types.
    /// </summary>
    private static string BuildRequiredTools(string deploymentTool, string[] resourceTypes)
    {
        var tools = new List<string> { "az cli (az --version)" };

        if (string.Equals(deploymentTool, DeploymentTool.Azd, StringComparison.OrdinalIgnoreCase))
        {
            tools.Add("azd (azd version)");
        }

        if (resourceTypes.Contains(AzureServiceNames.AzureContainerApp, StringComparer.OrdinalIgnoreCase))
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
        if (string.Equals(iacType, IacType.Terraform, StringComparison.OrdinalIgnoreCase) && string.Equals(deploymentTool, DeploymentTool.Azd, StringComparison.OrdinalIgnoreCase))
        {
            return "Note: Do not use Terraform CLI.";
        }

        return string.Empty;
    }
}
