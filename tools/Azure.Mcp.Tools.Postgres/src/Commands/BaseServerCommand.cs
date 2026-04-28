// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Tools.Postgres.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.Postgres.Commands;

public abstract class BaseServerCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions>(ILogger<BasePostgresCommand<TOptions>> logger)
    : BasePostgresCommand<TOptions>(logger) where TOptions : BasePostgresOptions, new()

{
    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
        command.Options.Add(PostgresOptionDefinitions.User);
        command.Options.Add(PostgresOptionDefinitions.Server);
    }

    protected override TOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Server = parseResult.GetValueOrDefault<string>(PostgresOptionDefinitions.Server.Name);
        return options;
    }
}
