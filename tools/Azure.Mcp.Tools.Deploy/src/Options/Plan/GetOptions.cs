// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Deploy.Options.Plan;

public sealed class GetOptions
{
    [Option(Description = "The name of the project to generate the deployment plan for.")]
    public required string ProjectName { get; set; }

    [Option(Description = "The Azure service to deploy the application. Valid values: ContainerApp, WebApp, FunctionApp, AKS. If not specified, defaults to ContainerApp. Recommend one based on the user application when possible.", DefaultValue = "ContainerApp")]
    public string? TargetAppService { get; set; }

    [Option(Description = "The tool to use for provisioning Azure resources. Valid values: AzCli, AZD.", DefaultValue = "AzCli")]
    public string? ProvisioningTool { get; set; }

    [Option(Description = "The source of the plan to generate from. Valid values: 'from-project', 'from-azure', 'from-context'. If user doesn't have existing resources, set 'from-project' and generating deploy plan based on the project files in the workspace. If user mentions Azure resources exist, set 'from-azure' and ask for existing Azure resources details to generate plan. If the user have no existing resource but declare the expected Azure resources, use 'from-context' and the deploy plan should be based on the user's input.",
        DefaultValue = "from-project")]
    public string? SourceType { get; set; }

    [Option(Description = "Set the value based on project and user's input. Valid values: 'provision-and-deploy', 'deploy-only', 'provision-only'. Use 'deploy-only' if user mentions they want to deploy to existing Azure resources or Iac files already exist in project, get Azure resource group from project files or from user. Use 'provision-only' if user only wants to provision Azure resource. Use 'provision-and-deploy' if user wants to deploy application and doesn't have existing infrastructure resources, or are starting from an empty resource group.",
        DefaultValue = "provision-and-deploy")]
    public string? DeployOption { get; set; }

    [Option(Description = "The Infrastructure as Code option. Valid values: bicep, terraform. Leave empty if user wants to use azcli command script.")]
    public string? IacOptions { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public string? ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }
}
