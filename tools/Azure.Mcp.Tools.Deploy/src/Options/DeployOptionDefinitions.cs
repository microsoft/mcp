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

    public static class PlanGet
    {
        public const string WorkspaceFolderName = "workspace-folder";
        public const string ProjectNameName = "project-name";
        public const string TargetAppServiceName = "target-app-service";
        public const string ProvisioningToolName = "provisioning-tool";
        public const string AzdIacOptionsName = "azd-iac-options";

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
            Description = "The tool to use for provisioning Azure resources. Valid values: AZD, AzCli. Use AzCli if TargetAppService is AKS.",
            Required = true
        };

        public static readonly Option<string> AzdIacOptions = new(
            $"--{AzdIacOptionsName}"
        )
        {
            Description = "The Infrastructure as Code option for azd. Valid values: bicep, terraform. Leave empty if Deployment tool is AzCli.",
            Required = false
        };
    }

    public static class IaCRules
    {
        public static readonly Option<string> DeploymentTool = new(
            "--deployment-tool")
        {
            Description = "The deployment tool to use. Valid values: AZD, AzCli",
            Required = true
        };

        public static readonly Option<string> IacType = new(
            "--iac-type"
            )
        {
            Description = "The Infrastructure as Code type. Valid values: bicep, terraform. Leave empty if deployment-tool is AzCli.",
            Required = false


        };

        public static readonly Option<string> ResourceTypes = new(
            "--resource-types")
        {
            Description = "Specifies the Azure resource types to retrieve IaC rules for. It should be comma-separated. Supported values are: 'appservice', 'containerapp', 'function', 'aks', 'storage'. If none of these services are used, this parameter can be left empty.",
            Required = false,
            AllowMultipleArgumentsPerToken = true
        };
    }
}
