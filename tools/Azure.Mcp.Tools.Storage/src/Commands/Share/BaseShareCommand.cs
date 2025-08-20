// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Storage.Options;
using Azure.Mcp.Tools.Storage.Options.Share;

namespace Azure.Mcp.Tools.Storage.Commands.Share;

public abstract class BaseShareCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions>
    : BaseStorageCommand<TOptions> where TOptions : BaseShareOptions, new()
{
    protected readonly Option<string> _shareOption = StorageOptionDefinitions.Share;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_shareOption);
    }

    protected override TOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Share = parseResult.GetValueForOption(_shareOption);
        return options;
    }
}
