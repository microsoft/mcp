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
using Microsoft.Mcp.Core.Options;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Compute.UnitTests.GalleryApplication;

public class GalleryApplicationVersionDeleteCommandTests
{
    [Fact]
    public async Task ExecuteAsync_ReturnsDeletedResult()
    {
        var computeService = Substitute.For<IComputeService>();
        var logger = Substitute.For<ILogger<GalleryApplicationVersionDeleteCommand>>();
        var command = new GalleryApplicationVersionDeleteCommand(logger, computeService);
        var parseResult = command.GetCommand().Parse([
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--gallery", "gallery1",
            "--gallery-application", "app1",
            "--gallery-application-version", "1.0.0"
        ]);

        computeService.DeleteGalleryApplicationVersionAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var response = await command.ExecuteAsync(new(new ServiceCollection().BuildServiceProvider()), parseResult, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, ComputeJsonContext.Default.GalleryApplicationVersionDeleteCommandResult);
        Assert.NotNull(result);
        Assert.True(result.Deleted);
    }
}
