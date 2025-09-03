// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Storage.Options;
using Azure.Mcp.Tools.Storage.Options.Share.File;

namespace Azure.Mcp.Tools.Storage.Commands.Share.File;

public abstract class BaseFileCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions>
    : BaseShareCommand<TOptions> where TOptions : BaseFileOptions, new()
{
    protected readonly Option<string> _directoryPathOption = StorageOptionDefinitions.DirectoryPath;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(_directoryPathOption);
    }

    protected override TOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.DirectoryPath = parseResult.GetValueOrDefault(_directoryPathOption);
        return options;
    }
}
