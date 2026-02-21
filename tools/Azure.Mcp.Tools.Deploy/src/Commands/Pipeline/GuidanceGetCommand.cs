// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Deploy.Options;
using Azure.Mcp.Tools.Deploy.Options.Pipeline;
using Azure.Mcp.Tools.Deploy.Services.Util;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Deploy.Commands.Pipeline;

public sealed class GuidanceGetCommand(ILogger<GuidanceGetCommand> logger)
    : SubscriptionCommand<GuidanceGetOptions>()
{
    private const string CommandTitle = "Get Azure Deployment CICD Pipeline Guidance";
    private readonly ILogger<GuidanceGetCommand> _logger = logger;
    public override string Id => "8aec84f9-e884-4119-a386-53b7cfbe9e00";

    public override string Name => "get";

    public override string Description =>
        """
        This tool helps create a CI/CD pipeline to deploy a project to Azure. BEFORE calling this tool, you MUST confirm with user 1. if they want to use Github Actions or ADO pipeline 2. if they have existing Azure resources to deploy for different environments. If user has existing Azure resources, *ASK* for subscription ID, resource groups, environments, Azure hosting service TYPEs. If user does not have existing resources, *ASK* whether to include provision in the pipeline. *DO NOT call this tool UNTIL you made these confirmations with user*.
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
        command.Options.Add(DeployOptionDefinitions.PipelineGenerateOptions.isAZDProject);
        command.Options.Add(DeployOptionDefinitions.PipelineGenerateOptions.pipelinePlatform);
        command.Options.Add(DeployOptionDefinitions.PipelineGenerateOptions.deployOption);
    }

    protected override GuidanceGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.IsAZDProject = parseResult.GetValueOrDefault<bool>(DeployOptionDefinitions.PipelineGenerateOptions.isAZDProject.Name);
        options.PipelinePlatform = parseResult.GetValueOrDefault<string>(DeployOptionDefinitions.PipelineGenerateOptions.pipelinePlatform.Name);
        options.DeployOption = parseResult.GetValueOrDefault<string>(DeployOptionDefinitions.PipelineGenerateOptions.deployOption.Name);
        return options;
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
            var result = PipelineGenerationUtil.GeneratePipelineGuidelines(options);

            context.Response.Message = result;
            context.Response.Status = HttpStatusCode.OK;
        }
        catch (Exception ex)
        {
            HandleException(context, ex);
        }
        return Task.FromResult(context.Response);
    }

}
