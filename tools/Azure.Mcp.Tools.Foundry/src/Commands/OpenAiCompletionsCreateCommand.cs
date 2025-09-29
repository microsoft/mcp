// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Models;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.Foundry.Models;
using Azure.Mcp.Tools.Foundry.Options;
using Azure.Mcp.Tools.Foundry.Options.Models;
using Azure.Mcp.Tools.Foundry.Services;

namespace Azure.Mcp.Tools.Foundry.Commands;

public sealed class OpenAiCompletionsCreateCommand : SubscriptionCommand<OpenAiCompletionsCreateOptions>
{
    private const string CommandTitle = "Create OpenAI Completion";

    public override string Name => "create-completion";

    public override string Description =>
        $"""
        Generate text completions using deployed Azure OpenAI models in AI Foundry. This tool sends prompts to Azure OpenAI 
        completion models and returns generated text with configurable parameters like temperature and max tokens. 
        Returns completion text as JSON. Requires resource-name, deployment-name, and prompt-text.
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
        command.Options.Add(FoundryOptionDefinitions.PromptTextOption);
        command.Options.Add(FoundryOptionDefinitions.MaxTokensOption);
        command.Options.Add(FoundryOptionDefinitions.TemperatureOption);
    }

    protected override OpenAiCompletionsCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.ResourceName = parseResult.GetValueOrDefault<string>(FoundryOptionDefinitions.ResourceNameOption.Name);
        options.DeploymentName = parseResult.GetValueOrDefault<string>(FoundryOptionDefinitions.DeploymentNameOption.Name);
        options.PromptText = parseResult.GetValueOrDefault<string>(FoundryOptionDefinitions.PromptTextOption.Name);
        options.MaxTokens = parseResult.GetValueOrDefault<int?>(FoundryOptionDefinitions.MaxTokensOption.Name);
        options.Temperature = parseResult.GetValueOrDefault<double?>(FoundryOptionDefinitions.TemperatureOption.Name);
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
            var result = await foundryService.CreateCompletionAsync(
                options.ResourceName!,
                options.DeploymentName!,
                options.PromptText!,
                options.Subscription!,
                options.ResourceGroup!,
                options.MaxTokens,
                options.Temperature,
                options.Tenant,
                options.AuthMethod ?? AuthMethod.Credential,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create<OpenAiCompletionsCreateCommandResult>(
                new OpenAiCompletionsCreateCommandResult(result.CompletionText, result.UsageInfo),
                FoundryJsonContext.Default.OpenAiCompletionsCreateCommandResult);
        }
        catch (Exception ex)
        {
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record OpenAiCompletionsCreateCommandResult(string CompletionText, CompletionUsageInfo UsageInfo);
}
