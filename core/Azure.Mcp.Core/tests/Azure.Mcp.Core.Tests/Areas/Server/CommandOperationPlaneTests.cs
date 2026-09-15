// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Commands;
using Xunit;

namespace Azure.Mcp.Core.Tests.Areas.Server;

/// <summary>
/// Enforces that every registered tool declares an operation plane.
/// </summary>
/// <remarks>
/// Every tool must explicitly declare a defined operation plane on its
/// <see cref="CommandMetadataAttribute"/> before it can ship.
/// A tool that calls no service is <see cref="ToolOperationPlane.NotApplicable"/>, which
/// is an explicit classification and satisfies this test.
/// See <c>docs/design/operation-plane-metadata.md</c> for the classification rule.
/// </remarks>
public class CommandOperationPlaneTests
{
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
