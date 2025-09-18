// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Helpers;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.AzureTerraformBestPractices.Commands;

public sealed class AzureTerraformBestPracticesGetCommand(ILogger<AzureTerraformBestPracticesGetCommand> logger) : BaseCommand()
{
    private const string CommandTitle = "Get Terraform Best Practices for Azure";
    private readonly ILogger<AzureTerraformBestPracticesGetCommand> _logger = logger;
    private static readonly string s_bestPracticesText = LoadBestPracticesText();

    private static string GetBestPracticesText() => s_bestPracticesText;

    private static string LoadBestPracticesText()
    {
        Assembly assembly = typeof(AzureTerraformBestPracticesGetCommand).Assembly;
        string resourceName = EmbeddedResourceHelper.FindEmbeddedResource(assembly, "terraform-best-practices-for-azure.txt");
        return EmbeddedResourceHelper.ReadEmbeddedResource(assembly, resourceName);
    }

    public override string Name => "get";

    public override string Description =>
        @"Returns Terraform best practices for Azure. Call this command and follow its guidance before
        generating or suggesting any Terraform code specific to Azure. If this tool needs to be categorized, it belongs to
        the Azure Best Practices category.";

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

    public override Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var bestPractices = GetBestPracticesText();
        context.Response.Status = 200;
        context.Response.Results = ResponseResult.Create([bestPractices], AzureTerraformBestPracticesJsonContext.Default.ListString);
        context.Response.Message = string.Empty;
        return Task.FromResult(context.Response);
    }
}
