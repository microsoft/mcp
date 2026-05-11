// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.SreAgent.Commands.Architecture;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.SreAgent.UnitTests.Architecture;

public class PlanCommandTests
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = new PlanCommand(Substitute.For<ILogger<PlanCommand>>());
        var def = command.GetCommand();
        Assert.NotNull(def.Name);
        Assert.NotEmpty(def.Name);
        Assert.NotNull(def.Description);
        Assert.NotEmpty(def.Description);
    }

    [Fact]
    public void RegisterOptions_AddsExpectedOptions()
    {
        var command = new PlanCommand(Substitute.For<ILogger<PlanCommand>>());
        var def = command.GetCommand();
        Assert.NotNull(def.Options);
    }
}