// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
using System.Text.Json;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Models;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.Foundry.Models;
using Azure.Mcp.Tools.Foundry.Options;
using Azure.Mcp.Tools.Foundry.Options.Models;
using Azure.Mcp.Tools.Foundry.Services;

namespace Azure.Mcp.Tools.Foundry.Commands;

public sealed class OpenAiChatCompletionsCreateCommand : SubscriptionCommand<OpenAiChatCompletionsCreateOptions>
{
    private const string CommandTitle = "Create OpenAI Chat Completions";

    public override string Name => "chat-completions-create";

    public override string Description =>
        $"""
        Create interactive chat completions using Azure OpenAI chat models. This tool processes conversational 
        inputs with message history and system instructions to generate contextual responses. Returns chat 
        response as JSON. Requires resource-name, deployment-name, and message-array.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = false,
        OpenWorld = false,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(FoundryOptionDefinitions.ResourceNameOption);
        command.Options.Add(FoundryOptionDefinitions.DeploymentNameOption);
        command.Options.Add(FoundryOptionDefinitions.MessageArrayOption);
        command.Options.Add(FoundryOptionDefinitions.MaxTokensOption);
        command.Options.Add(FoundryOptionDefinitions.TemperatureOption);
        command.Options.Add(FoundryOptionDefinitions.TopPOption);
        command.Options.Add(FoundryOptionDefinitions.FrequencyPenaltyOption);
        command.Options.Add(FoundryOptionDefinitions.PresencePenaltyOption);
        command.Options.Add(FoundryOptionDefinitions.StopOption);
        command.Options.Add(FoundryOptionDefinitions.StreamOption);
        command.Options.Add(FoundryOptionDefinitions.SeedOption);
        command.Options.Add(FoundryOptionDefinitions.UserOption);
    }

    protected override OpenAiChatCompletionsCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceName = parseResult.GetValueOrDefault<string>(FoundryOptionDefinitions.ResourceName);
        options.DeploymentName = parseResult.GetValueOrDefault<string>(FoundryOptionDefinitions.DeploymentName);
        options.MessageArray = parseResult.GetValueOrDefault<string>(FoundryOptionDefinitions.MessageArray);
        options.MaxTokens = parseResult.GetValueOrDefault<int?>(FoundryOptionDefinitions.MaxTokens);
        options.Temperature = parseResult.GetValueOrDefault<double?>(FoundryOptionDefinitions.Temperature);
        options.TopP = parseResult.GetValueOrDefault<double?>(FoundryOptionDefinitions.TopP);
        options.FrequencyPenalty = parseResult.GetValueOrDefault<double?>(FoundryOptionDefinitions.FrequencyPenalty);
        options.PresencePenalty = parseResult.GetValueOrDefault<double?>(FoundryOptionDefinitions.PresencePenalty);
        options.Stop = parseResult.GetValueOrDefault<string>(FoundryOptionDefinitions.Stop);
        options.Stream = parseResult.GetValueOrDefault<bool?>(FoundryOptionDefinitions.Stream);
        options.Seed = parseResult.GetValueOrDefault<int?>(FoundryOptionDefinitions.Seed);
        options.User = parseResult.GetValueOrDefault<string>(FoundryOptionDefinitions.User);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        try
        {
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            {
                return context.Response;
            }

            var options = BindOptions(parseResult);

            var foundryService = context.GetService<IFoundryService>();

            // Parse the message array (simple parsing for now)
            var messages = new List<object>();
            if (!string.IsNullOrEmpty(options.MessageArray))
            {
                // For now, use basic JSON parsing - this will be refined in the service layer
                var jsonDocument = JsonDocument.Parse(options.MessageArray);
                foreach (var element in jsonDocument.RootElement.EnumerateArray())
                {
                    messages.Add(element);
                }
            }

            var result = await foundryService.CreateChatCompletionsAsync(
                options.Subscription!,
                options.ResourceName!,
                options.DeploymentName!,
                messages,
                options.MaxTokens,
                options.Temperature,
                options.TopP,
                options.FrequencyPenalty,
                options.PresencePenalty,
                options.Stop,
                options.Stream,
                options.Seed,
                options.User,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create<OpenAiChatCompletionsCreateCommandResult>(
                new OpenAiChatCompletionsCreateCommandResult(result, options.ResourceName!, options.DeploymentName!),
                FoundryJsonContext.Default.OpenAiChatCompletionsCreateCommandResult);
        }
        catch (Exception ex)
        {
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record OpenAiChatCompletionsCreateCommandResult(
        ChatCompletionResult Result,
        string ResourceName,
        string DeploymentName);
}
