// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models;
using Xunit;

namespace Azure.Mcp.Server.Tests.Infrastructure;

/// <summary>
/// Validates that command names referenced in the repository's user-facing documentation
/// (<c>azmcp-commands.md</c> and <c>e2eTestPrompts.md</c>) are present and correct, i.e. they
/// match the commands actually registered by the Azure MCP server.
///
/// This guards against the scenario described in
/// https://github.com/microsoft/mcp/issues/268: a command is added, renamed, or removed in code
/// but the documentation is never updated to match, leaving stale or missing command names for
/// users and AI agents that rely on these files for discovery.
/// </summary>
public sealed class DocumentationCommandCoverageTests
{
    private static readonly string s_repoRoot = GetRepoRoot();

    [Fact]
    public void AzmcpCommandsDoc_Should_Document_Every_Registered_Command()
    {
        var factory = CreateCommandFactory();
        var realCommandPaths = GetRealCommandPaths(factory);
        Assert.True(realCommandPaths.Count > 100, $"Expected 100+ registered commands, found {realCommandPaths.Count}.");

        var docsPath = Path.Combine(s_repoRoot, "servers", "Azure.Mcp.Server", "docs", "azmcp-commands.md");
        var docText = NormalizeLineEndings(File.ReadAllText(docsPath));

        var missing = realCommandPaths
            .Where(path => !DocContainsCommand(docText, path))
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToList();

        Assert.True(missing.Count == 0,
            $"{missing.Count} command(s) are registered in the server but are not documented as an " +
            "'azmcp <command>' usage example in servers/Azure.Mcp.Server/docs/azmcp-commands.md:\n  " +
            string.Join("\n  ", missing) +
            "\n\nAdd a documented example (with the correct command path) for each command above.");
    }

    [Fact]
    public void E2ETestPromptsDoc_Should_Cover_Every_Registered_Tool_With_No_Stale_Names()
    {
        var factory = CreateCommandFactory();
        var realToolNames = GetVisibleCommands(factory.AllCommands)
            .Select(kvp => kvp.Key)
            .ToHashSet(StringComparer.Ordinal);
        Assert.True(realToolNames.Count > 100, $"Expected 100+ registered tools, found {realToolNames.Count}.");

        var docsPath = Path.Combine(s_repoRoot, "servers", "Azure.Mcp.Server", "docs", "e2eTestPrompts.md");
        var docToolNames = ExtractToolNamesFromE2EDoc(docsPath);

        var missing = realToolNames.Except(docToolNames).OrderBy(n => n, StringComparer.Ordinal).ToList();
        var stale = docToolNames.Except(realToolNames).OrderBy(n => n, StringComparer.Ordinal).ToList();

        Assert.True(missing.Count == 0 && stale.Count == 0,
            BuildToolNameMismatchMessage(missing, stale));
    }

    private static string BuildToolNameMismatchMessage(List<string> missing, List<string> stale)
    {
        var message = "servers/Azure.Mcp.Server/docs/e2eTestPrompts.md is out of sync with the registered tools.\n";

        if (missing.Count > 0)
        {
            message += $"\n{missing.Count} tool(s) have no test prompt and must be added:\n  " + string.Join("\n  ", missing) + "\n";
        }

        if (stale.Count > 0)
        {
            message += $"\n{stale.Count} tool name(s) in the doc do not match any registered tool " +
                "(likely renamed or removed) and must be updated or removed:\n  " + string.Join("\n  ", stale) + "\n";
        }

        return message;
    }

    /// <summary>
    /// Builds the exact "azmcp"-relative CLI invocation path (e.g. "storage account get") for every
    /// visible, registered command by walking the actual <see cref="CommandGroup"/> hierarchy.
    ///
    /// This intentionally does NOT use <see cref="ICommandFactory.AllCommands"/>'s underscore-joined
    /// keys (e.g. "storage_account_get") and blindly replace every underscore with a space, because a
    /// handful of command groups/leaf commands (e.g. "get_azure_bestpractices", "send_message") contain
    /// literal underscores that are part of the CLI token itself, not group separators. Walking the
    /// hierarchy directly preserves those literal names and produces the exact string a user would type.
    /// </summary>
    private static List<string> GetRealCommandPaths(ICommandFactory factory)
    {
        var paths = new List<string>();

        foreach (var topGroup in factory.RootGroup.SubGroup)
        {
            CollectCommandPaths(topGroup, prefix: null, paths);
        }

        return paths;
    }

    private static void CollectCommandPaths(CommandGroup group, string? prefix, List<string> paths)
    {
        var currentPrefix = string.IsNullOrEmpty(prefix) ? group.Name : $"{prefix} {group.Name}";

        foreach (var (_, command) in GetVisibleCommands(group.Commands))
        {
            paths.Add($"{currentPrefix} {command.Name}");
        }

        foreach (var subGroup in group.SubGroup)
        {
            CollectCommandPaths(subGroup, currentPrefix, paths);
        }
    }

    /// <summary>
    /// Mirrors the (internal) filtering <see cref="CommandFactory"/> applies before exposing commands
    /// via <c>tools list</c>: commands marked with <see cref="HiddenCommandAttribute"/> are internal CLI
    /// plumbing (e.g. the "tools list" command itself) and are not expected to be documented.
    /// </summary>
    private static IEnumerable<KeyValuePair<string, IBaseCommand>> GetVisibleCommands(IEnumerable<KeyValuePair<string, IBaseCommand>> commands)
        => commands.Where(kvp => kvp.Value.GetType().GetCustomAttribute<HiddenCommandAttribute>() is null);

    private static bool DocContainsCommand(string docText, string commandPath)
    {
        var pattern = $@"(?m)^azmcp[ \t]+{Regex.Escape(commandPath)}(?=[ \t]|\\|$)";
        return Regex.IsMatch(docText, pattern);
    }

    private static HashSet<string> ExtractToolNamesFromE2EDoc(string docsPath)
    {
        var toolNames = new HashSet<string>(StringComparer.Ordinal);

        // Table rows look like: "| storage_account_get | <prompt text> | none |".
        // The header ("| Tool Name | ...") and separator ("|:---|...") rows are excluded because
        // real tool names always start with a lowercase letter and never contain spaces/colons.
        var rowPattern = new Regex(@"^\|\s*([a-z][a-z0-9_\-]*)\s*\|", RegexOptions.Compiled);

        foreach (var line in File.ReadLines(docsPath))
        {
            var match = rowPattern.Match(line);
            if (match.Success)
            {
                toolNames.Add(match.Groups[1].Value);
            }
        }

        return toolNames;
    }

    private static string NormalizeLineEndings(string text) => text.Replace("\r\n", "\n");

    private static ICommandFactory CreateCommandFactory()
    {
        var serviceCollection = new ServiceCollection();
        Program.ConfigureServices(serviceCollection);
        var services = serviceCollection.BuildServiceProvider();
        return services.GetRequiredService<ICommandFactory>();
    }

    private static string GetRepoRoot()
    {
        var currentDir = Directory.GetCurrentDirectory();
        var dir = new DirectoryInfo(currentDir);

        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "global.json")) &&
                Directory.Exists(Path.Combine(dir.FullName, "servers")))
            {
                return dir.FullName;
            }
            dir = dir.Parent;
        }

        throw new InvalidOperationException("Could not find repository root containing global.json and servers directory");
    }
}
