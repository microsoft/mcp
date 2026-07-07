// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.AppConfig.Models;
using Azure.Mcp.Tools.AppConfig.Options.KeyValue;
using Azure.Mcp.Tools.AppConfig.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AppConfig.Commands.KeyValue;

[CommandMetadata(
    Id = "abc28800-ae4a-4369-9ec0-2653a578e82a",
    Name = "get",
    Title = "Gets App Configuration Key-Value Settings",
    Description = """
        Gets key-values in an App Configuration store. This command can either retrieve a specific key-value by its key
        and optional label, or list key-values if no key is provided. Listing key-values can optionally be filtered by a
        key filter and label filter. Each key-value includes its key, value, label, content type, ETag, last modified time,
        and lock status.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class KeyValueGetCommand(ILogger<KeyValueGetCommand> logger, IAppConfigService appConfigService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<KeyValueGetOptions, KeyValueGetCommand.KeyValueGetCommandResult>(subscriptionResolver)
{
    private readonly ILogger<KeyValueGetCommand> _logger = logger;
    private readonly IAppConfigService _appConfigService = appConfigService;

    public override void ValidateOptions(KeyValueGetOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        if (!string.IsNullOrEmpty(options.Key) && !string.IsNullOrEmpty(options.KeyFilter))
        {
            validationResult.Errors.Add("Cannot specify both --key and --key-filter options together. Use only one to get a specific key-value or to filter the list of key-values.");
        }
        if (!string.IsNullOrEmpty(options.Label) && !string.IsNullOrEmpty(options.LabelFilter))
        {
            validationResult.Errors.Add("Cannot specify both --label and --label-filter options together. Use only one to get a specific key-value or to filter the list of key-values.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, KeyValueGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var settings = await _appConfigService.GetKeyValues(
                options.Account,
                options.Subscription!,
                options.Key,
                options.Label,
                options.KeyFilter,
                options.LabelFilter,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(settings ?? []), AppConfigJsonContext.Default.KeyValueGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError("An exception occurred processing command. Exception: {Exception}", ex);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record KeyValueGetCommandResult(List<KeyValueSetting> Settings);
}
