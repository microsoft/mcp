// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Deploy.Models.Templates;

/// <summary>
/// Parameters for generating deployment plan templates.
/// </summary>
public sealed class DeploymentPlanTemplateParameters
{
    /// <summary>
    /// The title of the deployment plan.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// The name of the project being deployed.
    /// </summary>
    public string ProjectName { get; set; } = string.Empty;

    /// <summary>
    /// The target Azure service (ContainerApp, WebApp, FunctionApp, AKS).
    /// </summary>
    public string TargetAppService { get; set; } = string.Empty;

    /// <summary>
    /// The provisioning tool (AZD, AzCli).
    /// </summary>
    public string ProvisioningTool { get; set; } = string.Empty;

    /// <summary>
    /// The Infrastructure as Code type (bicep, terraform).
    /// </summary>
    public string IacType { get; set; } = string.Empty;

    /// <summary>
    /// The Azure compute host display name.
    /// </summary>
    public string AzureComputeHost { get; set; } = string.Empty;

    /// <summary>
    /// The execution steps for the deployment.
    /// </summary>
    public string ExecutionSteps { get; set; } = string.Empty;


    /// <summary>
    /// The source of the plan to generate from.
    /// </summary>
    public string SourceType { get; set; } = string.Empty;

    /// <summary>
    /// The deploy option selected.
    /// </summary>
    public string DeployOption { get; set; } = string.Empty;

    /// <summary>
    /// The deployment goal of the plan
    /// </summary>
    public string Goal { get; set; } = string.Empty;

    /// <summary>
    /// The recommended resources or the existing(missing) resources
    /// </summary>
    public string ResourceInfo { get; set; } = string.Empty;

    /// <summary>
    /// The sample mermaid diagram for the deployment plan.
    /// </summary>
    public string SampleMermaid { get; set; } = string.Empty;

    /// <summary>
    /// Converts the parameters to a dictionary for template processing.
    /// </summary>
    /// <returns>A dictionary with parameter names as keys and their values.</returns>
    public Dictionary<string, string> ToDictionary()
    {
        return new Dictionary<string, string>
        {
            { nameof(Title), Title },
            { nameof(ProjectName), ProjectName },
            { nameof(TargetAppService), TargetAppService },
            { nameof(ProvisioningTool), ProvisioningTool },
            { nameof(IacType), IacType },
            { nameof(AzureComputeHost), AzureComputeHost },
            { nameof(ExecutionSteps), ExecutionSteps },
            { nameof(SourceType), SourceType },
            { nameof(DeployOption), DeployOption },
            { nameof(Goal), Goal },
            { nameof(ResourceInfo), ResourceInfo },
            { nameof(SampleMermaid), SampleMermaid }
        };
    }
}
