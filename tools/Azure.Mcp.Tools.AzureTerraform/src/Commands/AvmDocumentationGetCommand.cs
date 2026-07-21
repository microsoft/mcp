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
    Id = "e5f6a7b8-c9d0-1234-ef01-456789012cde",
    Name = "get",
    Title = "Get AVM Module Documentation",
    Description = """
        Retrieves the documentation (README.md) for a specific version of an Azure Verified Module (AVM).
        Works for both resource modules (avm-res-*) and pattern modules (avm-ptn-*).
        Returns the full module documentation including usage examples, input variables,
        output values, and resource descriptions. Use --module-name and --module-version
        to specify the module and version (e.g., --module-name avm-res-storage-storageaccount --module-version 0.4.0).
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = true,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class AvmDocumentationGetCommand(
    ILogger<AvmDocumentationGetCommand> logger,
    IAvmDocsService avmDocsService) : BaseCommand<AvmDocumentationOptions, AvmDocumentationResult>
{
    private readonly ILogger<AvmDocumentationGetCommand> _logger = logger;
    private readonly IAvmDocsService _avmDocsService = avmDocsService;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        AvmDocumentationOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            var documentation = await _avmDocsService.GetDocumentationAsync(
                options.ModuleName,
                options.ModuleVersion,
                cancellationToken).ConfigureAwait(false);

            context.Response.Results = ResponseResult.Create(
                new(options.ModuleName, options.ModuleVersion, documentation),
                AzureTerraformJsonContext.Default.AvmDocumentationResult);

            context.Activity
                ?.AddTag(AzureTerraformTelemetryTags.ToolArea, "avm")
                .AddTag(AzureTerraformTelemetryTags.ModuleName, options.ModuleName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving documentation for AVM module {ModuleName} version {Version}", options.ModuleName, options.ModuleVersion);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
