// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas.Server.Commands.ToolLoading;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Deploy.Services.Util;

namespace Azure.Mcp.Tools.Deploy.Options;

public static class DeployOptionDefinitions
{
    public static class RawMcpToolInput
    {
        public const string RawMcpToolInputName = CommandFactoryToolLoader.RawMcpToolInputOptionName;

        public static readonly Option<string> RawMcpToolInputOption = new(
            $"--{RawMcpToolInputName}"
        )
        {
            Description = JsonSchemaLoader.LoadAppTopologyJsonSchema(),
            Required = true
        };
    }

    public class AzdAppLogOptions : SubscriptionOptions
    {
        public const string WorkspaceFolderName = "workspace-folder";
        public const string AzdEnvNameName = "azd-env-name";
        public const string LimitName = "limit";

        public static readonly Option<string> WorkspaceFolder = new(
            $"--{WorkspaceFolderName}"
        )
        {
            Description = "The full path of the workspace folder.",
            Required = true
        };

        public static readonly Option<string> AzdEnvName = new(
            $"--{AzdEnvNameName}"
        )
        {
            Description = "The name of the environment created by azd (AZURE_ENV_NAME) during `azd init` or `azd up`. If not provided in context, try to find it in the .azure directory in the workspace or use 'azd env list'.",
            Required = true
        };

        public static readonly Option<int> Limit = new(
            $"--{LimitName}"
        )
        {
            Description = "The maximum row number of logs to retrieve. Use this to get a specific number of logs or to avoid the retrieved logs from reaching token limit. Default is 200.",
            DefaultValueFactory = _ => 200,
            Required = false
        };
    }

    public class PipelineGenerateOptions : SubscriptionOptions
    {
        public const string isAZDProjectName = "is-azd-project";
        public const string PipelinePlatformName = "pipeline-platform";

        public const string DeployOptionName = "deploy-option";

        public static readonly Option<bool> isAZDProject = new(
            $"--{isAZDProjectName}"
        )
        {
            Description = "Whether to use azd tool in the deployment pipeline. Set to true ONLY if azure.yaml is provided or the context suggests AZD tools.",
            DefaultValueFactory = _ => false,
            Required = true
        };

        public static readonly Option<string> pipelinePlatform = new(
            $"--{PipelinePlatformName}"
        )
        {
            Description = "The platform for the deployment pipeline. Valid values: github-actions, azure-devops.",
            DefaultValueFactory = _ => "github-actions",
            Required = true,
        };

        public static readonly Option<string> deployOption = new(
            $"--{DeployOptionName}"
        )
        {
            Description = "Valid values: deploy-only, provision-and-deploy. Default to deploy-only. Set to 'provision-and-deploy' ONLY WHEN user explicitly wants infra provisioning pipeline using local provisioning scripts.",
            DefaultValueFactory = _ => "deploy-only",
            Required = true
        };
    }

    public class PlanGet : SubscriptionOptions
    {
        public const string WorkspaceFolderName = "workspace-folder";
        public const string ProjectNameName = "project-name";
        public const string TargetAppServiceName = "target-app-service";
        public const string ProvisioningToolName = "provisioning-tool";
        public const string IacOptionsName = "iac-options";
        public const string SourceTypeName = "source-type";
        public const string DeployOptionName = "deploy-option";
        public const string ResourceGroupNameName = "resource-group-name";

        public static readonly Option<string> WorkspaceFolder = new(
            $"--{WorkspaceFolderName}"
        )
        {
            Description = "The full path of the workspace folder.",
            Required = true
        };

        public static readonly Option<string> ProjectName = new(
            $"--{ProjectNameName}"
        )
        {
            Description = "The name of the project to generate the deployment plan for. If not provided, will be inferred from the workspace.",
            Required = true
        };

        public static readonly Option<string> TargetAppService = new(
            $"--{TargetAppServiceName}"
        )
        {
            Description = "The Azure service to deploy the application. Valid values: ContainerApp, WebApp, FunctionApp, AKS. Recommend one based on user application.",
            Required = true
        };

        public static readonly Option<string> ProvisioningTool = new(
            $"--{ProvisioningToolName}"
        )
        {
            Description = "The tool to use for provisioning Azure resources. Valid values: AzCli, AZD.",
            Required = true,
            DefaultValueFactory = _ => "AzCli"
        };

        public static readonly Option<string> IacOptions = new(
            $"--{IacOptionsName}"
        )
        {
            Description = "The Infrastructure as Code option. Valid values: bicep, terraform. Leave empty if user wants to use azcli command script.",
            Required = false
        };

        public static readonly Option<string> SourceType = new(
            $"--{SourceTypeName}"
        )
        {
            Description = "The source of the plan to generate from. Valid values: 'from-project', 'from-azure', 'from-context'. If user doesn't have existing resources, set 'from-project' and generating deploy plan based on the project files in the workspace. If user mentions Azure resources exist, set 'from-azure' and ask for existing Azure resources details to generate plan. If the user have no existing resource but declare the expected Azure resources, use 'from-context' and the deploy plan should be based on the user's input.",
            DefaultValueFactory = _ => "from-project",
            Required = true
        };

        public static readonly Option<string> DeployOption = new(
            $"--{DeployOptionName}"
        )
        {
            Description = "Set the value based on project and user's input. Valid values: 'provision-and-deploy', 'deploy-only', 'provision-only'. Use 'deploy-only' if user mentions they want to deploy to existing Azure resources or Iac files already exist in project, get Azure resource group from project files or from user. Use 'provision-only' if user only wants to provision Azure resource. Use 'provision-and-deploy' if user wants to deploy application and doesn't have existing infrastructure resources, or are starting from an empty resource group.",
            DefaultValueFactory = _ => "provision-and-deploy",
            Required = true
        };

        public static readonly Option<string> ResourceGroupName = new(
            $"--{ResourceGroupNameName}"
        )
        {
            Description = "The Azure resource group name to use for the deployment. If user has existing resources, agent must use existing resource's resource group, ask for user's confirmation before using its resource group name. Don't use a placeholder or mock a value.",
            Required = false
        };
    }

    public static class IaCRules
    {
        public static readonly Option<string> DeploymentTool = new(
            "--deployment-tool")
        {
            Description = "The deployment tool to use. Valid values: AzCli, AZD",
            Required = true
        };

        public static readonly Option<string> IacType = new(
            "--iac-type"
            )
        {
            Description = "The type of IaC file used for deployment. Valid values: bicep, terraform. Leave empty ONLY if user wants to use AzCli command script and no IaC file.",
            Required = false
        };

        public static readonly Option<string> ResourceTypes = new(
            "--resource-types")
        {
            Description = "List of Azure resource types to generate rules for. Get the value from context and use the same resources defined in plan. Valid value: 'appservice','containerapp','function','aks','azuredatabaseforpostgresql','azuredatabaseformysql','azuresqldatabase','azurecosmosdb','azurestorageaccount','azurekeyvault'",
            Required = false,
            AllowMultipleArgumentsPerToken = true
        };
    }
}
