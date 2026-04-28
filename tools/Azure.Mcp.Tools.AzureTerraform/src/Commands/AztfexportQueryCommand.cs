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

public sealed class AztfexportQueryCommand(
    ILogger<AztfexportQueryCommand> logger,
    IAztfexportService aztfexportService) : BaseCommand<AztfexportQueryOptions>
{
    private readonly ILogger<AztfexportQueryCommand> _logger = logger;
    private readonly IAztfexportService _aztfexportService = aztfexportService;

    public override string Id => "b8c9d0e1-f2a3-4567-1234-789012345f01";

    public override string Name => "query";

    public override string Description =>
        """
        Generates an aztfexport command to export Azure resources matching an Azure Resource Graph query
        to Terraform configuration. Returns the command and arguments for the agent to execute locally.
        Specify --query with a KQL WHERE clause for Azure Resource Graph (e.g., "type =~ 'Microsoft.Storage/storageAccounts'").
        Optionally configure the Terraform provider (azurerm or azapi), naming pattern, output folder,
        parallelism, and whether to include role assignments.
        If aztfexport is not installed locally, returns installation instructions instead.
        """;

    public override string Title => "Export Azure Resources by Query to Terraform";

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = true,
        ReadOnly = true,
        LocalRequired = true,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(AzureTerraformOptionDefinitions.AzureResourceGraphQuery.AsRequired());
        command.Options.Add(AzureTerraformOptionDefinitions.OutputFolderName.AsOptional());
        command.Options.Add(AzureTerraformOptionDefinitions.TerraformProvider.AsOptional());
        command.Options.Add(AzureTerraformOptionDefinitions.NamePattern.AsOptional());
        command.Options.Add(AzureTerraformOptionDefinitions.IncludeRoleAssignment.AsOptional());
        command.Options.Add(AzureTerraformOptionDefinitions.Parallelism.AsOptional());
        command.Options.Add(AzureTerraformOptionDefinitions.ContinueOnError.AsOptional());
    }

    protected override AztfexportQueryOptions BindOptions(ParseResult parseResult)
    {
        return new AztfexportQueryOptions
        {
            Query = parseResult.GetValueOrDefault<string>(AzureTerraformOptionDefinitions.AzureResourceGraphQuery.Name),
            OutputFolderName = parseResult.GetValueOrDefault<string>(AzureTerraformOptionDefinitions.OutputFolderName.Name),
            Provider = parseResult.GetValueOrDefault<string>(AzureTerraformOptionDefinitions.TerraformProvider.Name),
            NamePattern = parseResult.GetValueOrDefault<string>(AzureTerraformOptionDefinitions.NamePattern.Name),
            IncludeRoleAssignment = parseResult.GetValueOrDefault<bool>(AzureTerraformOptionDefinitions.IncludeRoleAssignment.Name),
            Parallelism = parseResult.GetValueOrDefault<int>(AzureTerraformOptionDefinitions.Parallelism.Name),
            ContinueOnError = parseResult.GetValueOrDefault<bool>(AzureTerraformOptionDefinitions.ContinueOnError.Name)
        };
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        ParseResult parseResult,
        CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var isAvailable = await _aztfexportService.IsAztfexportAvailableAsync(cancellationToken).ConfigureAwait(false);

            Models.AztfexportCommandResult result;

            if (!isAvailable)
            {
                result = AztfexportService.NotFoundResult($"Export Azure resources by query: {options.Query}");
            }
            else
            {
                result = _aztfexportService.GenerateQueryCommand(
                    options.Query!,
                    options.OutputFolderName,
                    options.Provider ?? "azurerm",
                    options.NamePattern,
                    options.IncludeRoleAssignment,
                    options.Parallelism > 0 ? options.Parallelism : 10,
                    options.ContinueOnError);
            }

            context.Response.Status = HttpStatusCode.OK;
            context.Response.Results = ResponseResult.Create(result, AzureTerraformJsonContext.Default.AztfexportCommandResult);
            context.Response.Message = string.Empty;

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
