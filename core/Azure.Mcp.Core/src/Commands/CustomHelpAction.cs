// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using Azure.Mcp.Core.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Mcp.Core.Areas;

namespace Azure.Mcp.Core.Commands;

/// <summary>
/// Custom help action that displays version information before the standard help output.
/// </summary>
internal class CustomHelpAction : SynchronousCommandLineAction
{
    private readonly IOptions<AzureMcpServerConfiguration> _options;
    private readonly HelpAction _defaultHelp;

    private readonly IAreaSetup[]? _serviceAreas;

    public CustomHelpAction(IOptions<AzureMcpServerConfiguration> options, HelpAction action, IAreaSetup[]? serviceAreas = null)
    {
        _options = options;
        _defaultHelp = action;
        _serviceAreas = serviceAreas;
    }

    private static readonly Dictionary<CommandCategory, string> _categoryNames = new()
    {
        [CommandCategory.Cli] = "CLI",
        [CommandCategory.Mcp] = "MCP",
        [CommandCategory.RecommendedTools] = "Recommended Tools",
        [CommandCategory.SubscriptionManagement] = "Subscription Management",
        [CommandCategory.AzureServices] = "Azure Services"
    };

    public override int Invoke(ParseResult parseResult)
    {
        Console.WriteLine($"{_options.Value.Name} {_options.Value.Version}{Environment.NewLine}");

        if (_serviceAreas != null && parseResult.CommandResult.Command is RootCommand rootCommand)
        {
            RenderGroupAreasHelp(rootCommand);
            return 0;
        }

        return _defaultHelp.Invoke(parseResult);
    }

    private void RenderGroupAreasHelp(RootCommand rootCommand)
    {
        const int descriptionColumnWidth = 72;

        var commandColumnWidth = _serviceAreas!
            .Select(a => a.Name.Length).DefaultIfEmpty(20).Max() + 2;

        var indent = new string(' ', commandColumnWidth + 4);

        Console.WriteLine($"Description:{Environment.NewLine}  {rootCommand.Description}{Environment.NewLine}");
        Console.WriteLine($"Usage:{Environment.NewLine}  {_options.Value.RootCommandGroupName} [command] [options]{Environment.NewLine}");
        Console.WriteLine("Options:");
        Console.WriteLine("  -?, -h, --help  Show help and usage information");
        Console.WriteLine("  --version       Show version information");

        Console.WriteLine($"{Environment.NewLine}Examples:");
        Console.WriteLine($"  {_options.Value.RootCommandGroupName} storage account get --subscription \"my-sub\"");
        Console.WriteLine($"  {_options.Value.RootCommandGroupName} server start");

        var groupedAreas = _serviceAreas!.GroupBy(area => area.Category).OrderBy(g => (int)g.Key);
        foreach (var group in groupedAreas)
        {
            Console.WriteLine($"\n{_categoryNames[group.Key]}:");
            foreach (var commandArea in group.OrderBy(a => a.Name))
            {
                var subCommand = rootCommand.Subcommands.FirstOrDefault(c => c.Name.Equals(commandArea.Name));
                if (subCommand != null)
                {
                    var description = subCommand.Description ?? string.Empty;
                    var wrappedDescription = WrapDescription(description, descriptionColumnWidth, indent);
                    Console.WriteLine($"  {commandArea.Name.PadRight(commandColumnWidth)}  {wrappedDescription}");
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
