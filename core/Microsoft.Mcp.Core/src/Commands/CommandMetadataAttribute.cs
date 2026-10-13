// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Core.Commands;

/// <summary>
/// Declares command metadata (Id, Name, Description, Title) and tool behavioral hints
/// directly on a command class. <see cref="BaseCommand{TOptions, TResult}"/> reads this attribute
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

    /// <summary>The API plane the tool acts against.</summary>
    public required ToolOperationPlane OperationPlane { get; init; }

    /// <summary>Whether the tool may perform destructive updates.</summary>
    public required bool Destructive { get; init; }

    /// <summary>Whether repeated calls with the same arguments have no additional effect.</summary>
    public required bool Idempotent { get; init; }

    /// <summary>Whether the tool may interact with an open world of external entities.</summary>
    public required bool OpenWorld { get; init; }

    /// <summary>Whether the tool only performs read operations.</summary>
    public required bool ReadOnly { get; init; }

    /// <summary>Whether the tool handles sensitive or secret information.</summary>
    public required bool Secret { get; init; }

    /// <summary>Whether the tool requires local execution.</summary>
    public required bool LocalRequired { get; init; }

    internal bool IsValid() => !string.IsNullOrWhiteSpace(Id) &&
        !string.IsNullOrWhiteSpace(Name) &&
        !string.IsNullOrWhiteSpace(Description) &&
        !string.IsNullOrWhiteSpace(Title);

    internal ToolMetadata ToToolMetadata() => new()
    {
        OperationPlane = OperationPlane,
        Destructive = Destructive,
        Idempotent = Idempotent,
        OpenWorld = OpenWorld,
        ReadOnly = ReadOnly,
        Secret = Secret,
        LocalRequired = LocalRequired
    };
}
