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
    Id = "f6a7b8c9-d0e1-2345-f012-567890123def",
    Name = "resource",
    Title = "Export Azure Resource to Terraform",
    Description = """
        Generates an aztfexport command to export a single Azure resource to Terraform configuration.
        Returns the command and arguments for the agent to execute locally.
        Specify --resource-id with the full Azure resource ID. Optionally configure the Terraform provider
        (azurerm or azapi), custom resource name, output folder, parallelism, and whether to include role assignments.
        If aztfexport is not installed locally, returns installation instructions instead.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = true,
    ReadOnly = true,
    Secret = false,
    LocalRequired = true)]
public sealed class AztfexportResourceCommand(
    ILogger<AztfexportResourceCommand> logger,
    IAztfexportService aztfexportService) : BaseCommand<AztfexportResourceOptions, AztfexportCommandResult>
{
    private readonly ILogger<AztfexportResourceCommand> _logger = logger;
    private readonly IAztfexportService _aztfexportService = aztfexportService;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        AztfexportResourceOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            var isAvailable = await _aztfexportService.IsAztfexportAvailableAsync(cancellationToken).ConfigureAwait(false);

            AztfexportCommandResult result;

            if (!isAvailable)
            {
                result = AztfexportService.NotFoundResult($"Export Azure resource: {options.ResourceId}");
            }
            else
            {
                result = _aztfexportService.GenerateResourceCommand(
                    options.ResourceId,
                    options.OutputFolder,
                    options.Provider ?? "azurerm",
                    options.TerraformResourceName,
                    options.IncludeRoleAssignment,
                    options.Parallelism > 0 ? options.Parallelism : 10,
                    options.ContinueOnError);
            }

            context.Response.Results = ResponseResult.Create(result, AzureTerraformJsonContext.Default.AztfexportCommandResult);

            context.Activity
                ?.AddTag(AzureTerraformTelemetryTags.ToolArea, "aztfexport")
                .AddTag(AzureTerraformTelemetryTags.Provider, options.Provider ?? "azurerm");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating aztfexport resource command for {ResourceId}", options.ResourceId);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
