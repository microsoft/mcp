// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AzureTerraform.Models;
using Azure.Mcp.Tools.AzureTerraform.Options.Conftest;
using Azure.Mcp.Tools.AzureTerraform.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AzureTerraform.Commands.Conftest;

[CommandMetadata(
    Id = "a1b2c3d4-e5f6-7890-abcd-ef0123456789",
    Name = "workspace",
    Title = "Validate Terraform Workspace with Conftest",
    Description = """
        Generates a conftest command to validate Terraform .tf files in a workspace against Azure policies.
        Returns the command and arguments for the agent to execute locally.
        Uses the Azure policy library (policy-library-avm) for validation with configurable policy sets.
        Specify --workspace-folder with the path to the Terraform workspace. Optionally configure the policy set
        ('all', 'Azure-Proactive-Resiliency-Library-v2', or 'avmsec'), severity filter (for avmsec), and custom policy paths.
        If conftest is not installed locally, returns installation instructions instead.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = true,
    ReadOnly = true,
    Secret = false,
    LocalRequired = true)]
public sealed class ConftestWorkspaceValidationCommand(
    ILogger<ConftestWorkspaceValidationCommand> logger,
    IConftestService conftestService) : BaseCommand<ConftestWorkspaceValidationOptions, ConftestCommandResult>
{
    private readonly ILogger<ConftestWorkspaceValidationCommand> _logger = logger;
    private readonly IConftestService _conftestService = conftestService;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        ConftestWorkspaceValidationOptions options,
        CancellationToken cancellationToken)
    {

        try
        {
            var isAvailable = await _conftestService.IsConftestAvailableAsync(cancellationToken).ConfigureAwait(false);

            ConftestCommandResult result;
            if (!isAvailable)
            {
                result = ConftestService.NotFoundResult($"Validate Terraform workspace: {options.WorkspaceFolder}");
            }
            else
            {
                result = _conftestService.GenerateWorkspaceValidationCommand(
                    options.WorkspaceFolder,
                    options.PolicySet ?? "all",
                    options.SeverityFilter,
                    options.CustomPolicies);
            }

            context.Response.Results = ResponseResult.Create(result, AzureTerraformJsonContext.Default.ConftestCommandResult);

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
