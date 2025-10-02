// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Nodes;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Models;
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
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
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
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.ResourceName = parseResult.GetValueOrDefault<string>(FoundryOptionDefinitions.ResourceNameOption.Name);
        options.DeploymentName = parseResult.GetValueOrDefault<string>(FoundryOptionDefinitions.DeploymentNameOption.Name);
        options.MessageArray = parseResult.GetValueOrDefault<string>(FoundryOptionDefinitions.MessageArrayOption.Name);
        options.MaxTokens = parseResult.GetValueOrDefault<int?>(FoundryOptionDefinitions.MaxTokensOption.Name);
        options.Temperature = parseResult.GetValueOrDefault<double?>(FoundryOptionDefinitions.TemperatureOption.Name);
        options.TopP = parseResult.GetValueOrDefault<double?>(FoundryOptionDefinitions.TopPOption.Name);
        options.FrequencyPenalty = parseResult.GetValueOrDefault<double?>(FoundryOptionDefinitions.FrequencyPenaltyOption.Name);
        options.PresencePenalty = parseResult.GetValueOrDefault<double?>(FoundryOptionDefinitions.PresencePenaltyOption.Name);
        options.Stop = parseResult.GetValueOrDefault<string>(FoundryOptionDefinitions.StopOption.Name);
        options.Stream = parseResult.GetValueOrDefault<bool?>(FoundryOptionDefinitions.StreamOption.Name);
        options.Seed = parseResult.GetValueOrDefault<int?>(FoundryOptionDefinitions.SeedOption.Name);
        options.User = parseResult.GetValueOrDefault<string>(FoundryOptionDefinitions.UserOption.Name);
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

            // Parse the message array
            var messages = new List<object>();
            if (!string.IsNullOrEmpty(options.MessageArray))
            {
                var jsonDocument = JsonDocument.Parse(options.MessageArray);
                foreach (var element in jsonDocument.RootElement.EnumerateArray())
                {
                    // Convert JsonElement to JsonObject for proper type matching in service
                    var jsonNode = JsonNode.Parse(element.GetRawText());
                    if (jsonNode is JsonObject jsonObj)
                    {
                        messages.Add(jsonObj);
                    }
                }
            }

            var result = await foundryService.CreateChatCompletionsAsync(
                options.ResourceName!,
                options.DeploymentName!,
                options.Subscription!,
                options.ResourceGroup!,
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
                options.Tenant,
                options.AuthMethod ?? AuthMethod.Credential,
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
