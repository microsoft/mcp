// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Deploy.Options;
using Azure.Mcp.Tools.Deploy.Options.Infrastructure;
using Azure.Mcp.Tools.Deploy.Services.Util;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Deploy.Commands.Infrastructure;

public sealed class RulesGetCommand(ILogger<RulesGetCommand> logger)
    : BaseCommand()
{
    private const string CommandTitle = "Get Iac(Infrastructure as Code) Rules";
    private readonly ILogger<RulesGetCommand> _logger = logger;

    private readonly Option<string> _deploymentToolOption = DeployOptionDefinitions.IaCRules.DeploymentTool;
    private readonly Option<string> _iacTypeOption = DeployOptionDefinitions.IaCRules.IacType;
    private readonly Option<string> _resourceTypesOption = DeployOptionDefinitions.IaCRules.ResourceTypes;

    public override string Name => "get";
    public override string Title => CommandTitle;
    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    public override string Description =>
        """
        This tool offers guidelines for creating Bicep/Terraform files to deploy applications on Azure. The guidelines outline rules to improve the quality of Infrastructure as Code files, ensuring they are compatible with the azd tool and adhere to best practices.
        """;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(_deploymentToolOption);
        command.Options.Add(_iacTypeOption);
        command.Options.Add(_resourceTypesOption);
    }

    private RulesGetOptions BindOptions(ParseResult parseResult)
    {
        var options = new RulesGetOptions();
        options.DeploymentTool = parseResult.GetValue(_deploymentToolOption) ?? string.Empty;
        options.IacType = parseResult.GetValue(_iacTypeOption) ?? string.Empty;
        options.ResourceTypes = parseResult.GetValue(_resourceTypesOption) ?? string.Empty;

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
            var resourceTypes = options.ResourceTypes.Split(',')
                .Select(rt => rt.Trim())
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
