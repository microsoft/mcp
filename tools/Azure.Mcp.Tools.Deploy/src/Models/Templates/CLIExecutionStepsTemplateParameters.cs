// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Deploy.Models.Templates;

/// <summary>
/// Parameters for generating deployment plan templates.
/// </summary>
public sealed class CLIExecutionStepsTemplateParameters
{
    /// <summary>
    /// The azure compute host display name.
    /// </summary>
    public string AzureComputeHost { get; set; } = string.Empty;

    /// <summary>
    /// The azure cli command group title
    /// </summary>
    public string TargetAppCommandTitle { get; set; } = string.Empty;

    /// <summary>
    /// The deployment steps
    /// </summary>
    public string DeploymentSteps { get; set; } = string.Empty;

    /// <summary>
    /// The IaC type
    /// </summary>
    public string IaCType { get; set; } = string.Empty;

    /// <summary>
    /// The step to check ACR dependencies
    /// </summary>
    public string ACRDependencyCheck { get; set; } = string.Empty;


    /// <summary>
    /// Converts the parameters to a dictionary for template processing.
    /// </summary>
    /// <returns>A dictionary with parameter names as keys and their values.</returns>
    public Dictionary<string, string> ToDictionary()
    {
        return new Dictionary<string, string>
        {
            { nameof(IaCType), IaCType },
            { nameof(AzureComputeHost), AzureComputeHost },
            { nameof(TargetAppCommandTitle), TargetAppCommandTitle },
            { nameof(DeploymentSteps), DeploymentSteps },
            { nameof(ACRDependencyCheck), ACRDependencyCheck }
        };
    }
}
