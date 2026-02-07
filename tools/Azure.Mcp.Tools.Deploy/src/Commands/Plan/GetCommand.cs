// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Security.Cryptography;
using System.Text;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Deploy.Models;
using Azure.Mcp.Tools.Deploy.Options;
using Azure.Mcp.Tools.Deploy.Options.Plan;
using Azure.Mcp.Tools.Deploy.Services.Util;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

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
        Entry point to help the agent deploy a service to the cloud. Agent should read its output and generate a deploy plan in '.azure/plan.${COPILOT_MD_EXTENSION}' for execution steps, recommended Azure services based on the information agent detected from project. Before calling this tool, scan this workspace to detect the services to deploy and their dependent services. If user has existing resources and only wants to deploy to existing resources, agent MUST first help user to pick existing Azure resources's ARM ID with Az CLI command or prompt user to provide!
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
        command.Options.Add(DeployOptionDefinitions.PlanGet.ResourceGroupName);
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
            ResourceGroupName = parseResult.GetValueOrDefault<string>(DeployOptionDefinitions.PlanGet.ResourceGroupName.Name) ?? string.Empty
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
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(options.ProjectName));
                context.Activity?.AddTag(DeployTelemetryTags.ProjectName, BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant());
            }
            context.Activity?
                    .AddTag(DeployTelemetryTags.ComputeHostResources, options.TargetAppService)
                    .AddTag(DeployTelemetryTags.DeploymentTool, options.ProvisioningTool)
                    .AddTag(DeployTelemetryTags.IacType, options.IacOptions ?? string.Empty)
                    .AddTag(DeployTelemetryTags.DeployOption, options.DeployOption ?? string.Empty)
                    .AddTag(DeployTelemetryTags.SourceType, options.SourceType ?? string.Empty);

            var planTemplate = DeploymentPlanTemplateUtil.GetPlanTemplate(options.ProjectName, options.TargetAppService, options.ProvisioningTool, options.SourceType ?? string.Empty, options.DeployOption ?? string.Empty, options.IacOptions, options.Subscription, options.ResourceGroupName);

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
