// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Deploy.Options.Infrastructure;

public sealed class RulesGetOptions
{
    [Option(Description = "The deployment tool to use. Valid values: AzCli, AZD")]
    public required string DeploymentTool { get; set; }

    [Option(Description = "The type of IaC file used for deployment. Valid values: bicep, terraform. Leave empty ONLY if user wants to use AzCli command script and no IaC file.")]
    public string? IacType { get; set; }

    [Option(Description = "Comma-separated list of Azure resource types to generate rules for. Get the value from context and use the same resources defined in plan. Valid value: 'appservice', 'containerapp', 'function', 'aks', 'azuredatabaseforpostgresql', 'azuredatabaseformysql', 'azuresqldatabase', 'azurecosmosdb', 'azurestorageaccount', 'azurekeyvault'")]
    public string? ResourceTypes { get; set; }

    [Option(Description = "Generate guidance permitting public network access (less secure). Defaults to false; private networking is recommended otherwise.")]
    public bool EnablePublicNetworkAccess { get; set; }

    [Option(Description = "Permit database firewall access from all Azure services, including other customers (insecure). Requires --enable-public-network-access. Defaults to false.")]
    public bool AllowAzureServices { get; set; }

    [Option(Description = "Recommend elevated application permissions such as Key Vault Secrets Officer (broad access). Defaults to false; least-privilege runtime roles are used otherwise.")]
    public bool AllowPrivilegedRoles { get; set; }

    [Option(Description = "Use secret-based connection strings instead of workload or managed identity where needed (less secure). Defaults to false; secrets must still be stored securely.")]
    public bool UseConnectionStrings { get; set; }
}
