// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.AppConfig.Options;
using Azure.Mcp.Tools.AppConfig.Options.KeyValue;
using Azure.Mcp.Tools.AppConfig.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.AppConfig.Commands.KeyValue;

public sealed class KeyValueSetCommand(ILogger<KeyValueSetCommand> logger) : BaseKeyValueCommand<KeyValueSetOptions>()
{
    private const string CommandTitle = "Set App Configuration Key-Value Setting";
    private readonly Option<string> _valueOption = AppConfigOptionDefinitions.Value;
    private readonly Option<string[]> _tagsOption = AppConfigOptionDefinitions.Tags;
    private readonly ILogger<KeyValueSetCommand> _logger = logger;

    public override string Name => "set";

    public override string Description =>
        """
        Set a key-value setting in an App Configuration store. This command creates or updates a key-value setting
        with the specified value. You must specify an account name, key, and value. Optionally, you can specify a
        label otherwise the default label will be used. You can also specify a content type to indicate how the value
        should be interpreted. You can add tags in the format 'key=value' to associate metadata with the setting.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = true,
        Idempotent = true,
        OpenWorld = true,
        ReadOnly = false,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(_valueOption);
        command.Options.Add(_tagsOption);
    }

    protected override KeyValueSetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Value = parseResult.GetValue(_valueOption);
        options.Tags = parseResult.GetValue(_tagsOption);
        return options;
    }

    [McpServerTool(Destructive = true, ReadOnly = false, Title = CommandTitle)]
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var appConfigService = context.GetService<IAppConfigService>();
            await appConfigService.SetKeyValue(
                options.Account!,
                options.Key!,
                options.Value!,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy,
                options.Label,
                options.ContentType,
                options.Tags);
            context.Response.Results = ResponseResult.Create(
                new KeyValueSetCommandResult(
                    options.Key,
                    options.Value,
                    options.Label,
                    options.ContentType,
                    options.Tags),
                AppConfigJsonContext.Default.KeyValueSetCommandResult
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred setting value. Key: {Key}.", options.Key);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record KeyValueSetCommandResult(string? Key, string? Value, string? Label, string? ContentType = null, string[]? Tags = null);
}
