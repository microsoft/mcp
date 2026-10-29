// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Frozen;
using Microsoft.Mcp.Core.Commands;

namespace Microsoft.Mcp.Core.Areas.Server.Commands.ToolLoading;

internal sealed class NamespaceCommandCatalog(
    FrozenDictionary<string, NamespaceCommand> commands,
    FrozenDictionary<string, NamespaceCommand> routes)
{
    public IReadOnlyDictionary<string, NamespaceCommand> Commands { get; } = commands;

    public static NamespaceCommandCatalog Create(
        string namespaceName,
        IReadOnlyDictionary<string, CommandRegistration> registrations,
        bool useRelativeNames)
    {
        var commands = new Dictionary<string, NamespaceCommand>(StringComparer.OrdinalIgnoreCase);
        var routes = new Dictionary<string, NamespaceCommand>(StringComparer.OrdinalIgnoreCase);
        var prefix = namespaceName + CommandFactory.Separator;

        foreach (var (registeredName, registration) in registrations)
        {
            var relativeName = useRelativeNames && registeredName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                ? registeredName[prefix.Length..]
                : registeredName;
            var name = relativeName;
            if (useRelativeNames && registration.Command.NamespaceOperationName is { } operation)
            {
                var originalOperation = registration.Command.Name;
                if (string.IsNullOrEmpty(originalOperation) ||
                    !(relativeName.Equals(originalOperation, StringComparison.OrdinalIgnoreCase) ||
                      relativeName.EndsWith(CommandFactory.Separator + originalOperation, StringComparison.OrdinalIgnoreCase)))
                {
                    throw new InvalidOperationException($"Cannot replace operation '{originalOperation}' in namespace command '{registeredName}'.");
                }

                name = relativeName[..^originalOperation.Length] + operation;
            }

            var command = new NamespaceCommand(name, registeredName, registration);
            AddRoute(routes, namespaceName, name, command);
            AddRoute(routes, namespaceName, registeredName, command);
            AddRoute(routes, namespaceName, relativeName, command);
            commands.Add(name, command);
        }

        return new(commands.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase),
            routes.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase));
    }

    public NamespaceCommand? Resolve(string? name)
        => name is not null && routes.TryGetValue(name, out var command) ? command : null;

    private static void AddRoute(
        Dictionary<string, NamespaceCommand> routes,
        string namespaceName,
        string name,
        NamespaceCommand command)
    {
        if (routes.TryGetValue(name, out var existing) && !ReferenceEquals(existing, command))
        {
            throw new InvalidOperationException(
                $"Namespace '{namespaceName}' command name or alias '{name}' collides between '{existing.RegisteredName}' and '{command.RegisteredName}'.");
        }

        routes[name] = command;
    }
}
