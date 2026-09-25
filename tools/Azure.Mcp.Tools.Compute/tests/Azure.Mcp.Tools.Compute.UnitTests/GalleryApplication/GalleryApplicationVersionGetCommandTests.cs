// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.Compute.Commands;
using Azure.Mcp.Tools.Compute.Commands.GalleryApplication;
using Azure.Mcp.Tools.Compute.Models;
using Azure.Mcp.Tools.Compute.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Options;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Compute.UnitTests.GalleryApplication;

public class GalleryApplicationVersionGetCommandTests
{
    private readonly IComputeService _computeService;
    private readonly GalleryApplicationVersionGetCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public GalleryApplicationVersionGetCommandTests()
    {
        _computeService = Substitute.For<IComputeService>();
        var logger = Substitute.For<ILogger<GalleryApplicationVersionGetCommand>>();

        _command = new(logger, _computeService);
        _commandDefinition = _command.GetCommand();
        _context = new(new ServiceCollection().BuildServiceProvider());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsSingleVersion_WhenVersionSpecified()
    {
        _computeService.GetGalleryApplicationVersionAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(new GalleryApplicationVersionInfo("1.0.0", "id", "eastus", "https://example.com/app.zip", true, null, ["eastus"], null));

        var parseResult = _commandDefinition.Parse([
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--gallery", "gallery1",
            "--gallery-application", "app1",
            "--gallery-application-version", "1.0.0"
        ]);

        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, ComputeJsonContext.Default.GalleryApplicationVersionGetCommandResult);
        Assert.NotNull(result);
        Assert.Equal("1.0.0", result.GalleryApplicationVersion.Name);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsVersionList_WhenVersionNotSpecified()
    {
        _computeService.ListGalleryApplicationVersionsAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns([
                new GalleryApplicationVersionInfo("1.0.0", "id1", "eastus", "https://example.com/app-1.0.0.zip", false, null, ["eastus"], null),
                new GalleryApplicationVersionInfo("1.0.1", "id2", "eastus", "https://example.com/app-1.0.1.zip", false, null, ["eastus"], null)
            ]);

        var parseResult = _commandDefinition.Parse([
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--gallery", "gallery1",
            "--gallery-application", "app1"
        ]);

        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, ComputeJsonContext.Default.GalleryApplicationVersionGetListResult);
        Assert.NotNull(result);
        Assert.Equal(2, result.GalleryApplicationVersions.Count);
    }
}
