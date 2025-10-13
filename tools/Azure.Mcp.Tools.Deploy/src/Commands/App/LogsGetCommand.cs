// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Deploy.Options;
using Azure.Mcp.Tools.Deploy.Options.App;
using Azure.Mcp.Tools.Deploy.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Deploy.Commands.App;

public sealed class LogsGetCommand(ILogger<LogsGetCommand> logger) : SubscriptionCommand<LogsGetOptions>()
{
    private const string CommandTitle = "Get AZD deployed App Logs";
    private readonly ILogger<LogsGetCommand> _logger = logger;

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
        Shows application logs specifically for Azure Developer CLI (azd) deployed applications from their associated Log Analytics workspace for Container Apps, App Services, and Function Apps. Designed exclusively for applications deployed via 'azd up' command and automatically discovers the correct workspace and resources based on the azd environment configuration. Use this tool to check deployment status or troubleshoot post-deployment issues.
        """;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(DeployOptionDefinitions.AzdAppLogOptions.WorkspaceFolder);
        command.Options.Add(DeployOptionDefinitions.AzdAppLogOptions.AzdEnvName);
        command.Options.Add(DeployOptionDefinitions.AzdAppLogOptions.Limit);
    }

    protected override LogsGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.WorkspaceFolder = parseResult.GetValueOrDefault<string>(DeployOptionDefinitions.AzdAppLogOptions.WorkspaceFolder.Name)!;
        options.AzdEnvName = parseResult.GetValueOrDefault<string>(DeployOptionDefinitions.AzdAppLogOptions.AzdEnvName.Name)!;
        options.Limit = parseResult.GetValueOrDefault<int>(DeployOptionDefinitions.AzdAppLogOptions.Limit.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {


            var deployService = context.GetService<IDeployService>();
            string result = await deployService.GetAzdResourceLogsAsync(
                options.WorkspaceFolder!,
                options.AzdEnvName!,
                options.Subscription!,
                options.Limit);

            context.Response.Message = result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred getting azd app logs.");
            HandleException(context, ex);
        }

        return context.Response;
    }

}
