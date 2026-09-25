// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.Compute.Commands;
using Azure.Mcp.Tools.Compute.Commands.GalleryApplication;
using Azure.Mcp.Tools.Compute.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Compute.UnitTests.GalleryApplication;

public class GalleryApplicationDeleteCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IComputeService _computeService;
    private readonly GalleryApplicationDeleteCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public GalleryApplicationDeleteCommandTests()
    {
        _computeService = Substitute.For<IComputeService>();
        var logger = Substitute.For<ILogger<GalleryApplicationDeleteCommand>>();

        var collection = new ServiceCollection().AddSingleton(_computeService);
        _serviceProvider = collection.BuildServiceProvider();

        _command = new(logger, _computeService);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("delete", _command.Name);
        Assert.NotNull(_command.Description);
        Assert.NotEmpty(_command.Description);
    }

    [Theory]
    [InlineData("--subscription sub123 --resource-group rg1 --gallery gallery1 --gallery-application app1", true)]
    [InlineData("--subscription sub123 --resource-group rg1 --gallery gallery1", false)]
    [InlineData("--subscription sub123 --resource-group rg1 --gallery-application app1", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            _computeService.DeleteGalleryApplicationAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns(true);
        }

        var parseResult = _commandDefinition.Parse(args);
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsDeletedTrue_WhenApplicationDeleted()
    {
        _computeService.DeleteGalleryApplicationAsync(
            Arg.Is("gallery1"),
            Arg.Is("app1"),
            Arg.Is("rg1"),
            Arg.Is("sub123"),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(true);

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
        var result = JsonSerializer.Deserialize(json, ComputeJsonContext.Default.GalleryApplicationDeleteCommandResult);

        Assert.NotNull(result);
        Assert.True(result.Deleted);
        Assert.Equal("app1", result.GalleryApplication);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsDeletedFalse_WhenApplicationNotFound()
    {
        _computeService.DeleteGalleryApplicationAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(false);

        var parseResult = _commandDefinition.Parse([
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--gallery", "gallery1",
            "--gallery-application", "missing-app"
        ]);

        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, ComputeJsonContext.Default.GalleryApplicationDeleteCommandResult);

        Assert.NotNull(result);
        Assert.False(result.Deleted);
        Assert.Equal("missing-app", result.GalleryApplication);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesForbiddenError()
    {
        var forbiddenException = new RequestFailedException((int)HttpStatusCode.Forbidden, "Insufficient permissions");

        _computeService.DeleteGalleryApplicationAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(forbiddenException);

        var parseResult = _commandDefinition.Parse([
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--gallery", "gallery1",
            "--gallery-application", "app1"
        ]);

        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Authorization failed", response.Message, StringComparison.OrdinalIgnoreCase);
    }
}
