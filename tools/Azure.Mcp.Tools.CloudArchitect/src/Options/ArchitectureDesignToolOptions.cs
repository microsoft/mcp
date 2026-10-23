// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.CloudArchitect.Options;

/// <summary>
/// The set of parameters that the architecture design tool takes as input.
/// </summary>
public sealed class ArchitectureDesignToolOptions
{
    [Option(Description = "The current question being asked", DefaultValue = "")]
    public string Question { get; set; } = string.Empty;

    [Option(Description = "Current question number")]
    public int QuestionNumber { get; set; }

    [Option(Description = "Estimated total questions needed")]
    public int TotalQuestions { get; set; }

    [Option(Description = "The user's response to the question")]
    public string? Answer { get; set; }

    [Option(Description = "Whether another question is needed")]
    public bool NextQuestionNeeded { get; set; }

    [Option(Description = "A value between 0.0 and 1.0 representing confidence in understanding requirements. When this reaches 0.7 or higher, nextQuestionNeeded should be set to false.")]
    public double? ConfidenceScore { get; set; }

    [Option(Description = "The complete architecture state from the previous request as JSON, State input schema:\n{\n\"state\":{\n\"type\":\"object\",\n\"description\":\"The complete architecture state from the previous request\",\n\"properties\":{\n\"architectureComponents\":{\n\"type\":\"array\",\n\"description\":\"All architecture components suggested so far\",\n\"items\":{\n\"type\":\"string\"\n}\n},\n\"architectureTiers\":{\n\"type\":\"object\",\n\"description\":\"Components organized by architecture tier\",\n\"additionalProperties\":{\n\"type\":\"array\",\n\"items\":{\n\"type\":\"string\"\n}\n}\n},\n\"thought\":{\n\"type\":\"string\",\n\"description\":\"The calling agent's thoughts on the next question or reasoning process. The calling agent should use the requirements it has gathered to reason about the next question.\"\n},\n\"suggestedHint\":{\n\"type\":\"string\",\n\"description\":\"A suggested interaction hint to show the user, such as 'Ask me to create an ASCII art diagram of this architecture' or 'Ask about how this design handles disaster recovery'.\"\n},\n\"requirements\":{\n\"type\":\"object\",\n\"description\":\"Tracked requirements organized by type\",\n\"properties\":{\n\"explicit\":{\n\"type\":\"array\",\n\"description\":\"Requirements explicitly stated by the user\",\n\"items\":{\n\"type\":\"object\",\n\"properties\":{\n\"category\":{\n\"type\":\"string\"\n},\n\"description\":{\n\"type\":\"string\"\n},\n\"source\":{\n\"type\":\"string\"\n},\n\"importance\":{\n\"type\":\"string\",\n\"enum\":[\n\"high\",\n\"medium\",\n\"low\"\n]\n},\n\"confidence\":{\n\"type\":\"number\"\n}\n}\n}\n},\n\"implicit\":{\n\"type\":\"array\",\n\"description\":\"Requirements implied by user responses\",\n\"items\":{\n\"type\":\"object\",\n\"properties\":{\n\"category\":{\n\"type\":\"string\"\n},\n\"description\":{\n\"type\":\"string\"\n},\n\"source\":{\n\"type\":\"string\"\n},\n\"importance\":{\n\"type\":\"string\",\n\"enum\":[\n\"high\",\n\"medium\",\n\"low\"\n]\n},\n\"confidence\":{\n\"type\":\"number\"\n}\n}\n}\n},\n\"assumed\":{\n\"type\":\"array\",\n\"description\":\"Requirements assumed based on context/best practices\",\n\"items\":{\n\"type\":\"object\",\n\"properties\":{\n\"category\":{\n\"type\":\"string\"\n},\n\"description\":{\n\"type\":\"string\"\n},\n\"source\":{\n\"type\":\"string\"\n},\n\"importance\":{\n\"type\":\"string\",\n\"enum\":[\n\"high\",\n\"medium\",\n\"low\"\n]\n},\n\"confidence\":{\n\"type\":\"number\"\n}\n}\n}\n}\n}\n},\n\"confidenceFactors\":{\n\"type\":\"object\",\n\"description\":\"Factors that contribute to the overall confidence score\",\n\"properties\":{\n\"explicitRequirementsCoverage\":{\n\"type\":\"number\"\n},\n\"implicitRequirementsCertainty\":{\n\"type\":\"number\"\n},\n\"assumptionRisk\":{\n\"type\":\"number\"\n}\n}\n}\n}\n}\n}")]
    public string? State { get; set; }
}
