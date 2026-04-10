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
        Generates CI/CD pipeline configuration and step-by-step guidance for deploying an application to Azure using GitHub Actions or Azure DevOps pipelines. Use this tool when the user wants to create a CI/CD pipeline, set up automated deployment workflows, or configure pipeline files to deploy their application to Azure. Supports both Azure Developer CLI (azd) and Azure CLI based deployments, and can generate pipelines that provision infrastructure and deploy application code. Before calling this tool, confirm with the user whether they prefer GitHub Actions or Azure DevOps, and whether they have existing Azure resources for their deployment environments. Use when user asks: how do I set up a CI/CD pipeline with GitHub Actions or Azure DevOps to deploy my app to Azure?
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
        command.Options.Add(DeployOptionDefinitions.PipelineGenerateOptions.IsAZDProject);
        command.Options.Add(DeployOptionDefinitions.PipelineGenerateOptions.PipelinePlatform);
        command.Options.Add(DeployOptionDefinitions.PipelineGenerateOptions.DeployOption);
    }

    protected override GuidanceGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.IsAZDProject = parseResult.GetValueOrDefault<bool>(DeployOptionDefinitions.PipelineGenerateOptions.IsAZDProject.Name);
        options.PipelinePlatform = parseResult.GetValueOrDefault<string>(DeployOptionDefinitions.PipelineGenerateOptions.PipelinePlatform.Name);
        options.DeployOption = parseResult.GetValueOrDefault<string>(DeployOptionDefinitions.PipelineGenerateOptions.DeployOption.Name);
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
