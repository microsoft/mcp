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
        Description = "The specific question to ask the user. Use clear, focused questions about business goals, technical requirements, or constraints. Example: 'What type of application are you building?' or 'How many users do you expect?'",
        Required = false
    };

    public static readonly Option<int> QuestionNumber = new(
        $"--{QuestionNumberName}"
    )
    {
        Description = "Sequential number of current question (starts at 1). Used to track conversation progress and maintain context.",
        Required = false
    };

    public static readonly Option<int> TotalQuestions = new(
        $"--{TotalQuestionsName}"
    )
    {
        Description = "Estimated total questions needed to reach confidence threshold. Typically 3-8 questions depending on complexity. Helps set user expectations.",
        Required = false
    };

    public static readonly Option<string> Answer = new(
        $"--{AnswerName}"
    )
    {
        Description = "User's answer to the current question. Process this to extract requirements and update confidence score. Use to determine next question or present architecture.",
        Required = false
    };

    public static readonly Option<bool> NextQuestionNeeded = new(
        $"--{NextQuestionNeededName}"
    )
    {
        Description = "Boolean indicating if more questions are needed. Set to true while gathering requirements, false when ready to present architecture (confidenceScore ≥ 0.7).",
        Required = false
    };

    public static readonly Option<double> ConfidenceScore = new(
        $"--{ConfidenceScoreName}"
    )
    {
        Description = "Confidence level in understanding user requirements (0.0-1.0). Start around 0.1-0.2, increase with each answer. At ≥0.7, present final architecture by setting nextQuestionNeeded=false. Key decision threshold for tool completion.",
        Required = false
    };

    public static readonly Option<string> State = new(
        $"--{StateName}"
    )
    {
        Description = "JSON state object tracking conversation progress and gathered requirements. Contains: architectureComponents (list), requirements (explicit/implicit/assumed), confidenceFactors, and conversation context. Pass previous state to maintain context between questions. Essential for multi-turn conversations.",
        Required = false
    };
}
