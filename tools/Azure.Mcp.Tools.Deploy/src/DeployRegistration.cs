// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Azure.Mcp.Tools.Deploy.Commands.App;
using Azure.Mcp.Tools.Deploy.Commands.Architecture;
using Azure.Mcp.Tools.Deploy.Commands.Infrastructure;
using Azure.Mcp.Tools.Deploy.Commands.Pipeline;
using Azure.Mcp.Tools.Deploy.Commands.Plan;
using Azure.Mcp.Tools.Deploy.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Commands.Descriptors;

namespace Azure.Mcp.Tools.Deploy;

public sealed class DeployRegistration : IAreaRegistration
{
    public static string AreaName => "deploy";

    public static string AreaTitle => "Azure Deployment";

    public static CommandCategory Category => CommandCategory.AzureServices;

    public static CommandGroupDescriptor GetCommandDescriptors() => new()
    {
        Name = AreaName,
        Description = "Deploy operations – Commands for deploying applications to Azure. Provides sub-commands to generate deployment plans, offer infrastructure-as-code (Bicep/Terraform) guidance, fetch application logs, generate CI/CD pipeline guidance, and produce Azure architecture diagrams based on application topology.",
        Title = AreaTitle,
        SubGroups =
        [
            new CommandGroupDescriptor
            {
                Name = "app",
                Description = "Application-specific deployment tools",
                SubGroups =
                [
                    new CommandGroupDescriptor
                    {
                        Name = "logs",
                        Description = "Application logs management",
                        Commands =
                        [
                            new CommandDescriptor
                            {
                                Id = "ce9d648d-7c76-48a0-8cba-b9b57c6fd00b",
                                Name = "get",
                                Description = "Shows application logs specifically for Azure Developer CLI (azd) deployed applications from their associated Log Analytics workspace for Container Apps, App Services, and Function Apps. Designed exclusively for applications deployed via 'azd up' command and automatically discovers the correct workspace and resources based on the azd environment configuration. Use this tool to check deployment status or troubleshoot post-deployment issues.",
                                Title = "Get",
                                Annotations = new ToolAnnotations
                                {
                                    Destructive = false,
                                    Idempotent = true,
                                    OpenWorld = false,
                                    ReadOnly = true,
                                    LocalRequired = false,
                                    Secret = false,
                                },
                                Options =
                                [
                                    new OptionDescriptor
                                    {
                                        Name = "workspace-folder",
                                        Description = "The full path of the workspace folder.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "azd-env-name",
                                        Description = "The name of the environment created by azd (AZURE_ENV_NAME) during `azd init` or `azd up`. If not provided in context, try to find it in the .azure directory in the workspace or use 'azd env list'.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "limit",
                                        Description = "The maximum row number of logs to retrieve. Use this to get a specific number of logs or to avoid the retrieved logs from reaching token limit. Default is 200.",
                                        TypeName = "string",
                                    },
                                ],
                                Kind = CommandKind.Subscription,
                                HandlerType = nameof(LogsGetCommand)
                            },
                        ],
                    },
                ],
            },
            new CommandGroupDescriptor
            {
                Name = "architecture",
                Description = "Architecture diagram operations",
                SubGroups =
                [
                    new CommandGroupDescriptor
                    {
                        Name = "diagram",
                        Description = "Architecture diagram generation",
                        Commands =
                        [
                            new CommandDescriptor
                            {
                                Id = "34d7ec6a-e229-4775-8af3-85f81ae3e6d3",
                                Name = "generate",
                                Description = "Generates an Azure service architecture diagram showing the recommended Azure services and their connections for an application. Use this tool when the user asks to generate, create, or visualize an Azure architecture diagram for their application, or wants to see which Azure services to use. Renders the diagram from an application topology (AppTopology) provided as input; scan the workspace first to build this topology by detecting services, frameworks, and environment variables for connection strings, and for .NET Aspire applications, check aspireManifest.json. Do not use this tool when the user needs a detailed network topology or security design.",
                                Title = "Generate",
                                Annotations = new ToolAnnotations
                                {
                                    Destructive = false,
                                    Idempotent = true,
                                    OpenWorld = false,
                                    ReadOnly = true,
                                    LocalRequired = false,
                                    Secret = false,
                                },
                                Options =
                                [
                                    new OptionDescriptor
                                    {
                                        Name = "raw-mcp-tool-input",
                                        Description = "{ \"type\": \"object\", \"properties\": { \"workspaceFolder\": { \"type\": \"string\", \"description\": \"The full path of the workspace folder.\" }, \"projectName\": { \"type\": \"string\", \"description\": \"The name of the project. This is used to generate the resource names.\" }, \"services\": { \"type\": \"array\", \"description\": \"An array of service parameters.\", \"items\": { \"type\": \"object\", \"properties\": { \"name\": { \"type\": \"string\", \"description\": \"The name of the service.\" }, \"path\": { \"type\": \"string\", \"description\": \"The relative path of the service main project folder\" }, \"language\": { \"type\": \"string\", \"description\": \"The programming language of the service.\" }, \"port\": { \"type\": \"string\", \"description\": \"The port number the service uses. Get this from Dockerfile for container apps. If not available, default to '80'.\" }, \"azureComputeHost\": { \"type\": \"string\", \"description\": \"The appropriate azure service that should be used to host this service. Use containerapp if the service is containerized and has a Dockerfile.\", \"enum\": [ \"appservice\", \"containerapp\", \"function\", \"staticwebapp\", \"aks\" ] }, \"dockerSettings\": { \"type\": \"object\", \"description\": \"Docker settings for the service. This is only needed if the service's azureComputeHost is containerapp.\", \"properties\": { \"dockerFilePath\": { \"type\": \"string\", \"description\": \"The absolute path to the Dockerfile for the service. If the service's azureComputeHost is not containerapp, leave blank.\" }, \"dockerContext\": { \"type\": \"string\", \"description\": \"The absolute path to the Docker build context for the service. If the service's azureComputeHost is not containerapp, leave blank.\" } }, \"required\": [ \"dockerFilePath\", \"dockerContext\" ] }, \"dependencies\": { \"type\": \"array\", \"description\": \"An array of dependent services. A compute service may have a dependency on another compute service.\", \"items\": { \"type\": \"object\", \"properties\": { \"name\": { \"type\": \"string\", \"description\": \"The name of the dependent service. Can be arbitrary, or must reference another service in the services array if referencing appservice, containerapp, staticwebapps, aks, or functionapp.\" }, \"serviceType\": { \"type\": \"string\", \"description\": \"The name of the azure service that can be used for this dependent service.\", \"enum\": [ \"azureaisearch\", \"azureaiservices\", \"appservice\", \"azureapplicationinsights\", \"azurebotservice\", \"containerapp\", \"azurecosmosdb\", \"functionapp\", \"azurekeyvault\", \"aks\", \"azuredatabaseformysql\", \"azureopenai\", \"azuredatabaseforpostgresql\", \"azureprivateendpoint\", \"azurecacheforredis\", \"azuresqldatabase\", \"azurestorageaccount\", \"staticwebapp\", \"azureservicebus\", \"azuresignalrservice\", \"azurevirtualnetwork\", \"azurewebpubsub\" ] }, \"connectionType\": { \"type\": \"string\", \"description\": \"The connection authentication type of the dependency.\", \"enum\": [ \"http\", \"secret\", \"system-identity\", \"user-identity\", \"bot-connection\" ] }, \"environmentVariables\": { \"type\": \"array\", \"description\": \"An array of environment variables defined in source code to set up the connection.\", \"items\": { \"type\": \"string\" } } }, \"required\": [ \"name\", \"serviceType\", \"connectionType\", \"environmentVariables\" ] } }, \"settings\": { \"type\": \"array\", \"description\": \"An array of environment variables needed to run this service. Please search the entire codebase to find environment variables.\", \"items\": { \"type\": \"string\" } } }, \"required\": [ \"name\", \"path\", \"azureComputeHost\", \"language\", \"port\", \"dependencies\", \"settings\" ] } } }, \"required\": [ \"workspaceFolder\", \"services\" ] }",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                ],
                                Kind = CommandKind.Basic,
                                HandlerType = nameof(DiagramGenerateCommand)
                            },
                        ],
                    },
                ],
            },
            new CommandGroupDescriptor
            {
                Name = "iac",
                Description = "Infrastructure as Code operations",
                SubGroups =
                [
                    new CommandGroupDescriptor
                    {
                        Name = "rules",
                        Description = "Infrastructure as Code rules and guidelines",
                        Commands =
                        [
                            new CommandDescriptor
                            {
                                Id = "942b5c00-01dd-4ca0-9596-4cf650ff7934",
                                Name = "get",
                                Description = "Retrieves rules and best practices for creating Bicep and Terraform Infrastructure as Code (IaC) files to deploy Azure applications. Use this tool when the user asks for rules, guidelines, or best practices for writing Bicep scripts or Terraform templates for Azure resources. The rules cover Azure resource configuration standards, compatibility with Azure Developer CLI (azd) and Azure CLI, and general IaC quality requirements. Use when user asks: show me the rules and best practices for writing Bicep and Terraform IaC for Azure.",
                                Title = "Get",
                                Annotations = new ToolAnnotations
                                {
                                    Destructive = false,
                                    Idempotent = true,
                                    OpenWorld = false,
                                    ReadOnly = true,
                                    LocalRequired = false,
                                    Secret = false,
                                },
                                Options =
                                [
                                    new OptionDescriptor
                                    {
                                        Name = "deployment-tool",
                                        Description = "The deployment tool to use. Valid values: AzCli, AZD",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "iac-type",
                                        Description = "The type of IaC file used for deployment. Valid values: bicep, terraform. Leave empty ONLY if user wants to use AzCli command script and no IaC file.",
                                        TypeName = "string",
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "resource-types",
                                        Description = "List of Azure resource types to generate rules for. Get the value from context and use the same resources defined in plan. Valid value: 'appservice','containerapp','function','aks','azuredatabaseforpostgresql','azuredatabaseformysql','azuresqldatabase','azurecosmosdb','azurestorageaccount','azurekeyvault'",
                                        TypeName = "string",
                                    },
                                ],
                                Kind = CommandKind.Subscription,
                                HandlerType = nameof(LogsGetCommand)
                            },
                        ],
                    },
                ],
            },
            new CommandGroupDescriptor
            {
                Name = "pipeline",
                Description = "CI/CD pipeline operations",
                SubGroups =
                [
                    new CommandGroupDescriptor
                    {
                        Name = "guidance",
                        Description = "CI/CD pipeline guidance",
                        Commands =
                        [
                            new CommandDescriptor
                            {
                                Id = "8aec84f9-e884-4119-a386-53b7cfbe9e00",
                                Name = "get",
                                Description = "Generates CI/CD pipeline configuration and step-by-step guidance for deploying an application to Azure using GitHub Actions or Azure DevOps pipelines. Use this tool when the user wants to create a CI/CD pipeline, set up automated deployment workflows, or configure pipeline files to deploy their application to Azure. Supports both Azure Developer CLI (azd) and Azure CLI based deployments, and can generate pipelines that provision infrastructure and deploy application code. Before calling this tool, confirm with the user whether they prefer GitHub Actions or Azure DevOps, and whether they have existing Azure resources for their deployment environments. Use when user asks: how do I set up a CI/CD pipeline with GitHub Actions or Azure DevOps to deploy my app to Azure?",
                                Title = "Get",
                                Annotations = new ToolAnnotations
                                {
                                    Destructive = false,
                                    Idempotent = true,
                                    OpenWorld = false,
                                    ReadOnly = true,
                                    LocalRequired = false,
                                    Secret = false,
                                },
                                Options =
                                [
                                    new OptionDescriptor
                                    {
                                        Name = "is-azd-project",
                                        Description = "Whether to use azd tool in the deployment pipeline. Set to true ONLY if azure.yaml is provided or the context suggests AZD tools.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "pipeline-platform",
                                        Description = "The platform for the deployment pipeline. Valid values: github-actions, azure-devops.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "deploy-option",
                                        Description = "Valid values: deploy-only, provision-and-deploy. Default to deploy-only. Set to 'provision-and-deploy' ONLY WHEN user explicitly wants infra provisioning pipeline using local provisioning scripts.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                ],
                                Kind = CommandKind.Subscription,
                                HandlerType = nameof(LogsGetCommand)
                            },
                        ],
                    },
                ],
            },
            new CommandGroupDescriptor
            {
                Name = "plan",
                Description = "Deployment planning operations",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "92ca95b2-cde6-407c-ac67-9743db40dfc4",
                        Name = "get",
                        Description = "Creates a deployment plan for deploying an application to Azure using the options provided by the caller. Use this tool when the user wants a formatted, step-by-step deployment plan (including suggested Azure resources, infrastructure as code (IaC) templates, and deployment instructions) based on a target Azure hosting service (for example, Container Apps, App Service, or AKS) and a chosen provisioning tool (such as Azure Developer CLI (azd) or Azure CLI with Bicep or Terraform). This command does not scan the workspace or automatically recommend Azure services. Instead, the caller or agent must first analyze the workspace, determine the services, frameworks, and dependencies to deploy, select the appropriate Azure hosting service, provisioning tool, IaC type, and deployment option, and then pass those chosen values into this tool to generate the deployment plan.",
                        Title = "Get",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = false,
                            Idempotent = true,
                            OpenWorld = false,
                            ReadOnly = true,
                            LocalRequired = false,
                            Secret = false,
                        },
                        Options =
                        [
                            new OptionDescriptor
                            {
                                Name = "workspace-folder",
                                Description = "The full path of the workspace folder.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "project-name",
                                Description = "The name of the project to generate the deployment plan for. If not provided, will be inferred from the workspace.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "target-app-service",
                                Description = "The Azure service to deploy the application. Valid values: ContainerApp, WebApp, FunctionApp, AKS. Recommend one based on user application.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "provisioning-tool",
                                Description = "The tool to use for provisioning Azure resources. Valid values: AzCli, AZD.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "iac-options",
                                Description = "The Infrastructure as Code option. Valid values: bicep, terraform. Leave empty if user wants to use azcli command script.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "source-type",
                                Description = "The source of the plan to generate from. Valid values: 'from-project', 'from-azure', 'from-context'. If user doesn't have existing resources, set 'from-project' and generating deploy plan based on the project files in the workspace. If user mentions Azure resources exist, set 'from-azure' and ask for existing Azure resources details to generate plan. If the user have no existing resource but declare the expected Azure resources, use 'from-context' and the deploy plan should be based on the user's input.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "deploy-option",
                                Description = "Set the value based on project and user's input. Valid values: 'provision-and-deploy', 'deploy-only', 'provision-only'. Use 'deploy-only' if user mentions they want to deploy to existing Azure resources or Iac files already exist in project, get Azure resource group from project files or from user. Use 'provision-only' if user only wants to provision Azure resource. Use 'provision-and-deploy' if user wants to deploy application and doesn't have existing infrastructure resources, or are starting from an empty resource group.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "resource-group",
                                Description = "The name of the Azure resource group. This is a logical container for Azure resources.",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(LogsGetCommand)
                    },
                ],
            },
        ],
    };

    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IDeployService, DeployService>();
        services.AddSingleton<LogsGetCommand>();
        services.AddSingleton<RulesGetCommand>();
        services.AddSingleton<GuidanceGetCommand>();
        services.AddSingleton<GetCommand>();
        services.AddSingleton<DiagramGenerateCommand>();
    }

    public static IBaseCommand ResolveHandler(string handlerTypeName, IServiceProvider serviceProvider) =>
        handlerTypeName switch
        {
            nameof(LogsGetCommand) => serviceProvider.GetRequiredService<LogsGetCommand>(),
            nameof(DiagramGenerateCommand) => serviceProvider.GetRequiredService<DiagramGenerateCommand>(),
            _ => throw new InvalidOperationException($"Unknown handler type '{{handlerTypeName}}' in deploy area.")
        };
}
