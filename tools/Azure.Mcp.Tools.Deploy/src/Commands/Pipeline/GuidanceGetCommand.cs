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
    private const string CommandTitle = "Get Azure Deployment CI/CD Pipeline Guidance";
    private readonly ILogger<GuidanceGetCommand> _logger = logger;
    public override string Id => "8aec84f9-e884-4119-a386-53b7cfbe9e00";

    public override string Name => "get";

    public override string Description =>
        """
        Provides the recommended Azure-specific rules for generating CI/CD pipeline files (GitHub Actions or Azure DevOps) for deploying to Azure. Call this tool before writing pipeline/workflow YAML when the user asks to set up CI/CD pipelines or workflows — it returns current authentication patterns (e.g., OIDC with managed identity), multi-environment configuration, and deployment constraints that vary by platform and are not covered by general best practices. Determine the pipeline platform (github-actions or azure-devops) and deployment scope (deploy-only or provision-and-deploy) from project context before calling. Handles both azd-based and Azure CLI-based deployments.
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
