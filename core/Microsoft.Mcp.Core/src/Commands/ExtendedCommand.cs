// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Core.Commands;

/// <summary>
/// Custom extension of System.CommandLine.Command that includes a reference to the underlying IBaseCommand
/// implementation. This allows us to access the strongly-typed options and execution logic defined in the IBaseCommand
/// when handling command invocations, while still leveraging the parsing capabilities of System.CommandLine.
/// </summary>
/// <param name="baseCommand"></param>
/// <param name="name"></param>
/// <param name="description"></param>
public sealed class ExtendedCommand(IBaseCommand baseCommand, string name, string? description)
    : Command(name, description)
{
    /// <summary>
    /// The underlying IBaseCommand implementation that contains the strongly-typed options and execution logic for
    /// this command.
    /// </summary>
    public IBaseCommand BaseCommand => baseCommand;
}