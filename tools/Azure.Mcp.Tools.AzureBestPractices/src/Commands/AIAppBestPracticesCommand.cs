// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Reflection;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AzureBestPractices.Commands;

public sealed class AIAppBestPracticesCommand(ILogger<AIAppBestPracticesCommand> logger) : BaseCommand<EmptyOptions>
{
    private const string CommandTitle = "Get AI App Best Practices";
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

    public override string Id => "6c29659e-406d-4b9b-8150-e3d4fd7ba31c";

    public override string Name => "ai_app";

    public override string Description =>
        @"Returns best practices and code generation guidance for building AI applications in Azure. 
        Use this command when you need recommendations on how to write code for AI agents, chatbots, workflows, or any AI / LLM features.
        This command also provides guidance for code generation on Microsoft Foundry for application development. 
        When the request involves code generation of AI components or applications in any capacity, use this command instead of calling the general code generation best practices command.
        Call this `get_bestpractices_ai_app` command first before creating any plans, todos or code.
        If this tool needs to be categorized, it belongs to the Azure Best Practices category.";

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
