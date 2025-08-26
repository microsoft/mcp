// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.CloudArchitect.Options;

public static class CloudArchitectOptionDefinitions
{
    public const string QuestionName = "question";
    public const string QuestionNumberName = "question-number";
    public const string TotalQuestionsName = "total-questions";
    public const string AnswerName = "answer";
    public const string NextQuestionNeededName = "next-question-needed";
    public const string ConfidenceScoreName = "confidence-score";
    public const string ArchitectureComponentName = "architecture-component";
    public const string ArchitectureTierName = "architecture-tier";
    public const string StateName = "state";

    public static readonly Option<string> Question = new(
        $"--{QuestionName}"
    )
    {
        Description = "The current question being asked",
        Required = false
    };

    public static readonly Option<int> QuestionNumber = new(
        $"--{QuestionNumberName}"
    )
    {
        Description = "Current question number",
        Required = false
    };

    public static readonly Option<int> TotalQuestions = new(
        $"--{TotalQuestionsName}"
    )
    {
        Description = "Estimated total questions needed",
        Required = false
    };

    public static readonly Option<string> Answer = new(
        $"--{AnswerName}"
    )
    {
        Description = "The user's response to the question",
        Required = false
    };

    public static readonly Option<bool> NextQuestionNeeded = new(
        $"--{NextQuestionNeededName}"
    )
    {
        Description = "Whether another question is needed",
        Required = false
    };

    public static readonly Option<double> ConfidenceScore = new(
        $"--{ConfidenceScoreName}"
    )
    {
        Description = "A value between 0.0 and 1.0 representing confidence in understanding requirements. When this reaches 0.7 or higher, nextQuestionNeeded should be set to false.",
        Required = false
    };

    public static readonly Option<string> State = new(
        $"--{StateName}"
    )
    {
        Description = "The complete architecture state from the previous request as JSON, State input schema:\n{\n\"state\":{\n\"type\":\"object\",\n\"description\":\"The complete architecture state from the previous request\",\n\"properties\":{\n\"architectureComponents\":{\n\"type\":\"array\",\n\"description\":\"All architecture components suggested so far\",\n\"items\":{\n\"type\":\"string\"\n}\n},\n\"architectureTiers\":{\n\"type\":\"object\",\n\"description\":\"Components organized by architecture tier\",\n\"additionalProperties\":{\n\"type\":\"array\",\n\"items\":{\n\"type\":\"string\"\n}\n}\n},\n\"thought\":{\n\"type\":\"string\",\n\"description\":\"The calling agent's thoughts on the next question or reasoning process. The calling agent should use the requirements it has gathered to reason about the next question.\"\n},\n\"suggestedHint\":{\n\"type\":\"string\",\n\"description\":\"A suggested interaction hint to show the user, such as 'Ask me to create an ASCII art diagram of this architecture' or 'Ask about how this design handles disaster recovery'.\"\n},\n\"requirements\":{\n\"type\":\"object\",\n\"description\":\"Tracked requirements organized by type\",\n\"properties\":{\n\"explicit\":{\n\"type\":\"array\",\n\"description\":\"Requirements explicitly stated by the user\",\n\"items\":{\n\"type\":\"object\",\n\"properties\":{\n\"category\":{\n\"type\":\"string\"\n},\n\"description\":{\n\"type\":\"string\"\n},\n\"source\":{\n\"type\":\"string\"\n},\n\"importance\":{\n\"type\":\"string\",\n\"enum\":[\n\"high\",\n\"medium\",\n\"low\"\n]\n},\n\"confidence\":{\n\"type\":\"number\"\n}\n}\n}\n},\n\"implicit\":{\n\"type\":\"array\",\n\"description\":\"Requirements implied by user responses\",\n\"items\":{\n\"type\":\"object\",\n\"properties\":{\n\"category\":{\n\"type\":\"string\"\n},\n\"description\":{\n\"type\":\"string\"\n},\n\"source\":{\n\"type\":\"string\"\n},\n\"importance\":{\n\"type\":\"string\",\n\"enum\":[\n\"high\",\n\"medium\",\n\"low\"\n]\n},\n\"confidence\":{\n\"type\":\"number\"\n}\n}\n}\n},\n\"assumed\":{\n\"type\":\"array\",\n\"description\":\"Requirements assumed based on context/best practices\",\n\"items\":{\n\"type\":\"object\",\n\"properties\":{\n\"category\":{\n\"type\":\"string\"\n},\n\"description\":{\n\"type\":\"string\"\n},\n\"source\":{\n\"type\":\"string\"\n},\n\"importance\":{\n\"type\":\"string\",\n\"enum\":[\n\"high\",\n\"medium\",\n\"low\"\n]\n},\n\"confidence\":{\n\"type\":\"number\"\n}\n}\n}\n}\n}\n},\n\"confidenceFactors\":{\n\"type\":\"object\",\n\"description\":\"Factors that contribute to the overall confidence score\",\n\"properties\":{\n\"explicitRequirementsCoverage\":{\n\"type\":\"number\"\n},\n\"implicitRequirementsCertainty\":{\n\"type\":\"number\"\n},\n\"assumptionRisk\":{\n\"type\":\"number\"\n}\n}\n}\n}\n}\n}",
        Required = false
    };
}
