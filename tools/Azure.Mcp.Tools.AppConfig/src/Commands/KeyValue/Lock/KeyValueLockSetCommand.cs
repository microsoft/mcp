// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.AppConfig.Options.KeyValue.Lock;
using Azure.Mcp.Tools.AppConfig.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AppConfig.Commands.KeyValue.Lock;

[CommandMetadata(
    Id = "b48fd781-d74a-4dfd-a29c-421ded9a6ce9",
    Name = "set",
    Title = "Sets the lock state of an App Configuration Key-Value Setting",
    Description = """
        Sets the lock state of a key-value in an App Configuration store. This command can lock and unlock key-values.
        Locking sets a key-value to read-only mode, preventing any modifications to its value. Unlocking removes the
        read-only mode from a key-value setting, allowing modifications to its value. You must specify an account name
        and key. Optionally, you can specify a label to lock or unlock a specific labeled version of the key-value.
        Default is unlocking the key-value.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class KeyValueLockSetCommand(ILogger<KeyValueLockSetCommand> logger, IAppConfigService appConfigService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<KeyValueLockSetOptions, KeyValueLockSetCommand.KeyValueLockSetCommandResult>(subscriptionResolver)
{
    private readonly ILogger<KeyValueLockSetCommand> _logger = logger;
    private readonly IAppConfigService _appConfigService = appConfigService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, KeyValueLockSetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            await _appConfigService.SetKeyValueLockState(
                options.Account,
                options.Key,
                options.Lock ?? false,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy,
                options.Label,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(options.Key, options.Label, options.Lock ?? false), AppConfigJsonContext.Default.KeyValueLockSetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred setting lock value. Key: {Key}, Label: {Label}, Lock: {Lock}",
                options.Key, options.Label, options.Lock);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record KeyValueLockSetCommandResult(string Key, string? Label, bool Locked);
}
