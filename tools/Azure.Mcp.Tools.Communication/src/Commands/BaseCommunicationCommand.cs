// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
using System.Diagnostics.CodeAnalysis;
using AzureMcp.Communication.Options;
using AzureMcp.Core.Commands;

namespace Azure.Mcp.Tools.Communication.Commands;

public abstract class BaseCommunicationCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions>
    : GlobalCommand<TOptions>
    where TOptions : BaseCommunicationOptions, new()
{
    protected readonly Option<string> _connectionStringOption = CommunicationOptionDefinitions.ConnectionString;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_connectionStringOption);
    }

    protected override TOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ConnectionString = parseResult.GetValueForOption(_connectionStringOption);
        return options;
    }
}