// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Deploy.Models;
using Azure.Mcp.Tools.Deploy.Options;
using Azure.Mcp.Tools.Deploy.Options.Infrastructure;
using Azure.Mcp.Tools.Deploy.Services.Util;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Deploy.Commands.Infrastructure;

public sealed class RulesGetCommand(ILogger<RulesGetCommand> logger)
    : BaseCommand<RulesGetOptions>
{
    private const string CommandTitle = "Get Iac(Infrastructure as Code) Rules";
    private readonly ILogger<RulesGetCommand> _logger = logger;
    public override string Id => "942b5c00-01dd-4ca0-9596-4cf650ff7934";

    public override string Name => "get";
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

    public override string Description =>
        """
        Retrieves rules and best practices for creating Bicep and Terraform Infrastructure as Code (IaC) files to deploy Azure applications. Use this tool when the user asks for rules, guidelines, or best practices for writing Bicep scripts or Terraform templates for Azure resources. The rules cover Azure resource configuration standards, compatibility with Azure Developer CLI (azd) and Azure CLI, and general IaC quality requirements. Use when user asks: show me the rules and best practices for writing Bicep and Terraform IaC for Azure.
        """;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(DeployOptionDefinitions.IaCRules.DeploymentTool);
        command.Options.Add(DeployOptionDefinitions.IaCRules.IacType);
        command.Options.Add(DeployOptionDefinitions.IaCRules.ResourceTypes);
    }

    protected override RulesGetOptions BindOptions(ParseResult parseResult)
    {
        var options = new RulesGetOptions();
        options.DeploymentTool = parseResult.GetValueOrDefault<string>(DeployOptionDefinitions.IaCRules.DeploymentTool.Name) ?? string.Empty;
        options.IacType = parseResult.GetValueOrDefault<string>(DeployOptionDefinitions.IaCRules.IacType.Name) ?? string.Empty;
        options.ResourceTypes = parseResult.GetValueOrDefault<string>(DeployOptionDefinitions.IaCRules.ResourceTypes.Name) ?? string.Empty;
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
            context.Activity?
                .AddTag(DeployTelemetryTags.DeploymentTool, options.DeploymentTool)
                .AddTag(DeployTelemetryTags.IacType, options.IacType)
                .AddTag(DeployTelemetryTags.ComputeHostResources, options.ResourceTypes);

            var resourceTypes = options.ResourceTypes.Split(',')
                .Select(rt => rt.Trim().ToLowerInvariant())
                .Where(rt => !string.IsNullOrWhiteSpace(rt))
                .ToArray();

            string iacRules = IaCRulesTemplateUtil.GetIaCRules(
                options.DeploymentTool,
                options.IacType,
                resourceTypes);

            context.Response.Message = iacRules;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred while retrieving IaC rules.");
            HandleException(context, ex);
        }
        return Task.FromResult(context.Response);
    }
}
