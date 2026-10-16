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

public class GalleryApplicationGetCommandTests
{
    private readonly IComputeService _computeService;
    private readonly GalleryApplicationGetCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public GalleryApplicationGetCommandTests()
    {
        _computeService = Substitute.For<IComputeService>();
        var logger = Substitute.For<ILogger<GalleryApplicationGetCommand>>();

        _command = new(logger, _computeService);
        _commandDefinition = _command.GetCommand();

        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        _context = new(serviceProvider);
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("get", _command.Name);
        Assert.NotNull(_command.Description);
        Assert.NotEmpty(_command.Description);
    }

    [Theory]
    [InlineData("--subscription sub123 --resource-group rg1 --gallery gallery1 --gallery-application app1", true)]
    [InlineData("--subscription sub123 --resource-group rg1 --gallery gallery1", true)]
    [InlineData("--subscription sub123 --resource-group rg1 --gallery-application app1", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            _computeService.GetGalleryApplicationAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns(new GalleryApplicationInfo(
                    Name: "app1",
                    Id: "/subscriptions/sub123/resourceGroups/rg1/providers/Microsoft.Compute/galleries/gallery1/applications/app1",
                    Location: "eastus",
                    Tags: new Dictionary<string, string> { ["env"] = "test" }));

            _computeService.ListGalleryApplicationsAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns([
                    new GalleryApplicationInfo(
                        Name: "app1",
                        Id: "/subscriptions/sub123/resourceGroups/rg1/providers/Microsoft.Compute/galleries/gallery1/applications/app1",
                        Location: "eastus",
                        Tags: new Dictionary<string, string> { ["env"] = "test" })
                ]);
        }

        var parseResult = _commandDefinition.Parse(args);

        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsGalleryApplicationResult()
    {
        var expected = new GalleryApplicationInfo(
            Name: "app1",
            Id: "/subscriptions/sub123/resourceGroups/rg1/providers/Microsoft.Compute/galleries/gallery1/applications/app1",
            Location: "eastus",
            Tags: new Dictionary<string, string> { ["team"] = "compute" });

        _computeService.GetGalleryApplicationAsync(
            Arg.Is("gallery1"),
            Arg.Is("app1"),
            Arg.Is("rg1"),
            Arg.Is("sub123"),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expected);

        var parseResult = _commandDefinition.Parse([
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--gallery", "gallery1",
            "--gallery-application", "app1"
        ]);

        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, ComputeJsonContext.Default.GalleryApplicationGetCommandResult);

        Assert.NotNull(result);
        Assert.Equal("app1", result.GalleryApplication.Name);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsGalleryApplicationList_WhenGalleryApplicationNotProvided()
    {
        _computeService.ListGalleryApplicationsAsync(
            Arg.Is("gallery1"),
            Arg.Is("rg1"),
            Arg.Is("sub123"),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns([
                new GalleryApplicationInfo(
                    Name: "app1",
                    Id: "/subscriptions/sub123/resourceGroups/rg1/providers/Microsoft.Compute/galleries/gallery1/applications/app1",
                    Location: "eastus",
                    Tags: null),
                new GalleryApplicationInfo(
                    Name: "app2",
                    Id: "/subscriptions/sub123/resourceGroups/rg1/providers/Microsoft.Compute/galleries/gallery1/applications/app2",
                    Location: "eastus",
                    Tags: null)
            ]);

        var parseResult = _commandDefinition.Parse([
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--gallery", "gallery1"
        ]);

        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, ComputeJsonContext.Default.GalleryApplicationGetListResult);

        Assert.NotNull(result);
        Assert.Equal(2, result.GalleryApplications.Count);
        Assert.Equal("app1", result.GalleryApplications[0].Name);
        Assert.Equal("app2", result.GalleryApplications[1].Name);
    }
}
