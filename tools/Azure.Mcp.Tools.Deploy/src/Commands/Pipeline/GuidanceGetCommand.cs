// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Deploy.Options;
using Azure.Mcp.Tools.Deploy.Options.Pipeline;
using Azure.Mcp.Tools.Deploy.Services.Util;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Deploy.Commands.Pipeline;

public sealed class GuidanceGetCommand(ILogger<GuidanceGetCommand> logger)
    : SubscriptionCommand<GuidanceGetOptions>()
{
    private const string CommandTitle = "Get Azure Deployment CICD Pipeline Guidance";
    private readonly ILogger<GuidanceGetCommand> _logger = logger;

    public override string Name => "get";

    public override string Description =>
        """
        Guidance to create a CI/CD pipeline which provision Azure resources and build and deploy applications to Azure. Use this tool BEFORE generating/creating a Github actions workflow file for DEPLOYMENT on Azure. Infrastructure files should be ready and the application should be ready to be containerized.
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
        command.Options.Add(DeployOptionDefinitions.PipelineGenerateOptions.UseAZDPipelineConfig);
        command.Options.Add(DeployOptionDefinitions.PipelineGenerateOptions.OrganizationName);
        command.Options.Add(DeployOptionDefinitions.PipelineGenerateOptions.RepositoryName);
        command.Options.Add(DeployOptionDefinitions.PipelineGenerateOptions.GithubEnvironmentName);
    }

    protected override GuidanceGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.UseAZDPipelineConfig = parseResult.GetValueOrDefault<bool>(DeployOptionDefinitions.PipelineGenerateOptions.UseAZDPipelineConfig.Name);
        options.OrganizationName = parseResult.GetValueOrDefault<string>(DeployOptionDefinitions.PipelineGenerateOptions.OrganizationName.Name);
        options.RepositoryName = parseResult.GetValueOrDefault<string>(DeployOptionDefinitions.PipelineGenerateOptions.RepositoryName.Name);
        options.GithubEnvironmentName = parseResult.GetValueOrDefault<string>(DeployOptionDefinitions.PipelineGenerateOptions.GithubEnvironmentName.Name);
        return options;
    }

    public override Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
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
            context.Response.Status = 200;
        }
        catch (Exception ex)
        {
            HandleException(context, ex);
        }
        return Task.FromResult(context.Response);
    }

}
