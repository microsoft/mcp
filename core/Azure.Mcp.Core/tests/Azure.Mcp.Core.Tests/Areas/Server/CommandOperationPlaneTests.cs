// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Commands;
using Xunit;

namespace Azure.Mcp.Core.Tests.Areas.Server;

/// <summary>
/// Enforces that every registered command declares an operation plane.
/// </summary>
/// <remarks>
/// There is deliberately no allowlist. <see cref="ToolOperationPlane.Unspecified"/> is an unset
/// marker rather than a valid answer, so a new command must be classified before it can ship.
/// A command that calls no Azure service is <see cref="ToolOperationPlane.NotApplicable"/>, which
/// is an explicit classification and satisfies this test.
/// See <c>docs/design/operation-plane-metadata.md</c> for the classification rule.
/// </remarks>
public class CommandOperationPlaneTests
{
    [Fact]
    public void AllCommands_DeclareAnOperationPlane()
    {
        var commandFactory = CommandFactoryHelpers.CreateCommandFactory();

        var unclassified = commandFactory.AllCommands
            .Where(entry => entry.Value.Metadata.OperationPlane == ToolOperationPlane.Unspecified)
            .Select(entry => $"{entry.Key} ({entry.Value.GetType().FullName})")
            .Order()
            .ToList();

        Assert.True(unclassified.Count == 0,
            $"{unclassified.Count} command(s) do not declare an OperationPlane. Set OperationPlane on the " +
            $"[CommandMetadata] attribute. Classify by the command's deliverable: the call that produces what " +
            $"the user asked for. An ARM call made only to locate the target does not count. See " +
            $"docs/design/operation-plane-metadata.md.\n" +
            string.Join("\n", unclassified));
    }

    [Fact]
    public void AllCommands_DeclareAKnownOperationPlane()
    {
        var commandFactory = CommandFactoryHelpers.CreateCommandFactory();

        var unknown = commandFactory.AllCommands
            .Where(entry => !Enum.IsDefined(entry.Value.Metadata.OperationPlane))
            .Select(entry => $"{entry.Key} => {(int)entry.Value.Metadata.OperationPlane}")
            .Order()
            .ToList();

        Assert.True(unknown.Count == 0,
            "The following commands declare an OperationPlane value that is not defined on the enum:\n" +
            string.Join("\n", unknown));
    }

    /// <summary>
    /// Guards against a regression that classifies everything the same way, which would leave the
    /// annotation present but useless for distinguishing tools.
    /// </summary>
    [Fact]
    public void Classification_DistinguishesDataFromControl()
    {
        var commandFactory = CommandFactoryHelpers.CreateCommandFactory();

        var planes = commandFactory.AllCommands.Select(entry => entry.Value.Metadata.OperationPlane).ToList();

        Assert.Contains(ToolOperationPlane.Data, planes);
        Assert.Contains(ToolOperationPlane.Control, planes);
    }
}
