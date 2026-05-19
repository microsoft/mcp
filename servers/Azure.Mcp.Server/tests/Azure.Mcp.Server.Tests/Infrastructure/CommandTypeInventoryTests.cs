// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Commands;
using Xunit;

namespace Azure.Mcp.Server.Tests.Infrastructure;

/// <summary>
/// Reflects over all IBaseCommand implementations to produce a JSON inventory
/// of command/option class hierarchies and exposed options.
/// Run this test to regenerate .work/tool-conversion-checklist.json.
/// </summary>
public sealed class CommandTypeInventoryTests
{
    [Fact]
    public async Task GenerateToolConversionChecklist()
    {
        // Use Program.ConfigureServices which registers everything properly
        var serviceCollection = new ServiceCollection();
        Program.ConfigureServices(serviceCollection);
        await using var services = serviceCollection.BuildServiceProvider();

        // Resolve all registered IBaseCommand implementations from DI (tools only)
        var commandTypes = serviceCollection
            .Where(sd => sd.ServiceType != null
                && typeof(IBaseCommand).IsAssignableFrom(sd.ServiceType)
                && (sd.ServiceType.Assembly.GetName().Name?.StartsWith("Azure.Mcp.Tools") == true))
            .Select(sd => sd.ServiceType)
            .ToList();

        var commands = commandTypes
            .Select(t => (IBaseCommand)services.GetRequiredService(t))
            .ToList();

        var inventory = new SortedDictionary<string, List<Dictionary<string, object>>>();

        foreach (var command in commands)
        {
            var commandType = command.GetType();
            var assemblyName = commandType.Assembly.GetName().Name ?? "Unknown";

            if (!assemblyName.StartsWith("Azure.Mcp.Tools"))
                continue;

            if (!inventory.TryGetValue(assemblyName, out var entries))
            {
                entries = [];
                inventory[assemblyName] = entries;
            }

            var commandHierarchy = GetTypeHierarchy(commandType);
            var optionsType = ExtractOptionsType(commandType);
            var optionsHierarchy = optionsType != null ? GetTypeHierarchy(optionsType) : [];

            // Get exposed options from the live System.CommandLine.Command instance
            var sclCommand = command.GetCommand();
            var exposedOptions = sclCommand.Options
                .Where(static o => o is not System.CommandLine.Help.HelpOption && o.Name != "learn")
                .Select(static o => new Dictionary<string, object>
                {
                    ["name"] = o.Name.StartsWith("--") ? o.Name : $"--{o.Name}",
                    ["required"] = o.Required,
                })
                .ToList();

            entries.Add(new Dictionary<string, object>
            {
                ["commandClass"] = commandType.Name,
                ["id"] = command.Id,
                ["pattern"] = IsNewPattern(commandType) ? "two-generic" : "one-generic",
                ["commandHierarchy"] = commandHierarchy,
                ["optionsClass"] = optionsType?.Name ?? "unknown",
                ["optionsHierarchy"] = optionsHierarchy,
                ["exposedOptions"] = exposedOptions,
            });
        }

        var json = JsonSerializer.Serialize(inventory, new JsonSerializerOptions { WriteIndented = true });

        var workDir = Path.Combine(GetRepoRoot(), ".work");
        Directory.CreateDirectory(workDir);
        File.WriteAllText(Path.Combine(workDir, "tool-conversion-checklist.json"), json);

        Assert.NotEmpty(inventory);
        Assert.True(commands.Count > 100, $"Expected 100+ commands, found {commands.Count}");
    }

    private static List<string> GetTypeHierarchy(Type type)
    {
        var hierarchy = new List<string>();
        var current = type;
        while (current != null && current != typeof(object))
        {
            hierarchy.Add(FormatTypeName(current));
            current = current.BaseType;
        }
        return hierarchy;
    }

    private static string FormatTypeName(Type type)
    {
        if (!type.IsGenericType)
            return type.Name;

        var name = type.Name;
        var backtick = name.IndexOf('`');
        if (backtick > 0)
            name = name[..backtick];

        var args = type.GetGenericArguments().Select(FormatTypeName);
        return $"{name}<{string.Join(", ", args)}>";
    }

    private static Type? ExtractOptionsType(Type commandType)
    {
        var current = commandType;
        while (current != null && current != typeof(object))
        {
            if (current.IsGenericType)
            {
                var genericDef = current.GetGenericTypeDefinition();
                if (genericDef.FullName is "Microsoft.Mcp.Core.Commands.BaseCommand`1"
                                        or "Microsoft.Mcp.Core.Commands.BaseCommand`2")
                {
                    return current.GetGenericArguments()[0];
                }
            }
            current = current.BaseType;
        }
        return null;
    }

    private static bool IsNewPattern(Type commandType)
    {
        var current = commandType;
        while (current != null && current != typeof(object))
        {
            if (current.IsGenericType)
            {
                var genericDef = current.GetGenericTypeDefinition();
                if (genericDef.FullName == "Microsoft.Mcp.Core.Commands.BaseCommand`2")
                    return true;
                if (genericDef.FullName == "Microsoft.Mcp.Core.Commands.BaseCommand`1")
                    return false;
            }
            current = current.BaseType;
        }
        return false;
    }

    private static string GetRepoRoot()
    {
        var dir = AppContext.BaseDirectory;
        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir, "Microsoft.Mcp.slnx")))
                return dir;
            dir = Directory.GetParent(dir)?.FullName;
        }
        throw new InvalidOperationException("Could not find repo root (Microsoft.Mcp.slnx)");
    }
}
