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

public sealed class AztfexportResourceGroupCommand(
    ILogger<AztfexportResourceGroupCommand> logger,
    IAztfexportService aztfexportService) : BaseCommand<AztfexportResourceGroupOptions>
{
    private readonly ILogger<AztfexportResourceGroupCommand> _logger = logger;
    private readonly IAztfexportService _aztfexportService = aztfexportService;

    public override string Id => "a7b8c9d0-e1f2-3456-0123-678901234ef0";

    public override string Name => "resourcegroup";

    public override string Description =>
        """
        Generates an aztfexport command to export an Azure resource group and all its resources to Terraform configuration.
        Returns the command and arguments for the agent to execute locally.
        Specify --resource-group with the name of the resource group. Optionally configure the Terraform provider
        (azurerm or azapi), naming pattern, output folder, parallelism, and whether to include role assignments.
        If aztfexport is not installed locally, returns installation instructions instead.
        """;

    public override string Title => "Export Azure Resource Group to Terraform";

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
        command.Options.Add(AzureTerraformOptionDefinitions.ResourceGroupName.AsRequired());
        command.Options.Add(AzureTerraformOptionDefinitions.OutputFolderName.AsOptional());
        command.Options.Add(AzureTerraformOptionDefinitions.TerraformProvider.AsOptional());
        command.Options.Add(AzureTerraformOptionDefinitions.NamePattern.AsOptional());
        command.Options.Add(AzureTerraformOptionDefinitions.IncludeRoleAssignment.AsOptional());
        command.Options.Add(AzureTerraformOptionDefinitions.Parallelism.AsOptional());
        command.Options.Add(AzureTerraformOptionDefinitions.ContinueOnError.AsOptional());
    }

    protected override AztfexportResourceGroupOptions BindOptions(ParseResult parseResult)
    {
        return new AztfexportResourceGroupOptions
        {
            ResourceGroupName = parseResult.GetValueOrDefault<string>(AzureTerraformOptionDefinitions.ResourceGroupName.Name),
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
                result = AztfexportService.NotFoundResult($"Export Azure resource group: {options.ResourceGroupName}");
            }
            else
            {
                result = _aztfexportService.GenerateResourceGroupCommand(
                    options.ResourceGroupName!,
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
            _logger.LogError(ex, "Error generating aztfexport resource group command for {ResourceGroup}", options.ResourceGroupName);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
