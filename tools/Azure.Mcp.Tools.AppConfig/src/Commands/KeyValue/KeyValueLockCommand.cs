// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.AppConfig.Options.KeyValue;
using Azure.Mcp.Tools.AppConfig.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.AppConfig.Commands.KeyValue;

public sealed class KeyValueLockCommand(ILogger<KeyValueLockCommand> logger) : BaseKeyValueCommand<KeyValueLockOptions>()
{
    private const string CommandTitle = "Lock App Configuration Key-Value Setting";
    private readonly ILogger<KeyValueLockCommand> _logger = logger;

    public override string Name => "lock";

    public override string Description =>
        """
        Lock a key-value in an App Configuration store. This command sets a key-value to read-only mode,
        preventing any modifications to its value. You must specify an account name and key. Optionally,
        you can specify a label to lock a specific labeled version of the key-value.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = true,
        ReadOnly = false,
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
            await appConfigService.LockKeyValue(
                options.Account!,
                options.Key!,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy,
                options.Label);

            context.Response.Results =
                ResponseResult.Create(
                    new KeyValueLockCommandResult(options.Key, options.Label),
                    AppConfigJsonContext.Default.KeyValueLockCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred locking value. Key: {Key}, Label: {Label}", options.Key, options.Label);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record KeyValueLockCommandResult(string? Key, string? Label);
}
