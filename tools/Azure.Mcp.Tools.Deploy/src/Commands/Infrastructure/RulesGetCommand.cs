// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Deploy.Models;
using Azure.Mcp.Tools.Deploy.Options;
using Azure.Mcp.Tools.Deploy.Options.Infrastructure;
using Azure.Mcp.Tools.Deploy.Services.Templates;
using Azure.Mcp.Tools.Deploy.Services.Util;
using Microsoft.Extensions.Logging;

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
        This tool offers guidelines for creating Infrastructure as Code (IaC) files to deploy applications on Azure using Azure CLI. The guidelines outline rules to improve the quality of IaC files, ensuring they are compatible with Azure CLI deployment workflows.
        """;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(DeployOptionDefinitions.IaCRules.ResourceTypes);
    }

    protected override RulesGetOptions BindOptions(ParseResult parseResult)
    {
        var options = new RulesGetOptions();
        options.ResourceTypes = parseResult.GetValueOrDefault<string>(DeployOptionDefinitions.IaCRules.ResourceTypes.Name) ?? string.Empty;
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
            context.Activity?
                .AddTag(DeployTelemetryTags.ComputeHostResources, options.ResourceTypes);

            var resourceTypes = options.ResourceTypes.Split(',')
                .Select(rt => rt.Trim())
                .Where(rt => !string.IsNullOrWhiteSpace(rt))
                .ToArray();

            string iacRules = TemplateService.LoadTemplate("IaCRules/azcli-rules");

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
