// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.Compute.Commands;
using Azure.Mcp.Tools.Compute.Commands.Gallery;
using Azure.Mcp.Tools.Compute.Models;
using Azure.Mcp.Tools.Compute.Services;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Compute.Tests.Gallery;

/// <summary>
/// Unit tests for the GalleryCreateCommand.
/// </summary>
public class GalleryCreateCommandTests : SubscriptionCommandUnitTestsBase<GalleryCreateCommand, IComputeService>
{
    private const string Subscription = "test-sub";
    private const string ResourceGroup = "testrg";
    private const string GalleryName = "myGallery";

    private static GalleryInfo CreateMockGallery(
        string name = GalleryName,
        string location = "eastus",
        string? description = null,
        Dictionary<string, string>? tags = null) => new()
        {
            Name = name,
            Id = $"/subscriptions/{Subscription}/resourceGroups/{ResourceGroup}/providers/Microsoft.Compute/galleries/{name}",
            ResourceGroup = ResourceGroup,
            Location = location,
            Description = description,
            UniqueName = $"{Subscription}-{name}",
            ProvisioningState = "Succeeded",
            SharingPermissions = "Private",
            Tags = tags
        };

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.NotNull(Command);
        Assert.Equal("create", Command.Name);
        Assert.NotEqual(Guid.Empty.ToString(), Command.Id);
        Assert.NotNull(Command.Description);
        Assert.NotEmpty(Command.Description);
    }

    [Fact]
    public void Metadata_HasCorrectProperties()
    {
        var metadata = Command.Metadata;

        Assert.True(metadata.Destructive);
        Assert.False(metadata.Idempotent);
        Assert.False(metadata.OpenWorld);
        Assert.False(metadata.ReadOnly);
        Assert.False(metadata.Secret);
        Assert.False(metadata.LocalRequired);
    }

    [Fact]
    public async Task ExecuteAsync_CreateGallery_ReturnsSuccess()
    {
        // Arrange
        Service.CreateGalleryAsync(
            GalleryName,
            ResourceGroup,
            Subscription,
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<IReadOnlyDictionary<string, string>?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(CreateMockGallery());

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", Subscription,
            "--resource-group", ResourceGroup,
            "--gallery", GalleryName);

        // Assert
        var result = ValidateAndDeserializeResponse(response, ComputeJsonContext.Default.GalleryCreateCommandResult);

        Assert.NotNull(result.Gallery);
        Assert.Equal(GalleryName, result.Gallery.Name);
        Assert.Equal(ResourceGroup, result.Gallery.ResourceGroup);
        Assert.Equal("eastus", result.Gallery.Location);
        Assert.Equal("Succeeded", result.Gallery.ProvisioningState);
    }

    [Fact]
    public async Task ExecuteAsync_WithAllOptions_PassesValuesToService()
    {
        // Arrange
        const string location = "westus2";
        const string description = "Gallery for VM applications";

        Service.CreateGalleryAsync(
            GalleryName,
            ResourceGroup,
            Subscription,
            location,
            description,
            Arg.Any<IReadOnlyDictionary<string, string>?>(),
            "test-tenant",
            Arg.Any<CancellationToken>())
            .Returns(CreateMockGallery(location: location, description: description));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", Subscription,
            "--resource-group", ResourceGroup,
            "--gallery", GalleryName,
            "--location", location,
            "--description", description,
            "--tenant", "test-tenant");

        // Assert
        var result = ValidateAndDeserializeResponse(response, ComputeJsonContext.Default.GalleryCreateCommandResult);

        Assert.Equal(location, result.Gallery.Location);
        Assert.Equal(description, result.Gallery.Description);

        await Service.Received(1).CreateGalleryAsync(
            GalleryName,
            ResourceGroup,
            Subscription,
            location,
            description,
            Arg.Any<IReadOnlyDictionary<string, string>?>(),
            "test-tenant",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithoutLocation_PassesNullLocationToService()
    {
        // Arrange
        Service.CreateGalleryAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<IReadOnlyDictionary<string, string>?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(CreateMockGallery());

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", Subscription,
            "--resource-group", ResourceGroup,
            "--gallery", GalleryName);

        // Assert - service resolves the default location from the resource group
        ValidateAndDeserializeResponse(response, ComputeJsonContext.Default.GalleryCreateCommandResult);

        await Service.Received(1).CreateGalleryAsync(
            GalleryName,
            ResourceGroup,
            Subscription,
            null,
            null,
            Arg.Any<IReadOnlyDictionary<string, string>?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ParsesMultipleTags()
    {
        // Arrange
        IReadOnlyDictionary<string, string>? capturedTags = null;

        Service.CreateGalleryAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Do<IReadOnlyDictionary<string, string>?>(t => capturedTags = t),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(CreateMockGallery());

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", Subscription,
            "--resource-group", ResourceGroup,
            "--gallery", GalleryName,
            "--tags", "env=prod,team=compute");

        // Assert
        ValidateAndDeserializeResponse(response, ComputeJsonContext.Default.GalleryCreateCommandResult);

        Assert.NotNull(capturedTags);
        Assert.Equal(2, capturedTags.Count);
        Assert.Equal("prod", capturedTags["env"]);
        Assert.Equal("compute", capturedTags["team"]);
    }

    [Fact]
    public async Task ExecuteAsync_TrimsWhitespaceInTags()
    {
        // Arrange
        IReadOnlyDictionary<string, string>? capturedTags = null;

        Service.CreateGalleryAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Do<IReadOnlyDictionary<string, string>?>(t => capturedTags = t),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(CreateMockGallery());

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", Subscription,
            "--resource-group", ResourceGroup,
            "--gallery", GalleryName,
            "--tags", "  env = prod ,  team = compute  ");

        // Assert
        ValidateAndDeserializeResponse(response, ComputeJsonContext.Default.GalleryCreateCommandResult);

        Assert.NotNull(capturedTags);
        Assert.Equal("prod", capturedTags["env"]);
        Assert.Equal("compute", capturedTags["team"]);
    }

    [Fact]
    public async Task ExecuteAsync_WithoutTags_PassesNullTagsToService()
    {
        // Arrange
        Service.CreateGalleryAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<IReadOnlyDictionary<string, string>?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(CreateMockGallery());

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", Subscription,
            "--resource-group", ResourceGroup,
            "--gallery", GalleryName);

        // Assert
        ValidateAndDeserializeResponse(response, ComputeJsonContext.Default.GalleryCreateCommandResult);

        await Service.Received(1).CreateGalleryAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            null,
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_MissingGallery_ReturnsBadRequest()
    {
        var response = await ExecuteCommandAsync(
            "--subscription", Subscription,
            "--resource-group", ResourceGroup);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_MissingResourceGroup_ReturnsBadRequest()
    {
        var response = await ExecuteCommandAsync(
            "--subscription", Subscription,
            "--gallery", GalleryName);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_MissingSubscription_ReturnsBadRequest()
    {
        var response = await ExecuteCommandAsync(
            "--resource-group", ResourceGroup,
            "--gallery", GalleryName);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
    }

    [Theory]
    [InlineData("-leading-hyphen")]
    [InlineData(".leadingperiod")]
    [InlineData("_leadingunderscore")]
    [InlineData("my-gallery")]
    [InlineData("trailinghyphen-")]
    [InlineData("trailingperiod.")]
    [InlineData("trailingunderscore_")]
    [InlineData("has space")]
    [InlineData("has/slash")]
    [InlineData("has$dollar")]
    public async Task ExecuteAsync_InvalidGalleryName_ReturnsBadRequest(string galleryName)
    {
        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", Subscription,
            "--resource-group", ResourceGroup,
            "--gallery", galleryName);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);

        await Service.DidNotReceive().CreateGalleryAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<IReadOnlyDictionary<string, string>?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_GalleryNameExceeds80Characters_ReturnsBadRequest()
    {
        // Arrange
        var longName = new string('a', 81);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", Subscription,
            "--resource-group", ResourceGroup,
            "--gallery", longName);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_GalleryNameAtMaxLength_ReturnsSuccess()
    {
        // Arrange
        var maxLengthName = new string('a', 80);

        Service.CreateGalleryAsync(
            maxLengthName,
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<IReadOnlyDictionary<string, string>?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(CreateMockGallery(name: maxLengthName));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", Subscription,
            "--resource-group", ResourceGroup,
            "--gallery", maxLengthName);

        // Assert
        var result = ValidateAndDeserializeResponse(response, ComputeJsonContext.Default.GalleryCreateCommandResult);
        Assert.Equal(maxLengthName, result.Gallery.Name);
    }

    [Theory]
    [InlineData("envprod")]
    [InlineData("env=prod,teamcompute")]
    [InlineData("=prod")]
    [InlineData("env=")]
    [InlineData(",")]
    [InlineData("env=prod,")]
    [InlineData(",env=prod")]
    [InlineData("env=prod,,team=compute")]
    [InlineData("env=prod, ,team=compute")]
    public async Task ExecuteAsync_MalformedTags_ReturnsBadRequestAndSkipsService(string tags)
    {
        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", Subscription,
            "--resource-group", ResourceGroup,
            "--gallery", GalleryName,
            "--tags", tags);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);

        await Service.DidNotReceive().CreateGalleryAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<IReadOnlyDictionary<string, string>?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ServiceThrowsGenericException_ReturnsError()
    {
        // Arrange
        Service.CreateGalleryAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<IReadOnlyDictionary<string, string>?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Service unavailable"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", Subscription,
            "--resource-group", ResourceGroup,
            "--gallery", GalleryName);

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ResourceGroupNotFound_ReturnsNotFound()
    {
        // Arrange
        Service.CreateGalleryAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<IReadOnlyDictionary<string, string>?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(404, "Resource group not found"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", Subscription,
            "--resource-group", "missing-rg",
            "--gallery", GalleryName);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("Resource group not found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ConflictingOperation_ReturnsConflict()
    {
        // Arrange
        Service.CreateGalleryAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<IReadOnlyDictionary<string, string>?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(409, "Conflict"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", Subscription,
            "--resource-group", ResourceGroup,
            "--gallery", GalleryName);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.Status);
        Assert.Contains("Conflict creating the gallery", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_LocationMismatch_ExplainsThatLocationIsImmutable()
    {
        // Arrange - ARM rejects a PUT that would move an existing gallery to another region.
        Service.CreateGalleryAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<IReadOnlyDictionary<string, string>?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(
                409,
                "The resource already exists in location 'eastus2'.",
                "InvalidResourceLocation",
                null));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", Subscription,
            "--resource-group", ResourceGroup,
            "--gallery", GalleryName,
            "--location", "westus");

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.Status);
        Assert.Contains("location cannot be changed", response.Message);
        Assert.DoesNotContain("Another operation may be in progress", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_AuthorizationFailure_ReturnsForbidden()
    {
        // Arrange
        Service.CreateGalleryAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<IReadOnlyDictionary<string, string>?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(403, "Forbidden"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", Subscription,
            "--resource-group", ResourceGroup,
            "--gallery", GalleryName);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Authorization failed", response.Message);
    }

    [Theory]
    [InlineData("a", true)]
    [InlineData("1", true)]
    [InlineData("myGallery", true)]
    [InlineData("my.gallery_name1", true)]
    [InlineData("", false)]
    [InlineData("-abc", false)]
    [InlineData("abc-", false)]
    [InlineData("abc.", false)]
    [InlineData("abc_", false)]
    // Galleries reject hyphens even though sibling Compute resources allow them.
    [InlineData("my-gallery", false)]
    public void IsValidGalleryName_ValidatesCorrectly(string galleryName, bool expected)
    {
        Assert.Equal(expected, GalleryCreateCommand.IsValidGalleryName(galleryName));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ParseTags_EmptyInput_ReturnsNull(string? tags)
    {
        Assert.Null(GalleryCreateCommand.ParseTags(tags));
    }

    [Fact]
    public void ParseTags_DuplicateKeys_UsesLastValue()
    {
        var parsed = GalleryCreateCommand.ParseTags("env=dev,env=prod");

        Assert.NotNull(parsed);
        Assert.Single(parsed);
        Assert.Equal("prod", parsed["env"]);
    }

    [Fact]
    public void ParseTags_ValueContainingEquals_KeepsFullValue()
    {
        var parsed = GalleryCreateCommand.ParseTags("connection=key=value");

        Assert.NotNull(parsed);
        Assert.Equal("key=value", parsed["connection"]);
    }
}
