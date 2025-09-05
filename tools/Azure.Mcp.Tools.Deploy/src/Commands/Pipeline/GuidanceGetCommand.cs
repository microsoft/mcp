// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
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

    private readonly Option<bool> _useAZDPipelineConfigOption = DeployOptionDefinitions.PipelineGenerateOptions.UseAZDPipelineConfig;
    private readonly Option<string> _organizationNameOption = DeployOptionDefinitions.PipelineGenerateOptions.OrganizationName;
    private readonly Option<string> _repositoryNameOption = DeployOptionDefinitions.PipelineGenerateOptions.RepositoryName;
    private readonly Option<string> _githubEnvironmentNameOption = DeployOptionDefinitions.PipelineGenerateOptions.GithubEnvironmentName;

    public override string Name => "get";

    public override string Description =>
        """
        Guidance to create a CI/CD pipeline which provision Azure resources and build and deploy applications to Azure. Use this tool BEFORE generating/creating a Github actions workflow file for DEPLOYMENT on Azure. Infrastructure files should be ready and the application should be ready to be containerized.
        """;

    public override string Title => CommandTitle;
    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(_useAZDPipelineConfigOption);
        command.Options.Add(_organizationNameOption);
        command.Options.Add(_repositoryNameOption);
        command.Options.Add(_githubEnvironmentNameOption);
    }

    protected override GuidanceGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.UseAZDPipelineConfig = parseResult.GetValue(_useAZDPipelineConfigOption);
        options.OrganizationName = parseResult.GetValue(_organizationNameOption);
        options.RepositoryName = parseResult.GetValue(_repositoryNameOption);
        options.GithubEnvironmentName = parseResult.GetValue(_githubEnvironmentNameOption);
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
