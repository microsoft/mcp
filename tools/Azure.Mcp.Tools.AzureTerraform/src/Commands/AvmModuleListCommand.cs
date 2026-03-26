// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.AzureTerraform.Options;
using Azure.Mcp.Tools.AzureTerraform.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AzureTerraform.Commands;

public sealed class AvmModuleListCommand(
    ILogger<AvmModuleListCommand> logger,
    IAvmDocsService avmDocsService) : BaseCommand<AvmModuleListOptions>
{
    private readonly ILogger<AvmModuleListCommand> _logger = logger;
    private readonly IAvmDocsService _avmDocsService = avmDocsService;

    public override string Id => "c3d4e5f6-a7b8-9012-cdef-234567890abc";

    public override string Name => "list";

    public override string Description =>
        """
        Retrieves all available Azure Verified Modules (AVM) for Terraform.
        Returns a list of modules with their name, description, source reference, and repository URL.
        The source field can be used directly in Terraform module blocks.
        """;

    public override string Title => "List AVM Modules";

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = true,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    protected override AvmModuleListOptions BindOptions(ParseResult parseResult) => new();

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        ParseResult parseResult,
        CancellationToken cancellationToken)
    {
        try
        {
            var modules = await _avmDocsService.ListModulesAsync(cancellationToken).ConfigureAwait(false);

            var result = new Models.AvmModuleListResult { Modules = modules };
            context.Response.Status = HttpStatusCode.OK;
            context.Response.Results = ResponseResult.Create(result, AzureTerraformJsonContext.Default.AvmModuleListResult);
            context.Response.Message = string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing AVM modules");
            HandleException(context, ex);
        }

        return context.Response;
    }
}
