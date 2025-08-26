// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMcp.Core.Models.Option;
using AzureMcp.Ai.Options;
using AzureMcp.Ai.Options.OpenAi;
using AzureMcp.Ai.Services;
using AzureMcp.Ai.Models;
using Microsoft.Extensions.Logging;

namespace AzureMcp.Ai.Commands.OpenAi;

public sealed class OpenAiCompletionsCreateCommand(ILogger<OpenAiCompletionsCreateCommand> logger) : BaseAiCommand<OpenAiCompletionsCreateOptions>()
{
    private const string CommandTitle = "Create OpenAI Completion";
    private readonly ILogger<OpenAiCompletionsCreateCommand> _logger = logger;

    public override string Name => "create";

    public override string Description =>
        $"""
        Generate text completions using deployed Azure OpenAI models. This tool sends prompts to Azure OpenAI 
        completion models and returns generated text with configurable parameters like temperature and max tokens. 
        Returns completion text as JSON. Requires resource-name, deployment-name, and prompt-text.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(AiOptionDefinitions.DeploymentName);
        command.AddOption(AiOptionDefinitions.PromptText);
        command.AddOption(AiOptionDefinitions.MaxTokens);
        command.AddOption(AiOptionDefinitions.Temperature);
    }

    protected override OpenAiCompletionsCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.DeploymentName = parseResult.GetValueForOption(AiOptionDefinitions.DeploymentName);
        options.PromptText = parseResult.GetValueForOption(AiOptionDefinitions.PromptText);
        options.MaxTokens = parseResult.GetValueForOption(AiOptionDefinitions.MaxTokens);
        options.Temperature = parseResult.GetValueForOption(AiOptionDefinitions.Temperature);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var options = BindOptions(parseResult);

        try
        {
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            {
                return context.Response;
            }

            var aiService = context.GetService<IAiService>();
            var result = await aiService.CreateCompletionAsync(
                options.ResourceName!,
                options.DeploymentName!,
                options.PromptText!,
                options.Subscription!,
                options.ResourceGroup!,
                options.MaxTokens,
                options.Temperature,
                options.Tenant,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create<OpenAiCompletionsCreateCommandResult>(
                new OpenAiCompletionsCreateCommandResult(result.CompletionText, result.UsageInfo), 
                AiJsonContext.Default.OpenAiCompletionsCreateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating OpenAI completion for resource '{ResourceName}', deployment '{DeploymentName}'", 
                options.ResourceName, options.DeploymentName);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record OpenAiCompletionsCreateCommandResult(string CompletionText, CompletionUsageInfo UsageInfo);
}
