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
using Microsoft.Mcp.Core.Options;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Compute.UnitTests.GalleryApplication;

public class GalleryApplicationVersionUpdateCommandTests
{
    [Fact]
    public async Task ExecuteAsync_ReturnsUpdatedVersion()
    {
        var computeService = Substitute.For<IComputeService>();
        var logger = Substitute.For<ILogger<GalleryApplicationVersionUpdateCommand>>();
        var command = new GalleryApplicationVersionUpdateCommand(logger, computeService);
        var parseResult = command.GetCommand().Parse([
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--gallery", "gallery1",
            "--gallery-application", "app1",
            "--gallery-application-version", "1.0.0",
            "--end-of-life-on", "2030-01-01T00:00:00Z"
        ]);

        computeService.UpdateGalleryApplicationVersionAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<bool?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(new GalleryApplicationVersionInfo("1.0.0", "id", "eastus", "https://example.com/app.zip", false, "2030-01-01T00:00:00.0000000Z", ["eastus"], null));

        var response = await command.ExecuteAsync(new(new ServiceCollection().BuildServiceProvider()), parseResult, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, ComputeJsonContext.Default.GalleryApplicationVersionUpdateCommandResult);
        Assert.NotNull(result);
        Assert.Equal("1.0.0", result.GalleryApplicationVersion.Name);
    }
}
