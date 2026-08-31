// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using Microsoft.Extensions.Options;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Configuration;

namespace Microsoft.Mcp.Core.Commands;

/// <summary>
/// Custom help action that displays version information before the standard help output.
/// </summary>
internal class CustomHelpAction : SynchronousCommandLineAction
{
    private readonly IOptions<McpServerConfiguration> _options;
    private readonly HelpAction _defaultHelp;

    private readonly IAreaSetup[]? _serviceAreas;

    public CustomHelpAction(IOptions<McpServerConfiguration> options, HelpAction action, IAreaSetup[]? serviceAreas = null)
    {
        _options = options;
        _defaultHelp = action;
        _serviceAreas = serviceAreas;
    }

    private static string GetCategoryName(CommandCategory category) => category switch
    {
        CommandCategory.Cli => "CLI",
        CommandCategory.Mcp => "MCP",
        CommandCategory.RecommendedTools => "Recommended Tools",
        CommandCategory.SubscriptionManagement => "Subscription Management",
        CommandCategory.AzureServices => "Azure Services",
        _ => category.ToString()
    };

    public override bool ClearsParseErrors => _defaultHelp.ClearsParseErrors;

    public override int Invoke(ParseResult parseResult)
    {
        var output = parseResult.InvocationConfiguration.Output;
        output.WriteLine($"{_options.Value.Name} {_options.Value.Version}{Environment.NewLine}");

        if (_serviceAreas != null && parseResult.CommandResult.Command is RootCommand rootCommand)
        {
            RenderGroupAreasHelp(rootCommand, output);
            return 0;
        }

        return _defaultHelp.Invoke(parseResult);
    }

    private void RenderGroupAreasHelp(RootCommand rootCommand, TextWriter output)
    {
        const int descriptionColumnWidth = 72;

        var commandColumnWidth = _serviceAreas!
            .Select(a => a.Name.Length).DefaultIfEmpty(20).Max() + 2;

        var indent = new string(' ', commandColumnWidth + 4);

        output.WriteLine($"Description:{Environment.NewLine}  {rootCommand.Description}{Environment.NewLine}");
        output.WriteLine($"Usage:{Environment.NewLine}  {_options.Value.RootCommandGroupName} [command] [options]{Environment.NewLine}");
        output.WriteLine("Options:");
        output.WriteLine("  -?, -h, --help  Show help and usage information");
        output.WriteLine("  --version       Show version information");

        output.WriteLine($"{Environment.NewLine}Examples:");
        output.WriteLine($"  {_options.Value.RootCommandGroupName} storage account get --subscription \"my-sub\"");
        output.WriteLine($"  {_options.Value.RootCommandGroupName} server start");

        var groupedAreas = _serviceAreas!.GroupBy(area => area.Category).OrderBy(g => (int)g.Key);
        foreach (var group in groupedAreas)
        {
            output.WriteLine($"\n{GetCategoryName(group.Key)}:");
            foreach (var commandArea in group.OrderBy(a => a.Name))
            {
                var subCommand = rootCommand.Subcommands.FirstOrDefault(c => c.Name.Equals(commandArea.Name));
                if (subCommand != null)
                {
                    var description = subCommand.Description ?? string.Empty;
                    var wrappedDescription = WrapDescription(description, descriptionColumnWidth, indent);
                    output.WriteLine($"  {commandArea.Name.PadRight(commandColumnWidth)}  {wrappedDescription}");
                }
            }
        }
    }

    private static string WrapDescription(string description, int maxWidth, string indent)
    {
        if (string.IsNullOrEmpty(description))
            return string.Empty;

        var words = description.Split([' ', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        var lines = new List<string>();
        var line = "";

        foreach (var word in words)
        {
            var test = line.Length == 0 ? word : $"{line} {word}";
            if (test.Length <= maxWidth)
                line = test;
            else
            {
                if (line.Length > 0)
                    lines.Add(line);
                line = word;
            }
        }
        if (line.Length > 0)
        {
            lines.Add(line);
        }

        return string.Join(Environment.NewLine + indent, lines);
    }
}
