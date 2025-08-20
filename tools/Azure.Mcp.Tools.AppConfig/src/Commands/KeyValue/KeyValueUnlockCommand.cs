// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Services.Telemetry;
using Azure.Mcp.Tools.AppConfig.Options.KeyValue;
using Azure.Mcp.Tools.AppConfig.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.AppConfig.Commands.KeyValue;

public sealed class KeyValueUnlockCommand(ILogger<KeyValueUnlockCommand> logger) : BaseKeyValueCommand<KeyValueUnlockOptions>()
{
    private const string CommandTitle = "Unlock App Configuration Key-Value Setting";
    private readonly ILogger<KeyValueUnlockCommand> _logger = logger;

    public override string Name => "unlock";

    public override string Description =>
        """
        Unlock a key-value setting in an App Configuration store. This command removes the read-only mode from a
        key-value setting, allowing modifications to its value. You must specify an account name and key. Optionally,
        you can specify a label to unlock a specific labeled version of the setting, otherwise the setting with the
        default label will be unlocked.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = false };

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
            await appConfigService.UnlockKeyValue(
                options.Account!,
                options.Key!,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy,
                options.Label);

            context.Response.Results =
                ResponseResult.Create(
                    new KeyValueUnlockResult(options.Key, options.Label),
                    AppConfigJsonContext.Default.KeyValueUnlockResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred unlocking key. Key: {Key}.", options.Key);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record KeyValueUnlockResult(string? Key, string? Label);
}
