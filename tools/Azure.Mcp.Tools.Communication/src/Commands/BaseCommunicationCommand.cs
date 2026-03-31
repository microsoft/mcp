// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Communication.Options;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;

namespace Azure.Mcp.Tools.Communication.Commands;

public abstract class BaseCommunicationCommand<
    [System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicProperties | System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.NonPublicProperties)]
TOptions> : GlobalCommand<TOptions>
    where TOptions : BaseCommunicationOptions, new()
{
    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(CommunicationOptionDefinitions.Endpoint);
    }

    protected override TOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Endpoint = parseResult.GetValueOrDefault<string>(CommunicationOptionDefinitions.Endpoint.Name);
        return options;
    }
}
