// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AzureTerraform.Models;
using Azure.Mcp.Tools.AzureTerraform.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AzureTerraform.Commands;

[CommandMetadata(
    Id = "c3d4e5f6-a7b8-9012-cdef-234567890abc",
    Name = "list",
    Title = "List AVM Modules",
    Description = """
        Retrieves all available Azure Verified Modules (AVM) for Terraform, including both
        resource modules (avm-res-*) and pattern modules (avm-ptn-*).
        Returns a list of modules with their name, description, source reference, repository URL,
        and moduleType ('resource' or 'pattern').
        The source field can be used directly in Terraform module blocks.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = true,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class AvmModuleListCommand(ILogger<AvmModuleListCommand> logger, IAvmDocsService avmDocsService)
    : BaseCommand<EmptyOptions, AvmModuleListResult>
{
    private readonly ILogger<AvmModuleListCommand> _logger = logger;
    private readonly IAvmDocsService _avmDocsService = avmDocsService;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        EmptyOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            var modules = await _avmDocsService.ListModulesAsync(cancellationToken).ConfigureAwait(false);

            context.Response.Results = ResponseResult.Create(new(modules), AzureTerraformJsonContext.Default.AvmModuleListResult);

            context.Activity?.AddTag(AzureTerraformTelemetryTags.ToolArea, "avm");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing AVM modules");
            HandleException(context, ex);
        }

        return context.Response;
    }
}
