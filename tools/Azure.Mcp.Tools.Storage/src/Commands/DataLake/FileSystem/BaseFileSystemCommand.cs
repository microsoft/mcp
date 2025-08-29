// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Tools.Storage.Options;
using Azure.Mcp.Tools.Storage.Options.DataLake;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.Storage.Commands.DataLake.FileSystem;

public abstract class BaseFileSystemCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions>
    : BaseStorageCommand<TOptions> where TOptions : BaseFileSystemOptions, new()
{
    protected readonly Option<string> _fileSystemOption = StorageOptionDefinitions.FileSystem;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_fileSystemOption);
    }

    protected override TOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.FileSystem = parseResult.GetValueForOption(_fileSystemOption);
        return options;
    }
}
