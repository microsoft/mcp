// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Tools.FoundryExtensions.Models;
using Azure.Mcp.Tools.FoundryExtensions.Options;
using Azure.Mcp.Tools.FoundryExtensions.Options.Models;
using Azure.Mcp.Tools.FoundryExtensions.Services;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.FoundryExtensions.Commands;

public sealed class OpenAiCompletionsCreateCommand(IFoundryExtensionsService foundryExtensionsService) : SubscriptionCommand<OpenAiCompletionsCreateOptions>
{
    private readonly IFoundryExtensionsService _foundryExtensionsService = foundryExtensionsService;

    private const string CommandTitle = "Create OpenAI Completion";

    public override string Id => "e5f6a7b8-5678-9abc-ef01-234567890123";

    public override string Name => "create-completion";

    public override string Description =>
        $"""
        Create text completions using Azure OpenAI in Microsoft Foundry. Send a prompt or question to Azure OpenAI models
        deployed in your Microsoft Foundry resource and receive generated text answers. Use this when you need to create
        completions, get AI-generated content, generate answers to questions, or produce text completions from Azure
        OpenAI based on any input prompt. Supports customization with temperature and max tokens.
        Requires resource-name, deployment-name, and prompt-text.
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
        command.Options.Add(FoundryExtensionsOptionDefinitions.ResourceNameOption);
        command.Options.Add(FoundryExtensionsOptionDefinitions.DeploymentNameOption);
        command.Options.Add(FoundryExtensionsOptionDefinitions.PromptTextOption);
        command.Options.Add(FoundryExtensionsOptionDefinitions.MaxTokensOption);
        command.Options.Add(FoundryExtensionsOptionDefinitions.TemperatureOption);
    }

    protected override OpenAiCompletionsCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.ResourceName = parseResult.GetValueOrDefault<string>(FoundryExtensionsOptionDefinitions.ResourceNameOption.Name);
        options.DeploymentName = parseResult.GetValueOrDefault<string>(FoundryExtensionsOptionDefinitions.DeploymentNameOption.Name);
        options.PromptText = parseResult.GetValueOrDefault<string>(FoundryExtensionsOptionDefinitions.PromptTextOption.Name);
        options.MaxTokens = parseResult.GetValueOrDefault<int?>(FoundryExtensionsOptionDefinitions.MaxTokensOption.Name);
        options.Temperature = parseResult.GetValueOrDefault<double?>(FoundryExtensionsOptionDefinitions.TemperatureOption.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        try
        {
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            {
                return context.Response;
            }

            var options = BindOptions(parseResult);

            var foundryService = _foundryExtensionsService;
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
                options.RetryPolicy,
                cancellationToken: cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(result.CompletionText, result.UsageInfo),
                FoundryExtensionsJsonContext.Default.OpenAiCompletionsCreateCommandResult);
        }
        catch (Exception ex)
        {
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record OpenAiCompletionsCreateCommandResult(string CompletionText, CompletionUsageInfo UsageInfo);
}
