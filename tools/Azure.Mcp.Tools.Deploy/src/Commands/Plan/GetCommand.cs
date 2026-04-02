// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Security.Cryptography;
using System.Text;
using Azure.Mcp.Tools.Deploy.Models;
using Azure.Mcp.Tools.Deploy.Options;
using Azure.Mcp.Tools.Deploy.Options.Plan;
using Azure.Mcp.Tools.Deploy.Services.Util;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.Deploy.Commands.Plan;

public sealed class GetCommand(ILogger<GetCommand> logger)
    : BaseCommand<GetOptions>
{
    private const string CommandTitle = "Generate Azure Deployment Plan";
    private readonly ILogger<GetCommand> _logger = logger;
    public override string Id => "92ca95b2-cde6-407c-ac67-9743db40dfc4";

    public override string Name => "get";

    public override string Description =>
        """
        Creates a deployment plan for deploying an application to Azure using the options provided by the caller. Use this tool when the user wants a formatted, step-by-step deployment plan (including suggested Azure resources, infrastructure as code (IaC) templates, and deployment instructions) based on a target Azure hosting service (for example, Container Apps, App Service, or AKS) and a chosen provisioning tool (such as Azure Developer CLI (azd) or Azure CLI with Bicep or Terraform). This command does not scan the workspace or automatically recommend Azure services. Instead, the caller or agent must first analyze the workspace, determine the services, frameworks, and dependencies to deploy, select the appropriate Azure hosting service, provisioning tool, IaC type, and deployment option, and then pass those chosen values into this tool to generate the deployment plan.
        """;

    public override string Title => CommandTitle;
    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(DeployOptionDefinitions.PlanGet.WorkspaceFolder);
        command.Options.Add(DeployOptionDefinitions.PlanGet.ProjectName);
        command.Options.Add(DeployOptionDefinitions.PlanGet.TargetAppService);
        command.Options.Add(DeployOptionDefinitions.PlanGet.ProvisioningTool);
        command.Options.Add(DeployOptionDefinitions.PlanGet.IacOptions);
        command.Options.Add(DeployOptionDefinitions.PlanGet.SourceType);
        command.Options.Add(DeployOptionDefinitions.PlanGet.DeployOption);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsOptional());
    }

    protected override GetOptions BindOptions(ParseResult parseResult)
    {
        return new GetOptions
        {
            WorkspaceFolder = parseResult.GetValueOrDefault<string>(DeployOptionDefinitions.PlanGet.WorkspaceFolder.Name) ?? string.Empty,
            ProjectName = parseResult.GetValueOrDefault<string>(DeployOptionDefinitions.PlanGet.ProjectName.Name) ?? string.Empty,
            TargetAppService = parseResult.GetValueOrDefault<string>(DeployOptionDefinitions.PlanGet.TargetAppService.Name) ?? string.Empty,
            ProvisioningTool = parseResult.GetValueOrDefault<string>(DeployOptionDefinitions.PlanGet.ProvisioningTool.Name) ?? string.Empty,
            IacOptions = parseResult.GetValueOrDefault<string>(DeployOptionDefinitions.PlanGet.IacOptions.Name) ?? string.Empty,
            SourceType = parseResult.GetValueOrDefault<string>(DeployOptionDefinitions.PlanGet.SourceType.Name) ?? string.Empty,
            DeployOption = parseResult.GetValueOrDefault<string>(DeployOptionDefinitions.PlanGet.DeployOption.Name) ?? string.Empty,
            ResourceGroup = parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name) ?? string.Empty,
        };
    }

    public override Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return Task.FromResult(context.Response);
        }

        var options = BindOptions(parseResult);

        try
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(options.ProjectName));
            context.Activity?.AddTag(DeployTelemetryTags.ProjectName, Convert.ToHexStringLower(bytes));
            context.Activity?
                    .AddTag(DeployTelemetryTags.ComputeHostResources, options.TargetAppService)
                    .AddTag(DeployTelemetryTags.DeploymentTool, options.ProvisioningTool)
                    .AddTag(DeployTelemetryTags.IacType, options.IacOptions ?? string.Empty)
                    .AddTag(DeployTelemetryTags.DeployOption, options.DeployOption ?? string.Empty)
                    .AddTag(DeployTelemetryTags.SourceType, options.SourceType ?? string.Empty);

            var planTemplate = DeploymentPlanTemplateUtil.GetPlanTemplate(options.ProjectName, options.TargetAppService, options.ProvisioningTool, options.SourceType ?? string.Empty, options.DeployOption ?? string.Empty, options.IacOptions, options.Subscription, options.ResourceGroup);

            context.Response.Message = planTemplate;
            context.Response.Status = HttpStatusCode.OK;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating deployment plan");
            HandleException(context, ex);
        }
        return Task.FromResult(context.Response);

    }

}
