// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.AppConfig.Options;
using Azure.Mcp.Tools.AppConfig.Options.KeyValue;

namespace Azure.Mcp.Tools.AppConfig.Commands.KeyValue;

public abstract class BaseKeyValueCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] T>
    : BaseAppConfigCommand<T> where T : BaseKeyValueOptions, new()
{
    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(AppConfigOptionDefinitions.Key);
        command.Options.Add(AppConfigOptionDefinitions.Label);
        command.Options.Add(AppConfigOptionDefinitions.ContentType);
    }

    protected override T BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Key = parseResult.GetValueOrDefault<string>(AppConfigOptionDefinitions.Key.Name);
        options.Label = parseResult.GetValueOrDefault<string>(AppConfigOptionDefinitions.Label.Name);
        options.ContentType = parseResult.GetValueOrDefault<string>(AppConfigOptionDefinitions.ContentType.Name);
        return options;
    }
}
