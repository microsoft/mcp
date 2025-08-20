// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Storage.Commands.Blob.Container;
using Azure.Mcp.Tools.Storage.Options;
using Azure.Mcp.Tools.Storage.Options.Blob;

namespace Azure.Mcp.Tools.Storage.Commands.Blob;

public abstract class BaseBlobCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions>
    : BaseContainerCommand<TOptions> where TOptions : BaseBlobOptions, new()
{
    protected readonly Option<string> _blobOption = StorageOptionDefinitions.Blob;

    protected BaseBlobCommand()
    {
    }

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_blobOption);
    }

    protected override TOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Blob = parseResult.GetValueForOption(_blobOption);
        return options;
    }
}
