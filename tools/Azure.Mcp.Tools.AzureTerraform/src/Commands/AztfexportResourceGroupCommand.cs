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
    Id = "a7b8c9d0-e1f2-3456-0123-678901234ef0",
    Name = "resourcegroup",
    Title = "Export Azure Resource Group to Terraform",
    Description = """
        Generates an aztfexport command to export an Azure resource group and all its resources to Terraform configuration.
        Returns the command and arguments for the agent to execute locally.
        Specify --resource-group with the name of the resource group. Optionally configure the Terraform provider
        (azurerm or azapi), naming pattern, output folder, parallelism, and whether to include role assignments.
        If aztfexport is not installed locally, returns installation instructions instead.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = true,
    ReadOnly = true,
    Secret = false,
    LocalRequired = true)]
public sealed class AztfexportResourceGroupCommand(
    ILogger<AztfexportResourceGroupCommand> logger,
    IAztfexportService aztfexportService) : BaseCommand<AztfexportResourceGroupOptions, AztfexportCommandResult>
{
    private readonly ILogger<AztfexportResourceGroupCommand> _logger = logger;
    private readonly IAztfexportService _aztfexportService = aztfexportService;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        AztfexportResourceGroupOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            var isAvailable = await _aztfexportService.IsAztfexportAvailableAsync(cancellationToken).ConfigureAwait(false);

            AztfexportCommandResult result;

            if (!isAvailable)
            {
                result = AztfexportService.NotFoundResult($"Export Azure resource group: {options.ResourceGroup}");
            }
            else
            {
                result = _aztfexportService.GenerateResourceGroupCommand(
                    options.ResourceGroup,
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
            _logger.LogError(ex, "Error generating aztfexport resource group command for {ResourceGroup}", options.ResourceGroup);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
