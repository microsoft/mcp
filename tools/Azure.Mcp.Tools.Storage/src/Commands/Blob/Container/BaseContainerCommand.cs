// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Storage.Options;
using Azure.Mcp.Tools.Storage.Options.Blob;

namespace Azure.Mcp.Tools.Storage.Commands.Blob.Container;

public abstract class BaseContainerCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions>
    : BaseStorageCommand<TOptions> where TOptions : BaseContainerOptions, new()
{
    protected BaseContainerCommand()
    {
    }

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(StorageOptionDefinitions.Container);
    }

    protected override TOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Container = parseResult.GetValueOrDefault<string>(StorageOptionDefinitions.Container.Name);
        return options;
    }
}
