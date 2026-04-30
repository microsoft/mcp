// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AppConfig.Options.KeyValue;
using Azure.Mcp.Tools.AppConfig.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AppConfig.Commands.KeyValue;

[CommandMetadata(
    Id = "f885a499-82ec-4897-a788-fb6b4615ab06",
    Name = "delete",
    Title = "Delete App Configuration Key-Value Setting",
    Description = """
        Delete a key-value pair from an App Configuration store. This command removes the specified key-value pair from the store.
        If a label is specified, only the labeled version is deleted. If no label is specified, the key-value with the matching
        key and the default label will be deleted.
        """,
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class KeyValueDeleteCommand(ILogger<KeyValueDeleteCommand> logger, IAppConfigService appConfigService)
    : BaseKeyValueCommand<KeyValueDeleteOptions>()
{
    private readonly ILogger<KeyValueDeleteCommand> _logger = logger;
    private readonly IAppConfigService _appConfigService = appConfigService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            await _appConfigService.DeleteKeyValue(
                options.Account!,
                options.Key!,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy,
                options.Label,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(options.Key, options.Label), AppConfigJsonContext.Default.KeyValueDeleteCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred deleting value. Key: {Key}.", options.Key);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record KeyValueDeleteCommandResult(string? Key, string? Label);
}
