// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.AppConfig.Options;
using Azure.Mcp.Tools.AppConfig.Options.KeyValue.Lock;
using Azure.Mcp.Tools.AppConfig.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.AppConfig.Commands.KeyValue.Lock;

public sealed class KeyValueLockSetCommand(ILogger<KeyValueLockSetCommand> logger) : BaseKeyValueCommand<KeyValueLockSetOptions>()
{
    private const string CommandTitle = "Sets the lock state of an App Configuration Key-Value Setting";
    private readonly ILogger<KeyValueLockSetCommand> _logger = logger;

    public override string Name => "set";

    public override string Description =>
        """
        Sets the lock state of a key-value in an App Configuration store. This command can lock and unlock key-values.
        Locking sets a key-value to read-only mode, preventing any modifications to its value. Unlocking removes the
        read-only mode from a key-value setting, allowing modifications to its value. You must specify an account name
        and key. Optionally, you can specify a label to lock or unlock a specific labeled version of the key-value.
        Default is unlocking the key-value.
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

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(AppConfigOptionDefinitions.Lock);
    }

    protected override KeyValueLockSetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Lock = parseResult.GetValueOrDefault(AppConfigOptionDefinitions.Lock);
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
            var appConfigService = context.GetService<IAppConfigService>();
            await appConfigService.SetKeyValueLockState(
                options.Account!,
                options.Key!,
                options.Lock,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy,
                options.Label);

            context.Response.Results = ResponseResult.Create(new(options.Key!, options.Label, options.Lock), AppConfigJsonContext.Default.KeyValueLockSetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred setting lock value. Key: {Key}, Label: {Label}, Lock: {Lock}",
                options.Key, options.Label, options.Lock);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record KeyValueLockSetCommandResult(string Key, string? Label, bool Locked);
}
