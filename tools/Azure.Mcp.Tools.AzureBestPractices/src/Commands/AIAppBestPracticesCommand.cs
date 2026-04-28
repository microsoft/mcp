// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Helpers;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AzureBestPractices.Commands;

[CommandMetadata(
    Id = "6c29659e-406d-4b9b-8150-e3d4fd7ba31c",
    Name = "ai_app",
    Title = "Get AI App Best Practices",
    Description = """
        Returns best practices and code generation guidance for building AI applications in Azure.
        Use this command when you need recommendations on how to write code for AI agents, chatbots, workflows, or any AI / LLM features.
        This command also provides guidance for code generation on Microsoft Foundry for application development.
        When the request involves code generation of AI components or AI applications in any capacity, use this command instead of calling the general code generation best practices command.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class AIAppBestPracticesCommand(ILogger<AIAppBestPracticesCommand> logger) : BaseCommand<EmptyOptions>
{
    private readonly ILogger<AIAppBestPracticesCommand> _logger = logger;
    private static readonly string s_bestPracticesText = LoadBestPracticesText();

    private static string GetBestPracticesText() => s_bestPracticesText;

    private static string LoadBestPracticesText()
    {
        string backgroundKnowledge = LoadEmbeddedText("ai-background-knowledge.txt");
        string errorPatterns = LoadEmbeddedText("ai-error-patterns.txt");
        string bestPracticesCore = LoadEmbeddedText("ai-best-practices-core.txt");

        return bestPracticesCore
            .Replace("{{BACKGROUND_KNOWLEDGE}}", backgroundKnowledge)
            .Replace("{{ERROR_PATTERNS}}", errorPatterns);
    }

    private static string LoadEmbeddedText(string fileName)
    {
        Assembly assembly = typeof(AIAppBestPracticesCommand).Assembly;
        string resourceName = EmbeddedResourceHelper.FindEmbeddedResource(assembly, fileName);
        return EmbeddedResourceHelper.ReadEmbeddedResource(assembly, resourceName);
    }

    protected override EmptyOptions BindOptions(ParseResult parseResult) => new();

    public override Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        try
        {
            var bestPractices = GetBestPracticesText();
            context.Response.Status = HttpStatusCode.OK;
            context.Response.Results = ResponseResult.Create([bestPractices], AzureBestPracticesJsonContext.Default.ListString);
            context.Response.Message = string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting AI app best practices for Azure");
            HandleException(context, ex);
        }

        return Task.FromResult(context.Response);
    }
}
