// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Core.Commands;

/// <summary>
/// Declares command metadata (Id, Name, Description, Title) and tool behavioral hints
/// directly on a command class. <see cref="BaseCommand{TOptions}"/> reads this attribute
/// via reflection in its constructor, eliminating the need for override properties.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class CommandMetadataAttribute : Attribute
{
    /// <summary>
    /// A unique identifier for the command (GUID string).
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// The command name used in the CLI path (e.g. "get", "list").
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// The command description shown to AI agents and in help text.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// A human-readable title for the command.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>Whether the tool may perform destructive updates. Default: true.</summary>
    public bool Destructive { get; init; } = true;

    /// <summary>Whether repeated calls with the same arguments have no additional effect. Default: false.</summary>
    public bool Idempotent { get; init; }

    /// <summary>Whether the tool may interact with an open world of external entities. Default: true.</summary>
    public bool OpenWorld { get; init; } = true;

    /// <summary>Whether the tool only performs read operations. Default: false.</summary>
    public bool ReadOnly { get; init; }

    /// <summary>Whether the tool handles sensitive or secret information. Default: false.</summary>
    public bool Secret { get; init; }

    /// <summary>Whether the tool requires local execution. Default: false.</summary>
    public bool LocalRequired { get; init; }

    internal ToolMetadata ToToolMetadata() => new()
    {
        Destructive = Destructive,
        Idempotent = Idempotent,
        OpenWorld = OpenWorld,
        ReadOnly = ReadOnly,
        Secret = Secret,
        LocalRequired = LocalRequired
    };
}
