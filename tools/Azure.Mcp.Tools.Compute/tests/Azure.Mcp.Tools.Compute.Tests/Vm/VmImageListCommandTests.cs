// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Compute.Commands;
using Azure.Mcp.Tools.Compute.Commands.Vm;
using Azure.Mcp.Tools.Compute.Models;
using Azure.Mcp.Tools.Compute.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Compute.UnitTests.Vm;

public class VmImageListCommandTests : CommandUnitTestsBase<VmImageListCommand, IComputeService>
{
    private readonly string _knownSubscription = "sub123";
    private readonly string _knownLocation = "eastus";

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("list-images", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--subscription sub123 --location eastus", true)]
    [InlineData("--subscription sub123 --location eastus --alias Ubuntu2404", true)]
    [InlineData("--subscription sub123 --location eastus --publisher Canonical --offer ubuntu-24_04-lts --image-sku server", true)]
    [InlineData("--subscription sub123", false)] // missing location
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.ListVmImagesAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<int?>(),
                Arg.Any<bool>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns(new List<VmImageInfo>());
        }

        var response = await ExecuteCommandAsync(args);
        if (shouldSucceed)
        {
            Assert.Equal(HttpStatusCode.OK, response.Status);
            Assert.NotNull(response.Results);
        }
        else
        {
            Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        }
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsImages()
    {
        var images = new List<VmImageInfo>
        {
            new("Ubuntu2404", "Canonical", "ubuntu-24_04-lts", "server", "latest",
                "Canonical:ubuntu-24_04-lts:server:latest", "Linux", "V2", "alias"),
        };

        Service.ListVmImagesAsync(
            Arg.Is(_knownSubscription),
            Arg.Is(_knownLocation),
            Arg.Is("Ubuntu2404"),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<bool>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(images);

        var response = await ExecuteCommandAsync(
            "--subscription", _knownSubscription,
            "--location", _knownLocation,
            "--alias", "Ubuntu2404");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        var result = ValidateAndDeserializeResponse(response, ComputeJsonContext.Default.VmImageListCommandResult);
        Assert.Single(result.Images);
        Assert.Equal("Ubuntu2404", result.Images[0].Alias);
    }

    [Fact]
    public async Task ExecuteAsync_PassesPublisherOfferSkuToService()
    {
        Service.ListVmImagesAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<bool>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(new List<VmImageInfo>());

        await ExecuteCommandAsync(
            "--subscription", _knownSubscription,
            "--location", _knownLocation,
            "--publisher", "Canonical",
            "--offer", "ubuntu-24_04-lts",
            "--image-sku", "server",
            "--top", "3");

        await Service.Received(1).ListVmImagesAsync(
            _knownSubscription,
            _knownLocation,
            null,
            "Canonical",
            "ubuntu-24_04-lts",
            "server",
            3,
            false,
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesNotFoundError()
    {
        Service.ListVmImagesAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<bool>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.NotFound, "Not found"));

        var response = await ExecuteCommandAsync(
            "--subscription", _knownSubscription,
            "--location", _knownLocation,
            "--publisher", "Bogus",
            "--offer", "missing",
            "--image-sku", "none");

        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("not found", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_PassesIncludeSharedGalleryFlag()
    {
        // Flag present: --include-shared-gallery should flow through to the service as true.
        Service.ListVmImagesAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<bool>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(new List<VmImageInfo>());

        await ExecuteCommandAsync(
            "--subscription", _knownSubscription,
            "--location", _knownLocation,
            "--include-shared-gallery");

        await Service.Received(1).ListVmImagesAsync(
            _knownSubscription,
            _knownLocation,
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            true,
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_DefaultsIncludeSharedGalleryToFalse()
    {
        Service.ListVmImagesAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<bool>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(new List<VmImageInfo>());

        await ExecuteCommandAsync(
            "--subscription", _knownSubscription,
            "--location", _knownLocation);

        await Service.Received(1).ListVmImagesAsync(
            _knownSubscription,
            _knownLocation,
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            false,
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsSharedGalleryImageWithReferenceId()
    {
        // Verifies a Compute Gallery image surfaces through with its sharedGallery resource ID
        // populated, so the unified-create flow can hand that ID directly back to --image.
        var images = new List<VmImageInfo>
        {
            new("MyGoldenUbuntu", "Canonical", "ubuntu-24_04-lts", "server", "1.0.0",
                "/sharedGalleries/contoso-gallery-id/images/golden-ubuntu/versions/1.0.0",
                "Linux", "V2", "sharedGallery"),
        };

        Service.ListVmImagesAsync(
            Arg.Is(_knownSubscription),
            Arg.Is(_knownLocation),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Is(true),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(images);

        var response = await ExecuteCommandAsync(
            "--subscription", _knownSubscription,
            "--location", _knownLocation,
            "--include-shared-gallery");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        var result = ValidateAndDeserializeResponse(response, ComputeJsonContext.Default.VmImageListCommandResult);
        Assert.Single(result.Images);
        Assert.StartsWith("/sharedGalleries/", result.Images[0].Urn);
    }
}
