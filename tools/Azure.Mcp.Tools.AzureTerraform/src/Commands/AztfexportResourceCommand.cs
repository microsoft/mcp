// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.AzureTerraform.Options;
using Azure.Mcp.Tools.AzureTerraform.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AzureTerraform.Commands;

public sealed class AztfexportResourceCommand(
    ILogger<AztfexportResourceCommand> logger,
    IAztfexportService aztfexportService) : BaseCommand<AztfexportResourceOptions>
{
    private readonly ILogger<AztfexportResourceCommand> _logger = logger;
    private readonly IAztfexportService _aztfexportService = aztfexportService;

    public override string Id => "f6a7b8c9-d0e1-2345-f012-567890123def";

    public override string Name => "resource";

    public override string Description =>
        """
        Generates an aztfexport command to export a single Azure resource to Terraform configuration.
        Returns the command and arguments for the agent to execute locally.
        Specify --resource-id with the full Azure resource ID. Optionally configure the Terraform provider
        (azurerm or azapi), custom resource name, output folder, parallelism, and whether to include role assignments.
        If aztfexport is not installed locally, returns installation instructions instead.
        """;

    public override string Title => "Export Azure Resource to Terraform";

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
        command.Options.Add(AzureTerraformOptionDefinitions.ResourceId);
        command.Options.Add(AzureTerraformOptionDefinitions.OutputFolderName);
        command.Options.Add(AzureTerraformOptionDefinitions.TerraformProvider);
        command.Options.Add(AzureTerraformOptionDefinitions.TerraformResourceName);
        command.Options.Add(AzureTerraformOptionDefinitions.IncludeRoleAssignment);
        command.Options.Add(AzureTerraformOptionDefinitions.Parallelism);
        command.Options.Add(AzureTerraformOptionDefinitions.ContinueOnError);
    }

    protected override AztfexportResourceOptions BindOptions(ParseResult parseResult)
    {
        return new AztfexportResourceOptions
        {
            ResourceId = parseResult.GetValueOrDefault<string>(AzureTerraformOptionDefinitions.ResourceId.Name),
            OutputFolderName = parseResult.GetValueOrDefault<string>(AzureTerraformOptionDefinitions.OutputFolderName.Name),
            Provider = parseResult.GetValueOrDefault<string>(AzureTerraformOptionDefinitions.TerraformProvider.Name),
            ResourceName = parseResult.GetValueOrDefault<string>(AzureTerraformOptionDefinitions.TerraformResourceName.Name),
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
                result = AztfexportService.NotFoundResult($"Export Azure resource: {options.ResourceId}");
            }
            else
            {
                result = _aztfexportService.GenerateResourceCommand(
                    options.ResourceId!,
                    options.OutputFolderName,
                    options.Provider ?? "azurerm",
                    options.ResourceName,
                    options.IncludeRoleAssignment,
                    options.Parallelism > 0 ? options.Parallelism : 10,
                    options.ContinueOnError);
            }

            context.Response.Status = HttpStatusCode.OK;
            context.Response.Results = ResponseResult.Create(result, AzureTerraformJsonContext.Default.AztfexportCommandResult);
            context.Response.Message = string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating aztfexport resource command for {ResourceId}", options.ResourceId);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
