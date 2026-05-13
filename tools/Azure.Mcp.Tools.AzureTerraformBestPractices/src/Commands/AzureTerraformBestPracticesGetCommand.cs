// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Helpers;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AzureTerraformBestPractices.Commands;

[CommandMetadata(
    Id = "5bd36575-6313-4bf4-aa26-a79fe0fa32a8",
    Name = "get",
    Title = "Get Terraform Best Practices for Azure",
    Description = """
        Returns Terraform best practices for Azure. Call this command and follow its guidance before
        generating or suggesting any Terraform code specific to Azure. If this tool needs to be categorized, it belongs to
        the Azure Best Practices category.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class AzureTerraformBestPracticesGetCommand(ILogger<AzureTerraformBestPracticesGetCommand> logger) : BaseCommand<EmptyOptions>
{
    private readonly ILogger<AzureTerraformBestPracticesGetCommand> _logger = logger;
    private static readonly string s_bestPracticesText = LoadBestPracticesText();

    private static string GetBestPracticesText() => s_bestPracticesText;

    private static string LoadBestPracticesText()
    {
        Assembly assembly = typeof(AzureTerraformBestPracticesGetCommand).Assembly;
        string resourceName = EmbeddedResourceHelper.FindEmbeddedResource(assembly, "terraform-best-practices-for-azure.txt");
        return EmbeddedResourceHelper.ReadEmbeddedResource(assembly, resourceName);
    }

    protected override EmptyOptions BindOptions(ParseResult parseResult) => new();

    public override Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        var bestPractices = GetBestPracticesText();
        context.Response.Status = HttpStatusCode.OK;
        context.Response.Results = ResponseResult.Create([bestPractices], AzureTerraformBestPracticesJsonContext.Default.ListString);
        context.Response.Message = string.Empty;
        return Task.FromResult(context.Response);
    }
}
