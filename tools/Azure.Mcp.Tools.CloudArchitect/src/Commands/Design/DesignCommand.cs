// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Reflection;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Helpers;
using Azure.Mcp.Tools.CloudArchitect.Models;
using Azure.Mcp.Tools.CloudArchitect.Options;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.CloudArchitect.Commands.Design;

public sealed class DesignCommand(ILogger<DesignCommand> logger) : GlobalCommand<ArchitectureDesignToolOptions>
{
    private const string CommandTitle = "Design Azure cloud architectures through guided questions";
    private readonly ILogger<DesignCommand> _logger = logger;

    private static readonly string s_designArchitectureText = LoadArchitectureDesignText();

    private static string GetArchitectureDesignText() => s_designArchitectureText;

    public override string Name => "design";

    public override string Description => """
        Design Azure architecture for a variety of large-scale cloud services, including: serving as ATM for users; cloud app for ordering groceries; cloud service that will store and present (stream/play back) videos for users; large-scale file upload, storage, and retrieval service; Iteratively gather functional and non-functional requirements; when confidenceScore >=0.7 emit Well-Architected multi-tier reference architecture (ingest/API, processing, storage, metadata/index, security/compliance, delivery/ops). File workloads: high-throughput parallel/chunked uploads, virus scanning, metadata indexing, lifecycle tiering (hot->cool->archive), geo-redundant object/blob storage, secure low-latency retrieval. Video workloads: upload, transcoding (adaptive bitrate), rendition storage, CDN/edge delivery, playback, analytics, cost optimization. Also supports transactional banking (ATM/banking app), retail & grocery ordering, e-commerce, SaaS. Required: none. Optional: question, answer, questionNumber, totalQuestions, nextQuestionNeeded (bool), confidenceScore (0-1), architectureComponent, architectureTier (frontend|api|processing|data|security|ops), state (JSON). Returns: components, dataFlows, azureServices, rationale, assumptions, risks, NFRs, securityCompliance, costLevers, nextSteps, asciiDiagram. Examples: design --question "Peak upload rate?" --confidenceScore 0.37; design --confidenceScore 0.72 (final). Not for deployment, pipelines, or IaC.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
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
        command.Options.Add(CloudArchitectOptionDefinitions.Question);
        command.Options.Add(CloudArchitectOptionDefinitions.QuestionNumber);
        command.Options.Add(CloudArchitectOptionDefinitions.TotalQuestions);
        command.Options.Add(CloudArchitectOptionDefinitions.Answer);
        command.Options.Add(CloudArchitectOptionDefinitions.NextQuestionNeeded);
        command.Options.Add(CloudArchitectOptionDefinitions.ConfidenceScore);
        command.Options.Add(CloudArchitectOptionDefinitions.State);

        command.Validators.Add(result =>
        {
            // Validate confidence score is between 0.0 and 1.0
            var confidenceScore = result.GetValue(CloudArchitectOptionDefinitions.ConfidenceScore);
            if (confidenceScore < 0.0 || confidenceScore > 1.0)
            {
                result.AddError("Confidence score must be between 0.0 and 1.0");
                return;
            }

            // Validate question number is not negative
            var questionNumber = result.GetValue(CloudArchitectOptionDefinitions.QuestionNumber);
            if (questionNumber < 0)
            {
                result.AddError("Question number cannot be negative");
                return;
            }

            // Validate total questions is not negative
            var totalQuestions = result.GetValue(CloudArchitectOptionDefinitions.TotalQuestions);
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
        options.Question = parseResult.GetValueOrDefault<string>(CloudArchitectOptionDefinitions.Question.Name) ?? string.Empty;
        options.QuestionNumber = parseResult.GetValueOrDefault<int>(CloudArchitectOptionDefinitions.QuestionNumber.Name);
        options.TotalQuestions = parseResult.GetValueOrDefault<int>(CloudArchitectOptionDefinitions.TotalQuestions.Name);
        options.Answer = parseResult.GetValueOrDefault<string>(CloudArchitectOptionDefinitions.Answer.Name);
        options.NextQuestionNeeded = parseResult.GetValueOrDefault<bool>(CloudArchitectOptionDefinitions.NextQuestionNeeded.Name);
        options.ConfidenceScore = parseResult.GetValueOrDefault<double>(CloudArchitectOptionDefinitions.ConfidenceScore.Name);
        options.State = DeserializeState(parseResult.GetValueOrDefault<string>(CloudArchitectOptionDefinitions.State.Name));
        return options;
    }

    private static ArchitectureDesignToolState DeserializeState(string? stateJson)
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

            context.Response.Status = HttpStatusCode.OK;
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
