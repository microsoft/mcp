// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.AzureTerraform.Models;
using Azure.Mcp.Tools.AzureTerraform.Options;
using Azure.Mcp.Tools.AzureTerraform.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AzureTerraform.Commands;

[CommandMetadata(
    Id = "f6a7b8c9-d0e1-2345-bcde-678901234ef0",
    Name = "get",
    Title = "Get AzAPI Provider Documentation",
    Description = """
        Retrieves AzAPI Terraform provider documentation and schema for a specified Azure resource type.
        Returns the resource schema in HCL format suitable for azapi_resource blocks, including property
        definitions with types and requirements, parent resource information, and Terraform usage examples.
        Use --resource-type to specify the Azure resource type in ARM format
        (e.g., Microsoft.Compute/virtualMachines, Microsoft.Storage/storageAccounts).
        Optionally specify --api-version to target a specific API version.
        This tool reuses Azure Bicep type definitions to generate accurate AzAPI schemas.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = true,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class AzApiDocsGetCommand(
    ILogger<AzApiDocsGetCommand> logger,
    IAzApiDocsService docsService,
    IAzApiExamplesService examplesService) : BaseCommand<AzApiDocsOptions, AzApiDocsResult>
{
    private readonly ILogger<AzApiDocsGetCommand> _logger = logger;
    private readonly IAzApiDocsService _docsService = docsService;
    private readonly IAzApiExamplesService _examplesService = examplesService;

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        InvalidDataException => HttpStatusCode.BadRequest,
        _ => base.GetStatusCode(ex)
    };

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        AzApiDocsOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = _docsService.GetDocumentation(options.ResourceType, options.ApiVersion);

            var examples = await _examplesService.GetExamplesAsync(
                options.ResourceType,
                cancellationToken).ConfigureAwait(false);

            if (examples.Count > 0)
            {
                result.Examples = examples;
            }

            context.Response.Results = ResponseResult.Create(result, AzureTerraformJsonContext.Default.AzApiDocsResult);

            context.Activity
                ?.AddTag(AzureTerraformTelemetryTags.ToolArea, "azapi")
                .AddTag(AzureTerraformTelemetryTags.ResourceType, options.ResourceType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving AzAPI documentation for {ResourceType}", options.ResourceType);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
