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

public sealed class ConftestPlanValidationCommand(
    ILogger<ConftestPlanValidationCommand> logger,
    IConftestService conftestService) : BaseCommand<ConftestPlanValidationOptions>
{
    private readonly ILogger<ConftestPlanValidationCommand> _logger = logger;
    private readonly IConftestService _conftestService = conftestService;

    public override string Id => "b2c3d4e5-f6a7-8901-bcde-f01234567890";

    public override string Name => "plan";

    public override string Description =>
        """
        Generates a conftest command to validate a Terraform plan JSON file against Azure policies.
        Returns the command and arguments for the agent to execute locally.
        Uses the Azure policy library (policy-library-avm) for validation with configurable policy sets.
        Specify --plan-folder with the path to the folder containing tfplan.json. Optionally configure the policy set
        ('all', 'Azure-Proactive-Resiliency-Library-v2', or 'avmsec'), severity filter (for avmsec), and custom policy paths.
        If conftest is not installed locally, returns installation instructions instead.
        """;

    public override string Title => "Validate Terraform Plan with Conftest";

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
        command.Options.Add(AzureTerraformOptionDefinitions.PlanFolder.AsRequired());
        command.Options.Add(AzureTerraformOptionDefinitions.PolicySet.AsOptional());
        command.Options.Add(AzureTerraformOptionDefinitions.SeverityFilter.AsOptional());
        command.Options.Add(AzureTerraformOptionDefinitions.CustomPolicies.AsOptional());
    }

    protected override ConftestPlanValidationOptions BindOptions(ParseResult parseResult)
    {
        return new ConftestPlanValidationOptions
        {
            PlanFolder = parseResult.GetValueOrDefault<string>(AzureTerraformOptionDefinitions.PlanFolder.Name),
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
                result = ConftestService.NotFoundResult($"Validate Terraform plan in: {options.PlanFolder}");
            }
            else
            {
                result = _conftestService.GeneratePlanValidationCommand(
                    options.PlanFolder!,
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
            _logger.LogError(ex, "Error generating conftest plan validation command for {PlanFolder}", options.PlanFolder);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
