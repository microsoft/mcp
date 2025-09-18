// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.AppConfig.Models;
using Azure.Mcp.Tools.AppConfig.Options.KeyValue;
using Azure.Mcp.Tools.AppConfig.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.AppConfig.Commands.KeyValue;

public sealed class KeyValueShowCommand(ILogger<KeyValueShowCommand> logger) : BaseKeyValueCommand<KeyValueShowOptions>()
{
    private const string CommandTitle = "Show App Configuration Key-Value Setting";
    private readonly ILogger<KeyValueShowCommand> _logger = logger;

    public override string Name => "show";

    public override string Description =>
        """
        Show a specific key-value setting in an App Configuration store. This command retrieves and displays the value,
        label, content type, ETag, last modified time, and lock status for a specific setting. You must specify an
        account name and key. Optionally, you can specify a label otherwise the setting with default label will be retrieved.
        You can also specify a content type to indicate how the value should be interpreted.
        """;

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
            var setting = await appConfigService.GetKeyValue(
                options.Account!,
                options.Key!,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy,
                options.Label,
                options.ContentType);

            context.Response.Results = ResponseResult.Create(new(setting), AppConfigJsonContext.Default.KeyValueShowResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred getting value. Key: {Key}.", options.Key);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record KeyValueShowResult(KeyValueSetting Setting);
}
