// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Reflection;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Helpers;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.AzureAIBestPractices.Commands;

public sealed class AzureAIBestPracticesGetCommand(ILogger<AzureAIBestPracticesGetCommand> logger) : BaseCommand<EmptyOptions>
{
    private const string CommandTitle = "Get AI App Best Practices for Azure";
    private readonly ILogger<AzureAIBestPracticesGetCommand> _logger = logger;
    private static readonly string s_bestPracticesText = LoadBestPracticesText();

    private static string GetBestPracticesText() => s_bestPracticesText;

    private static string LoadBestPracticesText()
    {
        Assembly assembly = typeof(AzureAIBestPracticesGetCommand).Assembly;
        string resourceName = EmbeddedResourceHelper.FindEmbeddedResource(assembly, "ai-best-practices-for-azure.txt");
        return EmbeddedResourceHelper.ReadEmbeddedResource(assembly, resourceName);
    }

    public override string Id => "6c29659e-406d-4b9b-8150-e3d4fd7ba31c";

    public override string Name => "get";

    public override string Description =>
        @"Returns best practices and code generation guidance for building AI applications in Azure. 
        Use this tool when you need recommendations on how to write code for AI agents, chatbots, workflows, or other AI features.
        This tool also provides guidance for code generation using the Azure resources (e.g. Azure AI Foundry) for application development only. 
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
            context.Response.Results = ResponseResult.Create([bestPractices], AzureAIBestPracticesJsonContext.Default.ListString);
            context.Response.Message = string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting AI best practices for Azure");
            HandleException(context, ex);
        }

        return Task.FromResult(context.Response);
    }
}
