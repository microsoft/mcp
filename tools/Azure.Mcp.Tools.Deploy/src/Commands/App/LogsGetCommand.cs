// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Tools.Deploy.Options;
using Azure.Mcp.Tools.Deploy.Options.App;
using Azure.Mcp.Tools.Deploy.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Deploy.Commands.App;

public sealed class LogsGetCommand(ILogger<LogsGetCommand> logger) : SubscriptionCommand<LogsGetOptions>()
{
    private const string CommandTitle = "Get AZD deployed App Logs";
    private readonly ILogger<LogsGetCommand> _logger = logger;

    private readonly Option<string> _workspaceFolderOption = DeployOptionDefinitions.AzdAppLogOptions.WorkspaceFolder;
    private readonly Option<string> _azdEnvNameOption = DeployOptionDefinitions.AzdAppLogOptions.AzdEnvName;
    private readonly Option<int> _limitOption = DeployOptionDefinitions.AzdAppLogOptions.Limit;

    public override string Name => "get";
    public override string Title => CommandTitle;
    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = true,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    public override string Description =>
        """
        This tool fetches logs from the Log Analytics workspace for Container Apps, App Services, and Function Apps deployed using azd. Use it after a successful azd up to check app status or troubleshoot errors in deployed applications.
        """;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(_workspaceFolderOption);
        command.Options.Add(_azdEnvNameOption);
        command.Options.Add(_limitOption);
    }

    protected override LogsGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.WorkspaceFolder = parseResult.GetValue(_workspaceFolderOption)!;
        options.AzdEnvName = parseResult.GetValue(_azdEnvNameOption)!;
        options.Limit = parseResult.GetValue(_limitOption);
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
