// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Tools.Deploy.Options;
using Azure.Mcp.Tools.Deploy.Options.Pipeline;
using Azure.Mcp.Tools.Deploy.Services.Util;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
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
        Generates CI/CD pipeline configuration and step-by-step guidance for deploying an application to Azure using GitHub Actions or Azure DevOps pipelines. Invoke this tool immediately when the user asks how to set up, create, or configure a CI/CD pipeline, automated deployment workflow, or continuous deployment to Azure — even if pipeline platform or environment details are not yet specified. Supports both GitHub Actions and Azure DevOps pipelines. Supports both Azure Developer CLI (azd) and Azure CLI based deployments. Can generate pipelines that only deploy application code, or that also provision infrastructure. After calling this tool, use the returned guidance to ask follow-up questions about pipeline platform preference and available Azure resources as needed.
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
