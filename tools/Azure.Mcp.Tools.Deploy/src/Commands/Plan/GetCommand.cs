// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Deploy.Models;
using Azure.Mcp.Tools.Deploy.Options;
using Azure.Mcp.Tools.Deploy.Options.Plan;
using Azure.Mcp.Tools.Deploy.Services.Util;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Deploy.Commands.Plan;

public sealed class GetCommand(ILogger<GetCommand> logger)
    : BaseCommand()
{
    private const string CommandTitle = "Generate Azure Deployment Plan";
    private readonly ILogger<GetCommand> _logger = logger;

    private readonly Option<string> _workspaceFolderOption = DeployOptionDefinitions.PlanGet.WorkspaceFolder;
    private readonly Option<string> _projectNameOption = DeployOptionDefinitions.PlanGet.ProjectName;
    private readonly Option<string> _deploymentTargetServiceOption = DeployOptionDefinitions.PlanGet.TargetAppService;
    private readonly Option<string> _provisioningToolOption = DeployOptionDefinitions.PlanGet.ProvisioningTool;
    private readonly Option<string> _azdIacOptionsOption = DeployOptionDefinitions.PlanGet.AzdIacOptions;

    public override string Name => "get";

    public override string Description =>
        """
        Generates a deployment plan to construct the infrastructure and deploy the application on Azure. Agent should read its output and generate a deploy plan in '.azure/plan.copilotmd' for execution steps, recommended azure services based on the information agent detected from project. Before calling this tool, please scan this workspace to detect the services to deploy and their dependent services.
        """;

    public override string Title => CommandTitle;
    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_workspaceFolderOption);
        command.AddOption(_projectNameOption);
        command.AddOption(_deploymentTargetServiceOption);
        command.AddOption(_provisioningToolOption);
        command.AddOption(_azdIacOptionsOption);
    }

    private GetOptions BindOptions(ParseResult parseResult)
    {
        return new GetOptions
        {
            WorkspaceFolder = parseResult.GetValueForOption(_workspaceFolderOption) ?? string.Empty,
            ProjectName = parseResult.GetValueForOption(_projectNameOption) ?? string.Empty,
            TargetAppService = parseResult.GetValueForOption(_deploymentTargetServiceOption) ?? string.Empty,
            ProvisioningTool = parseResult.GetValueForOption(_provisioningToolOption) ?? string.Empty,
            AzdIacOptions = parseResult.GetValueForOption(_azdIacOptionsOption) ?? string.Empty
        };
    }

    public override Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var options = BindOptions(parseResult);

        try
        {
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            {
                return Task.FromResult(context.Response);
            }

            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(options.ProjectName));
                context.Activity?.AddTag(DeployTelemetryTags.ProjectName, BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant());
            }
            context.Activity?
                    .AddTag(DeployTelemetryTags.ComputeHostResources, options.TargetAppService)
                    .AddTag(DeployTelemetryTags.DeploymentTool, options.ProvisioningTool)
                    .AddTag(DeployTelemetryTags.IacType, options.AzdIacOptions ?? string.Empty);

            var planTemplate = DeploymentPlanTemplateUtil.GetPlanTemplate(options.ProjectName, options.TargetAppService, options.ProvisioningTool, options.AzdIacOptions);

            context.Response.Message = planTemplate;
            context.Response.Status = 200;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating deployment plan");
            HandleException(context, ex);
        }
        return Task.FromResult(context.Response);

    }

}
