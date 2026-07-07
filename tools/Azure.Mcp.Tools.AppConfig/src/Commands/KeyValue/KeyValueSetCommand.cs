// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.AppConfig.Options.KeyValue;
using Azure.Mcp.Tools.AppConfig.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AppConfig.Commands.KeyValue;

[CommandMetadata(
    Id = "a89086eb-acf4-4168-9d32-de5cd7384030",
    Name = "set",
    Title = "Set App Configuration Key-Value Setting",
    Description = """
        Set a key-value setting in an App Configuration store. This command creates or updates a key-value setting
        with the specified value. You must specify an account name, key, and value. Optionally, you can specify a
        label otherwise the default label will be used. You can also specify a content type to indicate how the value
        should be interpreted. You can add tags in the format 'key=value' to associate metadata with the setting.
        """,
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class KeyValueSetCommand(ILogger<KeyValueSetCommand> logger, IAppConfigService appConfigService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<KeyValueSetOptions, KeyValueSetCommand.KeyValueSetCommandResult>(subscriptionResolver)
{
    private readonly ILogger<KeyValueSetCommand> _logger = logger;
    private readonly IAppConfigService _appConfigService = appConfigService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, KeyValueSetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            await _appConfigService.SetKeyValue(
                options.Account,
                options.Key,
                options.Value,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy,
                options.Label,
                options.ContentType,
                options.Tags,
                cancellationToken);
            context.Response.Results = ResponseResult.Create(
                new(options.Key, options.Value, options.Label, options.ContentType, options.Tags),
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

    public sealed record KeyValueSetCommandResult(string? Key, string? Value, string? Label, string? ContentType = null, string[]? Tags = null);
}
