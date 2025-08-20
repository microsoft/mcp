// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Storage.Commands.Blob;
using Azure.Mcp.Tools.Storage.Services;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Storage.UnitTests.Blob;

public class BlobDetailsCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IStorageService _storageService;
    private readonly ILogger<BlobDetailsCommand> _logger;
    private readonly BlobDetailsCommand _command;
    private readonly CommandContext _context;
    private readonly Parser _parser;
    private readonly string _knownAccountName = "account123";
    private readonly string _knownContainerName = "container123";
    private readonly string _knownBlobName = "test-blob.txt";
    private readonly string _knownSubscriptionId = "sub123";

    public BlobDetailsCommandTests()
    {
        _storageService = Substitute.For<IStorageService>();
        _logger = Substitute.For<ILogger<BlobDetailsCommand>>();

        var collection = new ServiceCollection().AddSingleton(_storageService);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _parser = new(_command.GetCommand());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsBlobDetails()
    {
        // Arrange
        // Use reflection to create an instance of BlobProperties since it has no public constructor
        var expectedProperties = (BlobProperties)Activator.CreateInstance(
            typeof(BlobProperties),
            nonPublic: true
        )!;

        // Set basic properties using reflection
        SetProperty(expectedProperties, "LastModified", DateTimeOffset.UtcNow);
        SetProperty(expectedProperties, "ContentLength", 1024L);
        SetProperty(expectedProperties, "ContentType", "text/plain");
        SetProperty(expectedProperties, "BlobType", BlobType.Block);

        _storageService.GetBlobDetails(Arg.Is(_knownAccountName), Arg.Is(_knownContainerName),
            Arg.Is(_knownBlobName), Arg.Is(_knownSubscriptionId), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns(expectedProperties);

        var args = _parser.Parse([
            "--account", _knownAccountName,
            "--container", _knownContainerName,
            "--blob", _knownBlobName,
            "--subscription", _knownSubscriptionId
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(200, response.Status);
    }

    private static void SetProperty(object obj, string propertyName, object value)
    {
        var property = typeof(BlobProperties).GetProperty(propertyName,
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.NonPublic);

        if (property?.CanWrite == true)
        {
            property.SetValue(obj, value);
        }
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        var expectedError = "Blob not found";

        _storageService.GetBlobDetails(Arg.Is(_knownAccountName), Arg.Is(_knownContainerName),
            Arg.Is(_knownBlobName), Arg.Is(_knownSubscriptionId), null, Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(new Exception(expectedError));

        var args = _parser.Parse([
            "--account", _knownAccountName,
            "--container", _knownContainerName,
            "--blob", _knownBlobName,
            "--subscription", _knownSubscriptionId
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(500, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ValidatesRequiredParameters()
    {
        // Arrange - missing blob parameter
        var args = _parser.Parse([
            "--account", _knownAccountName,
            "--container", _knownContainerName,
            "--subscription", _knownSubscriptionId
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(400, response.Status);
        Assert.Contains("required", response.Message.ToLower());
    }

    private class BlobDetailsResult
    {
        [JsonPropertyName("details")]
        public BlobDetailsInfo Details { get; set; } = new();
    }

    private class BlobDetailsInfo
    {
        [JsonPropertyName("contentLength")]
        public long ContentLength { get; set; }

        [JsonPropertyName("contentType")]
        public string ContentType { get; set; } = string.Empty;

        [JsonPropertyName("blobType")]
        public BlobType BlobType { get; set; }

        [JsonPropertyName("lastModified")]
        public DateTimeOffset LastModified { get; set; }
    }
}
