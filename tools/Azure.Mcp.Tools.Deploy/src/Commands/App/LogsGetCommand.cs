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
    private const string CommandTitle = "Get Application Logs";
    private readonly ILogger<LogsGetCommand> _logger = logger;
    public override string Id => "ce9d648d-7c76-48a0-8cba-b9b57c6fd00b";

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
        Shows application logs from Log Analytics workspace for Container Apps, App Services, and Function Apps. Designed for deployed applications workspace and discovers the correct workspace and resources based on the provided resource group name. Use this tool to check deployment status or troubleshoot post-deployment issues.
        """;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(DeployOptionDefinitions.AppLogOptions.WorkspaceFolder);
        command.Options.Add(DeployOptionDefinitions.AppLogOptions.ResourceGroupName);
        command.Options.Add(DeployOptionDefinitions.AppLogOptions.Limit);
    }

    protected override LogsGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.WorkspaceFolder = parseResult.GetValueOrDefault<string>(DeployOptionDefinitions.AppLogOptions.WorkspaceFolder.Name)!;
        options.ResourceGroup = parseResult.GetValueOrDefault<string>(DeployOptionDefinitions.AppLogOptions.ResourceGroupName.Name);
        options.Limit = parseResult.GetValueOrDefault<int>(DeployOptionDefinitions.AppLogOptions.Limit.Name);
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
            string result = await deployService.GetResourceLogsAsync(
                options.WorkspaceFolder!,
                options.Subscription!,
                options.ResourceGroup!,
                options.Limit);

            context.Response.Message = result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred getting app logs.");
            HandleException(context, ex);
        }

        return context.Response;
    }

}
