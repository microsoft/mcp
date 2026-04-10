// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Deploy.Models.Templates;

/// <summary>
/// Parameters for IaC rules template generation.
/// </summary>
public sealed class IaCRulesTemplateParameters
{
    public string DeploymentTool { get; set; } = string.Empty;
    public string IacType { get; set; } = string.Empty;
    public string[] ResourceTypes { get; set; } = [];
    public string ResourceTypesDisplay { get; set; } = string.Empty;
    public string DeploymentToolRules { get; set; } = string.Empty;
    public string IacTypeRules { get; set; } = string.Empty;
    public string ResourceSpecificRules { get; set; } = string.Empty;
    public string FinalInstructions { get; set; } = string.Empty;
    public string RequiredTools { get; set; } = string.Empty;
    public string AdditionalNotes { get; set; } = string.Empty;

    /// <summary>
    /// Converts the parameters to a dictionary for template processing.
    /// </summary>
    /// <returns>A dictionary with parameter names as keys and their values.</returns>
    public Dictionary<string, string> ToDictionary()
    {
        return new Dictionary<string, string>
        {
            { nameof(DeploymentTool), DeploymentTool },
            { nameof(IacType), IacType },
            { nameof(ResourceTypesDisplay), ResourceTypesDisplay },
            { nameof(DeploymentToolRules), DeploymentToolRules },
            { nameof(IacTypeRules), IacTypeRules },
            { nameof(ResourceSpecificRules), ResourceSpecificRules },
            { nameof(FinalInstructions), FinalInstructions },
            { nameof(RequiredTools), RequiredTools },
            { nameof(AdditionalNotes), AdditionalNotes }
        };
    }
}
