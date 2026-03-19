// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.DocumentDb.Options;

namespace Azure.Mcp.Tools.DocumentDb.Commands;

public abstract class BaseDocumentDbCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions>
    : GlobalCommand<TOptions> where TOptions : BaseDocumentDbOptions, new()
{
    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(DocumentDbOptionDefinitions.ConnectionString);
    }

    protected override TOptions BindOptions(ParseResult parseResult)
    {
        return new TOptions
        {
            ConnectionString = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.ConnectionString.Name)
        };
    }
}
