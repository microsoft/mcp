// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AzureTerraform.Models;
using Azure.Mcp.Tools.AzureTerraform.Options;
using Azure.Mcp.Tools.AzureTerraform.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AzureTerraform.Commands;

[CommandMetadata(
    Id = "d4e5f6a7-b8c9-0123-abcd-567890123def",
    Name = "get",
    Title = "Get AzureRM Provider Documentation",
    Description = """
        Retrieves comprehensive AzureRM Terraform provider documentation for a specified resource type.
        Returns the resource summary, arguments with descriptions and requirements, attributes,
        usage examples, and important notes. Use --resource-type to specify the resource
        (e.g., azurerm_resource_group). Optionally filter by --doc-type (resource or data-source),
        --argument, or --attribute.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = true,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class AzureRMDocsGetCommand(
    ILogger<AzureRMDocsGetCommand> logger,
    IAzureRMDocsService docsService) : BaseCommand<AzureRMDocsOptions, AzureRMDocsResult>
{
    private readonly ILogger<AzureRMDocsGetCommand> _logger = logger;
    private readonly IAzureRMDocsService _docsService = docsService;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        AzureRMDocsOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _docsService.GetDocumentationAsync(
                options.ResourceType,
                options.DocType ?? "resource",
                options.Argument,
                options.Attribute,
                cancellationToken).ConfigureAwait(false);

            context.Response.Results = ResponseResult.Create(result, AzureTerraformJsonContext.Default.AzureRMDocsResult);

            context.Activity
                ?.AddTag(AzureTerraformTelemetryTags.ToolArea, "azurerm")
                .AddTag(AzureTerraformTelemetryTags.ResourceType, options.ResourceType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving AzureRM documentation for {ResourceType}", options.ResourceType);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
