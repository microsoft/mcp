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
}
