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

public class GalleryApplicationCreateCommandTests
{
    private readonly IComputeService _computeService;
    private readonly GalleryApplicationCreateCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public GalleryApplicationCreateCommandTests()
    {
        _computeService = Substitute.For<IComputeService>();
        var logger = Substitute.For<ILogger<GalleryApplicationCreateCommand>>();

        _command = new(logger, _computeService);
        _commandDefinition = _command.GetCommand();

        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        _context = new(serviceProvider);
    }

    [Theory]
    [InlineData("--subscription sub123 --resource-group rg1 --gallery gallery1 --gallery-application app1 --location eastus", true)]
    [InlineData("--subscription sub123 --resource-group rg1 --gallery gallery1 --location eastus", false)]
    [InlineData("--subscription sub123 --resource-group rg1 --gallery-application app1 --location eastus", false)]
    [InlineData("--subscription sub123 --resource-group rg1 --gallery gallery1 --gallery-application app1", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            _computeService.CreateGalleryApplicationAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns(new GalleryApplicationInfo("app1", "id", "eastus", null));
        }

        var parseResult = _commandDefinition.Parse(args);
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsCreatedGalleryApplication()
    {
        _computeService.CreateGalleryApplicationAsync(
            Arg.Is("gallery1"),
            Arg.Is("app1"),
            Arg.Is("rg1"),
            Arg.Is("sub123"),
            Arg.Is("eastus"),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(new GalleryApplicationInfo("app1", "id", "eastus", null));

        var parseResult = _commandDefinition.Parse([
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--gallery", "gallery1",
            "--gallery-application", "app1",
            "--location", "eastus"
        ]);

        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, ComputeJsonContext.Default.GalleryApplicationCreateCommandResult);
        Assert.NotNull(result);
        Assert.Equal("app1", result.GalleryApplication.Name);
    }
}
