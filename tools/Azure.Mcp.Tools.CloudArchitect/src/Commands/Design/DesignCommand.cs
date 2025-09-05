// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Helpers;
using Azure.Mcp.Tools.CloudArchitect.Models;
using Azure.Mcp.Tools.CloudArchitect.Options;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.CloudArchitect.Commands.Design;

public sealed class DesignCommand(ILogger<DesignCommand> logger) : GlobalCommand<ArchitectureDesignToolOptions>
{
    private const string CommandTitle = "Design Azure cloud architectures through guided questions";
    private readonly ILogger<DesignCommand> _logger = logger;

    private readonly Option<string> _questionOption = CloudArchitectOptionDefinitions.Question;
    private readonly Option<int> _questionNumberOption = CloudArchitectOptionDefinitions.QuestionNumber;
    private readonly Option<int> _questionTotalQuestions = CloudArchitectOptionDefinitions.TotalQuestions;
    private readonly Option<string> _answerOption = CloudArchitectOptionDefinitions.Answer;
    private readonly Option<bool> _nextQuestionNeededOption = CloudArchitectOptionDefinitions.NextQuestionNeeded;
    private readonly Option<double> _confidenceScoreOption = CloudArchitectOptionDefinitions.ConfidenceScore;

    private readonly Option<string> _architectureDesignToolState = CloudArchitectOptionDefinitions.State;

    private static readonly string s_designArchitectureText = LoadArchitectureDesignText();

    private static string GetArchitectureDesignText() => s_designArchitectureText;

    public override string Name => "design";

    public override string Description => """
        Azure architecture design tool that gathers requirements through guided questions and recommends optimal solutions.

        Key parameters: question, questionNumber, confidenceScore (0.0-1.0, present architecture when ≥0.7), totalQuestions, answer, nextQuestionNeeded, architectureComponent, architectureTier, state.

        Process:
        1. Ask about user role, business goals (1-2 questions at a time)
        2. Track confidence and update requirements (explicit/implicit/assumed)
        3. When confident enough, present architecture with table format, visual organization, ASCII diagrams
        4. Follow Azure Well-Architected Framework principles
        5. Cover all tiers: infrastructure, platform, application, data, security, operations
        6. Provide actionable advice and high-level overview

        State tracks components, requirements by category, and confidence factors. Be conservative with suggestions.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        ReadOnly = true
    };

    private static string LoadArchitectureDesignText()
    {
        Assembly assembly = typeof(DesignCommand).Assembly;
        string resourceName = EmbeddedResourceHelper.FindEmbeddedResource(assembly, "azure-architecture-design.txt");
        return EmbeddedResourceHelper.ReadEmbeddedResource(assembly, resourceName);
    }

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(_questionOption);
        command.Options.Add(_questionNumberOption);
        command.Options.Add(_questionTotalQuestions);
        command.Options.Add(_answerOption);
        command.Options.Add(_nextQuestionNeededOption);
        command.Options.Add(_confidenceScoreOption);
        command.Options.Add(_architectureDesignToolState);

        command.Validators.Add(result =>
            {
                // Validate confidence score is between 0.0 and 1.0
                var confidenceScore = result.GetValue(_confidenceScoreOption);
                if (confidenceScore < 0.0 || confidenceScore > 1.0)
                {
                    result.AddError("Confidence score must be between 0.0 and 1.0");
                    return;
                }

                // Validate question number is not negative
                var questionNumber = result.GetValue(_questionNumberOption);
                if (questionNumber < 0)
                {
                    result.AddError("Question number cannot be negative");
                    return;
                }

                // Validate total questions is not negative
                var totalQuestions = result.GetValue(_questionTotalQuestions);
                if (totalQuestions < 0)
                {
                    result.AddError("Total questions cannot be negative");
                    return;
                }
            });
    }

    protected override ArchitectureDesignToolOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Question = parseResult.GetValue(_questionOption) ?? string.Empty;
        options.QuestionNumber = parseResult.GetValue(_questionNumberOption);
        options.TotalQuestions = parseResult.GetValue(_questionTotalQuestions);
        options.Answer = parseResult.GetValue(_answerOption);
        options.NextQuestionNeeded = parseResult.GetValue(_nextQuestionNeededOption);
        options.ConfidenceScore = parseResult.GetValue(_confidenceScoreOption);
        options.State = DeserializeState(parseResult.GetValue(_architectureDesignToolState));
        return options;
    }

    private static ArchitectureDesignToolState DeserializeState(string? stateJson)
    {
        if (string.IsNullOrEmpty(stateJson))
        {
            return new ArchitectureDesignToolState();
        }

        try
        {
            var state = JsonSerializer.Deserialize(stateJson, CloudArchitectJsonContext.Default.ArchitectureDesignToolState);
            return state ?? new ArchitectureDesignToolState();
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to deserialize state JSON: {ex.Message}", ex);
        }
    }

    public override Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return Task.FromResult(context.Response);
        }

        var options = BindOptions(parseResult);

        try
        {
            var designArchitecture = GetArchitectureDesignText();
            var responseObject = new CloudArchitectResponseObject
            {
                DisplayText = options.Question,
                DisplayThought = options.State.Thought,
                DisplayHint = options.State.SuggestedHint,
                QuestionNumber = options.QuestionNumber,
                TotalQuestions = options.TotalQuestions,
                NextQuestionNeeded = options.NextQuestionNeeded,
                State = options.State
            };

            var result = new CloudArchitectDesignResponse
            {
                DesignArchitecture = designArchitecture,
                ResponseObject = responseObject
            };

            context.Response.Status = 200;
            context.Response.Results = ResponseResult.Create(result, CloudArchitectJsonContext.Default.CloudArchitectDesignResponse);
            context.Response.Message = string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred in cloud architect design command");
            HandleException(context, ex);
        }
        return Task.FromResult(context.Response);
    }
}
