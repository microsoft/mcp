// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Core.Commands;

/// <summary>
/// Options that control <see cref="CommandFactory"/> startup behavior.
/// </summary>
public class CommandFactoryOptions
{
    /// <summary>
    /// When set, only the area matching this name will have its commands fully initialized.
    /// This is a startup optimization: for single-command CLI invocations the first CLI
    /// token identifies the target service area, so there is no need to eagerly construct
    /// the remaining 250+ commands from all other areas.
    /// When <see langword="null"/> (the default), all areas are initialized — which is
    /// required for <c>server start</c>, root-level help, and <c>--learn</c> with no prefix.
    /// </summary>
    public string? TargetAreaName { get; set; }
}
