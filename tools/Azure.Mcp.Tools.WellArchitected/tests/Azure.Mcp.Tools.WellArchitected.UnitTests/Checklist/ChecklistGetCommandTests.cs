// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
using System.Net;
using Azure.Mcp.Tools.WellArchitected.Commands.Checklist;
using Azure.Mcp.Tools.WellArchitected.Models;
using Azure.Mcp.Tools.WellArchitected.Options;
using Azure.Mcp.Tools.WellArchitected.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.WellArchitected.UnitTests.Checklist;

public class ChecklistGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IWellArchitectedService _service;
    private readonly ILogger<ChecklistGetCommand> _logger;
    private readonly ChecklistGetCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public ChecklistGetCommandTests()
    {
        _service = Substitute.For<IWellArchitectedService>();
        _logger = Substitute.For<ILogger<ChecklistGetCommand>>();

        var collection = new ServiceCollection().AddSingleton(_service);
        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.Equal("get", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsChecklist_WhenFound()
    {
        var pillar = "Security";
        var mockChecklist = new WafChecklist(
            pillar,
            [
                new WafChecklistItem("item-1", "Enable encryption", "Encrypt data at rest"),
                new WafChecklistItem("item-2", "Configure firewall", "Set up network restrictions")
            ]
        );

        _service.GetChecklistAsync(pillar, Arg.Any<CancellationToken>())
            .Returns(mockChecklist);

        var parseResult = _commandDefinition.Parse(["--pillar", pillar]);
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        Assert.Empty(response.Message);

        await _service.Received(1).GetChecklistAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNull_WhenPillarNotFound()
    {
        var pillar = "NonexistentPillar";

        _service.GetChecklistAsync(pillar, Arg.Any<CancellationToken>())
            .Returns((WafChecklist?)null);

        var parseResult = _commandDefinition.Parse(["--pillar", pillar]);
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        await _service.Received(1).GetChecklistAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        var pillar = "Security";
        var exception = new InvalidOperationException("Service error");

        _service.GetChecklistAsync(pillar, Arg.Any<CancellationToken>())
            .ThrowsAsync(exception);

        var parseResult = _commandDefinition.Parse(["--pillar", pillar]);
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.NotNull(response.Message);
        Assert.NotEmpty(response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ValidatesInput_Success()
    {
        _service.GetChecklistAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new WafChecklist("TestPillar", []));

        var parseResult = _commandDefinition.Parse(["--pillar", "Security"]);
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ValidatesInput_MissingPillar()
    {
        var parseResult = _commandDefinition.Parse([]);
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        Assert.NotEqual(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesEmptyPillar()
    {
        var parseResult = _commandDefinition.Parse(["--pillar", ""]);
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        Assert.NotEqual(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsChecklistWithMultipleItems()
    {
        var pillar = "Cost Optimization";
        var mockChecklist = new WafChecklist(
            pillar,
            [
                new WafChecklistItem("cost-1", "Right-size resources", "Match resource size to workload"),
                new WafChecklistItem("cost-2", "Use reserved instances", "Purchase reserved capacity"),
                new WafChecklistItem("cost-3", "Enable auto-shutdown", "Stop resources when not in use")
            ]
        );

        _service.GetChecklistAsync(pillar, Arg.Any<CancellationToken>())
            .Returns(mockChecklist);

        var parseResult = _commandDefinition.Parse(["--pillar", pillar]);
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public void BindOptions_BindsOptionsCorrectly()
    {
        var pillar = "Reliability";
        var parseResult = _commandDefinition.Parse(["--pillar", pillar]);

        var options = _command.GetType()
            .GetMethod("BindOptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(_command, [parseResult]) as ChecklistGetOptions;

        Assert.NotNull(options);
        Assert.Equal(pillar, options.Pillar);
    }
}
