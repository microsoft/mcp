// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.ConfidentialLedger.Options;

namespace Azure.Mcp.Tools.ConfidentialLedger.Commands;

public abstract class BaseConfidentialLedgerCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] T>
    : GlobalCommand<T>
    where T : BaseConfidentialLedgerOptions, new()
{
    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(ConfidentialLedgerOptionDefinitions.LedgerName.AsRequired());
    }

    protected override T BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.LedgerName = parseResult.GetValueOrDefault<string>(ConfidentialLedgerOptionDefinitions.LedgerName.Name);
        return options;
    }
}
