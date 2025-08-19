using Azure;
using Azure.AI.OpenAI;
using Azure.Core;
using Azure.Identity;
using AzureMcp.Ai.Models;
using AzureMcp.Ai.Options.Completions;
using Microsoft.Extensions.Logging;

namespace AzureMcp.Ai.Services;

public sealed class AiService(ILogger<AiService> logger) : IAiService
{
    private readonly ILogger<AiService> _logger = logger;

    public async Task<CompletionsCreateResult> CreateCompletionAsync(
        CompletionsCreateOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (string.IsNullOrWhiteSpace(options.ResourceName))
            throw new ArgumentException("resource-name is required.", nameof(options.ResourceName));
        if (string.IsNullOrWhiteSpace(options.DeploymentName))
            throw new ArgumentException("deployment-name is required.", nameof(options.DeploymentName));
        if (string.IsNullOrWhiteSpace(options.PromptText))
            throw new ArgumentException("prompt-text is required.", nameof(options.PromptText));

        var endpoint = new Uri($"https://{options.ResourceName!.Trim()}.openai.azure.com/");
        TokenCredential credential = new DefaultAzureCredential();
        var client = new OpenAIClient(endpoint, credential);

        int maxTokens = options.MaxTokens ?? 256;
        double temperature = options.Temperature ?? 0.7;

        var req = new CompletionsOptions
        {
            MaxTokens = maxTokens,
            Temperature = (float)temperature
        };
        req.Prompts.Add(options.PromptText!);

        Response<Completions> response = await client.GetCompletionsAsync(
            deploymentOrModelName: options.DeploymentName!,
            req,
            cancellationToken);

        var completions = response.Value;
        var choice = completions.Choices.Count > 0 ? completions.Choices[0] : null;

        string? text = choice?.Text;
        string? finish = choice?.FinishReason?.ToString();

        var usage = completions.Usage;
        int? promptTokens = usage?.PromptTokens;
        int? completionTokens = usage?.CompletionTokens;
        int? totalTokens = usage?.TotalTokens;

        return new CompletionsCreateResult
        {
            Text = text,
            FinishReason = finish,
            PromptTokens = promptTokens,
            CompletionTokens = completionTokens,
            TotalTokens = totalTokens
        };
    }
}