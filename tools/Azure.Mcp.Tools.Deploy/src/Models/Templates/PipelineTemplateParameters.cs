// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Deploy.Models.Templates;

/// <summary>
/// Parameters for generating pipeline templates.
/// </summary>
public sealed class PipelineTemplateParameters
{
    public string DeploymentTool { get; set; } = string.Empty;

    public string PipelinePlatform { get; set; } = string.Empty;

    public string PrerequisiteChecksPrompt { get; set; } = string.Empty;

    public string PipelineFilePrompt { get; set; } = string.Empty;

    public string SetupMethodPrompt { get; set; } = string.Empty;

    public string AzureAuthConfigPrompt { get; set; } = string.Empty;

    public string EnvironmentSetupPrompt { get; set; } = string.Empty;

    /// <summary>
    /// Converts the parameters to a dictionary for template processing.
    /// </summary>
    /// <returns>A dictionary containing the parameter values.</returns>
    public Dictionary<string, string> ToDictionary()
    {
        return new Dictionary<string, string>
        {
            { nameof(DeploymentTool), DeploymentTool },
            { nameof(PipelinePlatform), PipelinePlatform },
            { nameof(PrerequisiteChecksPrompt), PrerequisiteChecksPrompt },
            { nameof(PipelineFilePrompt), PipelineFilePrompt },
            { nameof(SetupMethodPrompt), SetupMethodPrompt },
            { nameof(AzureAuthConfigPrompt), AzureAuthConfigPrompt },
            { nameof(EnvironmentSetupPrompt), EnvironmentSetupPrompt }
        };
    }
}
