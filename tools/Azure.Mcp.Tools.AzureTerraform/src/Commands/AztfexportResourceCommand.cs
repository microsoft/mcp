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
        Export one existing Azure resource to Terraform configuration by generating an Azure Export for Terraform
        (aztfexport) command. Use this tool whenever the request identifies the resource by its full Azure resource ID,
        such as /subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.Storage/storageAccounts/{name}.
        Exports only that single resource, not the resource group that contains it.
        Supports the AzureRM (azurerm, default) and AzAPI (azapi) providers;
        for an AzAPI export, set --provider to azapi.
        This tool returns the command and arguments for local execution; it does not execute the export or modify Azure resources.
        Requires --resource-id with the full Azure resource ID. Ask for the resource ID if it is not provided.
        Optionally configure a custom Terraform resource name, output folder, parallelism, and whether to include role assignments.
        To export every resource in a resource group by group name instead, use the aztfexport resourcegroup command.
        If aztfexport is not installed locally, returns installation instructions instead.
        """,
    OperationPlane = ToolOperationPlane.NotApplicable,
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
