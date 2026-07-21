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
    Id = "b8c9d0e1-f2a3-4567-1234-789012345f01",
    Name = "query",
    Title = "Export Azure Resources by Query to Terraform",
    Description = """
        Generates an aztfexport command to export Azure resources matching an Azure Resource Graph query
        to Terraform configuration. Returns the command and arguments for the agent to execute locally.
        Specify --query with a KQL WHERE clause for Azure Resource Graph (e.g., "type =~ 'Microsoft.Storage/storageAccounts'").
        Optionally configure the Terraform provider (azurerm or azapi), naming pattern, output folder,
        parallelism, and whether to include role assignments.
        If aztfexport is not installed locally, returns installation instructions instead.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = true,
    ReadOnly = true,
    Secret = false,
    LocalRequired = true)]
public sealed class AztfexportQueryCommand(
    ILogger<AztfexportQueryCommand> logger,
    IAztfexportService aztfexportService) : BaseCommand<AztfexportQueryOptions, AztfexportCommandResult>
{
    private readonly ILogger<AztfexportQueryCommand> _logger = logger;
    private readonly IAztfexportService _aztfexportService = aztfexportService;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        AztfexportQueryOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            var isAvailable = await _aztfexportService.IsAztfexportAvailableAsync(cancellationToken).ConfigureAwait(false);

            AztfexportCommandResult result;

            if (!isAvailable)
            {
                result = AztfexportService.NotFoundResult($"Export Azure resources by query: {options.Query}");
            }
            else
            {
                result = _aztfexportService.GenerateQueryCommand(
                    options.Query,
                    options.OutputFolder,
                    options.Provider ?? "azurerm",
                    options.NamePattern,
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
            _logger.LogError(ex, "Error generating aztfexport query command for query: {Query}", options.Query);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
