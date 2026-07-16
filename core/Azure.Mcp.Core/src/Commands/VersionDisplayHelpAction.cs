// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Help;
using System.CommandLine.Invocation;
using Azure.Mcp.Core.Configuration;
using Microsoft.Extensions.Options;

namespace Azure.Mcp.Core.Commands;

/// <summary>
/// Custom help action that displays version information before the standard help output.
/// </summary>
internal class VersionDisplayHelpAction : SynchronousCommandLineAction
{
    private readonly IOptions<AzureMcpServerConfiguration> _options;
    private readonly HelpAction _defaultHelp;

    public VersionDisplayHelpAction(IOptions<AzureMcpServerConfiguration> options, HelpAction action)
    {
        _options = options;
        _defaultHelp = action;
    }

    public override int Invoke(ParseResult parseResult)
    {
        Console.WriteLine($"{_options.Value.Name} {_options.Value.Version}{Environment.NewLine}");

        return _defaultHelp.Invoke(parseResult);
    }
}
