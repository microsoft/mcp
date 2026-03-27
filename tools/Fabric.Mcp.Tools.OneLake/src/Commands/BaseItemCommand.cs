// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Fabric.Mcp.Tools.OneLake.Options;
using Microsoft.Mcp.Core.Extensions;

namespace Fabric.Mcp.Tools.OneLake.Commands;

public abstract class BaseItemCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions>
    : BaseWorkspaceCommand<TOptions> where TOptions : BaseItemOptions, new()
{
    protected BaseItemCommand()
    {
    }

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(FabricOptionDefinitions.ItemId);
    }

    protected override TOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ItemId = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.ItemId.Name);
        return options;
    }
}
