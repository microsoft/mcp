// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.AppConfig.Models;
using Azure.Mcp.Tools.AppConfig.Options;
using Azure.Mcp.Tools.AppConfig.Options.KeyValue;
using Azure.Mcp.Tools.AppConfig.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.AppConfig.Commands.KeyValue;

public sealed class KeyValueGetCommand(ILogger<KeyValueGetCommand> logger) : BaseAppConfigCommand<KeyValueGetOptions>()
{
    private const string CommandTitle = "Gets App Configuration Key-Value Settings";
    private readonly ILogger<KeyValueGetCommand> _logger = logger;

    public override string Name => "get";

    public override string Description =>
        """
        Gets key-values in an App Configuration store. This command can either retrieve a specific key-value by its key
        and optional label, or list key-values if no key is provided. Listing key-values can optionally be filtered by a
        key filter and label filter. Each key-value includes its key, value, label, content type, ETag, last modified time,
        and lock status.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(AppConfigOptionDefinitions.Key.AsOptional());
        command.Options.Add(AppConfigOptionDefinitions.Label);
        command.Options.Add(AppConfigOptionDefinitions.KeyFilter);
        command.Options.Add(AppConfigOptionDefinitions.LabelFilter);
        command.Validators.Add(result =>
        {
            var key = result.GetValueOrDefault<string>(AppConfigOptionDefinitions.Key.Name);
            var keyFilter = result.GetValueOrDefault(AppConfigOptionDefinitions.KeyFilter);
            if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(keyFilter))
            {
                result.AddError("Cannot specify both --key and --key-filter options together. Use only one to get a specific key-value or to filter the list of key-values.");
            }
        });
    }

    protected override KeyValueGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Key = parseResult.GetValueOrDefault<string>(AppConfigOptionDefinitions.Key.Name);
        options.Label = parseResult.GetValueOrDefault<string>(AppConfigOptionDefinitions.Label.Name);
        options.KeyFilter = parseResult.GetValueOrDefault<string>(AppConfigOptionDefinitions.KeyFilter.Name);
        options.LabelFilter = parseResult.GetValueOrDefault<string>(AppConfigOptionDefinitions.LabelFilter.Name);
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
            var settings = await appConfigService.GetKeyValues(
                options.Account!,
                options.Subscription!,
                options.Key,
                options.Label,
                options.KeyFilter,
                options.LabelFilter,
                options.Tenant,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(new(settings ?? []), AppConfigJsonContext.Default.KeyValueGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError("An exception occurred processing command. Exception: {Exception}", ex);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record KeyValueGetCommandResult(List<KeyValueSetting> Settings);
}
