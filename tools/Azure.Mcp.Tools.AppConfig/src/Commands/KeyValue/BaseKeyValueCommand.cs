// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.AppConfig.Options;
using Azure.Mcp.Tools.AppConfig.Options.KeyValue;

namespace Azure.Mcp.Tools.AppConfig.Commands.KeyValue;

public abstract class BaseKeyValueCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] T>
    : BaseAppConfigCommand<T> where T : BaseKeyValueOptions, new()
{
    protected readonly Option<string> _keyOption = AppConfigOptionDefinitions.Key;
    protected readonly Option<string> _labelOption = AppConfigOptionDefinitions.Label;
    protected readonly Option<string> _contentTypeOption = AppConfigOptionDefinitions.ContentType;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_keyOption);
        command.AddOption(_labelOption);
        command.AddOption(_contentTypeOption);
    }

    protected override T BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Key = parseResult.GetValueForOption(_keyOption);
        options.Label = parseResult.GetValueForOption(_labelOption);
        options.ContentType = parseResult.GetValueForOption(_contentTypeOption);
        return options;
    }
}
