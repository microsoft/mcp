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

    public class PipelineGenerateOptions : SubscriptionOptions
    {
        public const string OrganizationNameName = "organization-name";
        public const string RepositoryNameName = "repository-name";
        public const string GithubEnvironmentNameName = "github-environment-name";

        public static readonly Option<string> OrganizationName = new(
            $"--{OrganizationNameName}"
        )
        {
            Description = "The name of the organization or the user account name of the current Github repository. DO NOT fill this in if you're not sure.",
            Required = false
        };

        public static readonly Option<string> RepositoryName = new(
            $"--{RepositoryNameName}"
        )
        {
            Description = "The name of the current Github repository. DO NOT fill this in if you're not sure.",
            Required = false
        };

        public static readonly Option<string> GithubEnvironmentName = new(
            $"--{GithubEnvironmentNameName}"
        )
        {
            Description = "The name of the environment to which the deployment pipeline will be deployed. DO NOT fill this in if you're not sure.",
            Required = false
        };

    }

    public static class PlanGet
    {
        public const string WorkspaceFolderName = "workspace-folder";
        public const string ProjectNameName = "project-name";
        public const string TargetAppServiceName = "target-app-service";

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
    }

    public static class IaCRules
    {

        public static readonly Option<string> ResourceTypes = new(
            "--resource-types")
        {
            Description = "Specifies the Azure resource types to retrieve IaC rules for. It should be comma-separated. Supported values are: 'appservice', 'containerapp', 'function', 'aks', 'storage'. If none of these services are used, this parameter can be left empty.",
            Required = false,
            AllowMultipleArgumentsPerToken = true
        };
    }
}
