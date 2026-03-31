// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Tools.Kusto.Options;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;

namespace Azure.Mcp.Tools.Kusto.Commands;

public abstract class BaseDatabaseCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions>
    : BaseClusterCommand<TOptions> where TOptions : BaseDatabaseOptions, new()
{
    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(KustoOptionDefinitions.Database);
    }

    protected override TOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Database = parseResult.GetValueOrDefault<string>(KustoOptionDefinitions.Database.Name);
        return options;
    }
}
