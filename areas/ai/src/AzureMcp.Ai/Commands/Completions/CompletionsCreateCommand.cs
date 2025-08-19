using System.CommandLine;
using System.CommandLine.Parsing;
using AzureMcp.Ai.Options;
using AzureMcp.Ai.Options.Completions;
using AzureMcp.Ai.Serialization;
using AzureMcp.Ai.Services;
using AzureMcp.Core.Commands;
using Microsoft.Extensions.Logging;

namespace AzureMcp.Ai.Commands.Completions;

public sealed class CompletionsCreateCommand(ILogger<CompletionsCreateCommand> logger)
    : GlobalCommand<CompletionsCreateOptions>()
{
    private const string CommandTitle = "Generate text completions using Azure OpenAI";
    private readonly ILogger<CompletionsCreateCommand> _logger = logger;

    private readonly Option<string> _resourceName = AiOptionDefinitions.Common.ResourceName;
    private readonly Option<string> _deploymentName = AiOptionDefinitions.Common.DeploymentName;
    private readonly Option<string> _promptText = AiOptionDefinitions.Common.PromptText;
    private readonly Option<int?> _maxTokens = AiOptionDefinitions.Common.MaxTokens;
    private readonly Option<double?> _temperature = AiOptionDefinitions.Common.Temperature;
    private readonly Option<string?> _apiVersion = AiOptionDefinitions.Common.ApiVersion;

    public override string Name => "create";
    public override string Title => CommandTitle;

    public override string Description =>
        "Generate text completions using deployed Azure OpenAI models. " +
        "Requires --resource-name, --deployment-name, and --prompt-text. " +
        "Optional: --temperature, --max-tokens.";

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        ReadOnly = true,
        SuggestedUserPrompt = "Generate a text completion for prompt: "
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_resourceName);
        command.AddOption(_deploymentName);
        command.AddOption(_promptText);
        command.AddOption(_maxTokens);
        command.AddOption(_temperature);
        command.AddOption(_apiVersion);
    }

    protected override CompletionsCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceName = parseResult.GetValueForOption(_resourceName);
        options.DeploymentName = parseResult.GetValueForOption(_deploymentName);
        options.PromptText = parseResult.GetValueForOption(_promptText);
        options.MaxTokens = parseResult.GetValueForOption(_maxTokens);
        options.Temperature = parseResult.GetValueForOption(_temperature);
        options.ApiVersion = parseResult.GetValueForOption(_apiVersion);
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

            var service = context.GetService<IAiService>();
            var result = await service.CreateCompletionAsync(options, context.CancellationToken);
            context.Response.Results = ResponseResult.Create(result, AiJsonContext.Default.CompletionsCreateResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating completion");
            HandleException(context, ex);
        }

        return context.Response;
    }
}