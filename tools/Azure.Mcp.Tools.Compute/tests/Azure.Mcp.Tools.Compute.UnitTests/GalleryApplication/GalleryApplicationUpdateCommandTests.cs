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

public class GalleryApplicationUpdateCommandTests
{
    private readonly IComputeService _computeService;
    private readonly GalleryApplicationUpdateCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public GalleryApplicationUpdateCommandTests()
    {
        _computeService = Substitute.For<IComputeService>();
        var logger = Substitute.For<ILogger<GalleryApplicationUpdateCommand>>();

        _command = new(logger, _computeService);
        _commandDefinition = _command.GetCommand();

        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        _context = new(serviceProvider);
    }

    [Theory]
    [InlineData("--subscription sub123 --resource-group rg1 --gallery gallery1 --gallery-application app1", true)]
    [InlineData("--subscription sub123 --resource-group rg1 --gallery gallery1", false)]
    [InlineData("--subscription sub123 --resource-group rg1 --gallery-application app1", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            _computeService.UpdateGalleryApplicationAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
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
    public async Task ExecuteAsync_ReturnsUpdatedGalleryApplication()
    {
        _computeService.UpdateGalleryApplicationAsync(
            Arg.Is("gallery1"),
            Arg.Is("app1"),
            Arg.Is("rg1"),
            Arg.Is("sub123"),
            Arg.Is<string?>(x => x == "eastus2"),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(new GalleryApplicationInfo("app1", "id", "eastus2", null));

        var parseResult = _commandDefinition.Parse([
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--gallery", "gallery1",
            "--gallery-application", "app1",
            "--location", "eastus2"
        ]);

        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, ComputeJsonContext.Default.GalleryApplicationUpdateCommandResult);
        Assert.NotNull(result);
        Assert.Equal("app1", result.GalleryApplication.Name);
        Assert.Equal("eastus2", result.GalleryApplication.Location);
    }
}
