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
    Id = "d4e5f6a7-b8c9-0123-def0-345678901bcd",
    Name = "versions",
    Title = "List AVM Module Versions",
    Description = """
        Retrieves all available versions of a specified Azure Verified Module (AVM).
        Works for both resource modules (avm-res-*) and pattern modules (avm-ptn-*).
        Returns version tags with creation dates, sorted from newest to oldest.
        The first version in the list is the latest. Use --module-name to specify
        the module (e.g., avm-res-storage-storageaccount or avm-ptn-aiml-ai-foundry).
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = true,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class AvmVersionListCommand(
    ILogger<AvmVersionListCommand> logger,
    IAvmDocsService avmDocsService) : BaseCommand<AvmVersionOptions, AvmVersionListResult>
{
    private readonly ILogger<AvmVersionListCommand> _logger = logger;
    private readonly IAvmDocsService _avmDocsService = avmDocsService;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        AvmVersionOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            var versions = await _avmDocsService.GetVersionsAsync(
                options.ModuleName,
                cancellationToken).ConfigureAwait(false);

            context.Response.Results = ResponseResult.Create(
                new(options.ModuleName, versions),
                AzureTerraformJsonContext.Default.AvmVersionListResult);

            context.Activity
                ?.AddTag(AzureTerraformTelemetryTags.ToolArea, "avm")
                .AddTag(AzureTerraformTelemetryTags.ModuleName, options.ModuleName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving versions for AVM module {ModuleName}", options.ModuleName);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
