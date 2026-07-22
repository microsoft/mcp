// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using System.Text.Json;
using Azure.Mcp.Tools.CloudArchitect.Models;
using Azure.Mcp.Tools.CloudArchitect.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Helpers;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.CloudArchitect.Commands.Design;

[CommandMetadata(
    Id = "aa7c2a8b-c664-423b-8fb5-8edfbdadc783",
    Name = "design",
    Title = "Design Azure cloud architectures through guided questions",
    Description = """
        Recommends architecture design for cloud services/apps/solutions, such as: file storage, banking, video streaming, e-commerce, SaaS, and more. Use as follows:
        1. Ask about user role, business goals, etc (1-2 questions at a time).
        2. Track confidence returned by service and update requirements (explicit/implicit/assumed).
        3. Repeat steps 1 and 2 as needed until confidence >= 0.7
        4. Present architecture with table format, visual organization, ASCII diagrams.
        5. Follow Azure Well-Architected Framework principles.
        6. Cover all tiers: infrastructure, platform, application, data, security, operations.
        7. Provide actionable advice and high-level overview. Note: State tracks components, requirements by category, and confidence factors. Be conservative with suggestions.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class DesignCommand(ILogger<DesignCommand> logger)
    : BaseCommand<ArchitectureDesignToolOptions, CloudArchitectDesignResponse>
{
    private readonly ILogger<DesignCommand> _logger = logger;

    private static readonly string s_designArchitectureText = LoadArchitectureDesignText();

    private static string GetArchitectureDesignText() => s_designArchitectureText;

    private static string LoadArchitectureDesignText()
    {
        Assembly assembly = typeof(DesignCommand).Assembly;
        string resourceName = EmbeddedResourceHelper.FindEmbeddedResource(assembly, "azure-architecture-design.txt");
        return EmbeddedResourceHelper.ReadEmbeddedResource(assembly, resourceName);
    }

    public override void ValidateOptions(ArchitectureDesignToolOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        // Validate confidence score is between 0.0 and 1.0
        if (options.ConfidenceScore < 0.0 || options.ConfidenceScore > 1.0)
        {
            validationResult.Errors.Add("Confidence score must be between 0.0 and 1.0");
        }

        // Validate question number is not negative
        if (options.QuestionNumber < 0)
        {
            validationResult.Errors.Add("Question number cannot be negative");
        }

        // Validate total questions is not negative
        if (options.TotalQuestions < 0)
        {
            validationResult.Errors.Add("Total questions cannot be negative");
        }
    }

    internal static ArchitectureDesignToolState DeserializeState(string? stateJson)
    {
        if (string.IsNullOrEmpty(stateJson))
        {
            return new();
        }

        try
        {
            var state = JsonSerializer.Deserialize(stateJson, CloudArchitectJsonContext.Default.ArchitectureDesignToolState);
            return state ?? new();
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to deserialize state JSON: {ex.Message}", ex);
        }
    }

    public override Task<CommandResponse> ExecuteAsync(CommandContext context, ArchitectureDesignToolOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var designArchitecture = GetArchitectureDesignText();
            var state = DeserializeState(options.State);
            var responseObject = new CloudArchitectResponseObject(
                DisplayText: options.Question,
                DisplayThought: state.Thought,
                DisplayHint: state.SuggestedHint,
                QuestionNumber: options.QuestionNumber,
                TotalQuestions: options.TotalQuestions,
                NextQuestionNeeded: options.NextQuestionNeeded,
                State: state);

            context.Response.Results = ResponseResult.Create(new(designArchitecture, responseObject), CloudArchitectJsonContext.Default.CloudArchitectDesignResponse);
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
