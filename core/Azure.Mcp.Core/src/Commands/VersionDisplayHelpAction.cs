// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.Reflection;
using Azure.Mcp.Core.Helpers;

namespace Azure.Mcp.Core.Commands;

/// <summary>
/// Custom help action that displays version information before the standard help output.
/// </summary>
internal class VersionDisplayHelpAction : SynchronousCommandLineAction
{
    private readonly HelpAction _defaultHelp;

    public VersionDisplayHelpAction(HelpAction action) => _defaultHelp = action;

    public override int Invoke(ParseResult parseResult)
    {
        DisplayVersion();
        return _defaultHelp.Invoke(parseResult);
    }

    private static void DisplayVersion()
    {
        var assembly = Assembly.GetEntryAssembly();
        string version = assembly != null ? AssemblyHelper.GetServerVersion(assembly) : "unknown";
        var title = assembly?.GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? "Azure MCP";

        Console.WriteLine($"{title} {version}\n");
    }
}
