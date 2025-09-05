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
        command.Options.Add(_keyOption);
        command.Options.Add(_labelOption);
        command.Options.Add(_contentTypeOption);
    }

    protected override T BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Key = parseResult.GetValue(_keyOption);
        options.Label = parseResult.GetValue(_labelOption);
        options.ContentType = parseResult.GetValue(_contentTypeOption);
        return options;
    }
}
