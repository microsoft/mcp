// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Communication.Options;

namespace Azure.Mcp.Tools.Communication.Commands;

public abstract class BaseCommunicationCommand<
    [System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicProperties | System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.NonPublicProperties)]
TOptions> : GlobalCommand<TOptions>
    where TOptions : BaseCommunicationOptions, new()
{
    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(CommunicationOptionDefinitions.ConnectionString);
    }

    protected override TOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ConnectionString = parseResult.GetValueOrDefault<string>(CommunicationOptionDefinitions.ConnectionString.Name);
        return options;
    }
}
