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

public class GalleryApplicationVersionCreateCommandTests
{
    [Fact]
    public async Task ExecuteAsync_ReturnsCreatedVersion()
    {
        var computeService = Substitute.For<IComputeService>();
        var logger = Substitute.For<ILogger<GalleryApplicationVersionCreateCommand>>();
        var command = new GalleryApplicationVersionCreateCommand(logger, computeService);
        var parseResult = command.GetCommand().Parse([
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--gallery", "gallery1",
            "--gallery-application", "app1",
            "--gallery-application-version", "1.0.0",
            "--location", "eastus",
            "--source-media-link", "https://example.com/app.zip",
            "--exclude-from-latest", "true",
            "--target-regions", "eastus,westus2"
        ]);

        computeService.CreateGalleryApplicationVersionAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<bool?>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(new GalleryApplicationVersionInfo("1.0.0", "id", "eastus", "https://example.com/app.zip", true, null, ["eastus", "westus2"], null));

        var response = await command.ExecuteAsync(new(new ServiceCollection().BuildServiceProvider()), parseResult, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, ComputeJsonContext.Default.GalleryApplicationVersionCreateCommandResult);
        Assert.NotNull(result);
        Assert.Equal("1.0.0", result.GalleryApplicationVersion.Name);
    }
}
