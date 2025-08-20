// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Services.Telemetry;
using Azure.Mcp.Tools.AppConfig.Models;
using Azure.Mcp.Tools.AppConfig.Options;
using Azure.Mcp.Tools.AppConfig.Options.KeyValue;
using Azure.Mcp.Tools.AppConfig.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.AppConfig.Commands.KeyValue;

public sealed class KeyValueListCommand(ILogger<KeyValueListCommand> logger) : BaseAppConfigCommand<KeyValueListOptions>()
{
    private const string CommandTitle = "List App Configuration Key-Value Settings";
    private readonly ILogger<KeyValueListCommand> _logger = logger;

    // KeyValueList has different key and label descriptions, which is why we are defining here instead of using BaseKeyValueCommand
    private readonly Option<string> _keyOption = AppConfigOptionDefinitions.KeyValueList.Key;
    private readonly Option<string> _labelOption = AppConfigOptionDefinitions.KeyValueList.Label;

    public override string Name => "list";

    public override string Description =>
        """
        List all key-values in an App Configuration store. This command retrieves and displays all key-value pairs
        from the specified store. Each key-value includes its key, value, label, content type, ETag, last modified
        time, and lock status.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_keyOption);
        command.AddOption(_labelOption);
    }

    protected override KeyValueListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Key = parseResult.GetValueForOption(_keyOption);
        options.Label = parseResult.GetValueForOption(_labelOption);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var options = BindOptions(parseResult);

        try
        {
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            {
                return context.Response;
            }

            var appConfigService = context.GetService<IAppConfigService>();
            var settings = await appConfigService.ListKeyValues(
                options.Account!,
                options.Subscription!,
                options.Key,
                options.Label,
                options.Tenant,
                options.RetryPolicy);

            context.Response.Results = settings?.Count > 0 ?
                ResponseResult.Create(
                    new KeyValueListCommandResult(settings),
                    AppConfigJsonContext.Default.KeyValueListCommandResult) :
                null;
        }
        catch (Exception ex)
        {
            _logger.LogError("An exception occurred processing command. Exception: {Exception}", ex);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record KeyValueListCommandResult(List<KeyValueSetting> Settings);
}
