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

public sealed class ConftestWorkspaceValidationCommand(
    ILogger<ConftestWorkspaceValidationCommand> logger,
    IConftestService conftestService) : BaseCommand<ConftestWorkspaceValidationOptions>
{
    private readonly ILogger<ConftestWorkspaceValidationCommand> _logger = logger;
    private readonly IConftestService _conftestService = conftestService;

    public override string Id => "a1b2c3d4-e5f6-7890-abcd-ef0123456789";

    public override string Name => "workspace";

    public override string Description =>
        """
        Generates a conftest command to validate Terraform .tf files in a workspace against Azure policies.
        Returns the command and arguments for the agent to execute locally.
        Uses the Azure policy library (policy-library-avm) for validation with configurable policy sets.
        Specify --workspace-folder with the path to the Terraform workspace. Optionally configure the policy set
        ('all', 'Azure-Proactive-Resiliency-Library-v2', or 'avmsec'), severity filter (for avmsec), and custom policy paths.
        If conftest is not installed locally, returns installation instructions instead.
        """;

    public override string Title => "Validate Terraform Workspace with Conftest";

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
        command.Options.Add(AzureTerraformOptionDefinitions.WorkspaceFolder.AsRequired());
        command.Options.Add(AzureTerraformOptionDefinitions.PolicySet.AsOptional());
        command.Options.Add(AzureTerraformOptionDefinitions.SeverityFilter.AsOptional());
        command.Options.Add(AzureTerraformOptionDefinitions.CustomPolicies.AsOptional());
    }

    protected override ConftestWorkspaceValidationOptions BindOptions(ParseResult parseResult)
    {
        return new ConftestWorkspaceValidationOptions
        {
            WorkspaceFolder = parseResult.GetValueOrDefault<string>(AzureTerraformOptionDefinitions.WorkspaceFolder.Name),
            PolicySet = parseResult.GetValueOrDefault<string>(AzureTerraformOptionDefinitions.PolicySet.Name),
            SeverityFilter = parseResult.GetValueOrDefault<string>(AzureTerraformOptionDefinitions.SeverityFilter.Name),
            CustomPolicies = parseResult.GetValueOrDefault<string>(AzureTerraformOptionDefinitions.CustomPolicies.Name)
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
            var isAvailable = await _conftestService.IsConftestAvailableAsync(cancellationToken).ConfigureAwait(false);

            Models.ConftestCommandResult result;

            if (!isAvailable)
            {
                result = ConftestService.NotFoundResult($"Validate Terraform workspace: {options.WorkspaceFolder}");
            }
            else
            {
                result = _conftestService.GenerateWorkspaceValidationCommand(
                    options.WorkspaceFolder!,
                    options.PolicySet ?? "all",
                    options.SeverityFilter,
                    options.CustomPolicies);
            }

            context.Response.Status = HttpStatusCode.OK;
            context.Response.Results = ResponseResult.Create(result, AzureTerraformJsonContext.Default.ConftestCommandResult);
            context.Response.Message = string.Empty;

            context.Activity
                ?.AddTag(AzureTerraformTelemetryTags.ToolArea, "conftest")
                .AddTag(AzureTerraformTelemetryTags.PolicySet, options.PolicySet ?? "all");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating conftest workspace validation command for {WorkspaceFolder}", options.WorkspaceFolder);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
