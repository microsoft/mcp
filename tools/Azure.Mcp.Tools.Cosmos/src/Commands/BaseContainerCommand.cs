// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Tools.Cosmos.Options;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;

namespace Azure.Mcp.Tools.Cosmos.Commands;

public abstract class BaseContainerCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions>
    : BaseDatabaseCommand<TOptions> where TOptions : BaseContainerOptions, new()
{

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(CosmosOptionDefinitions.Container);
    }

    protected override TOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Container = parseResult.GetValueOrDefault<string>(CosmosOptionDefinitions.Container.Name);
        return options;
    }
}
