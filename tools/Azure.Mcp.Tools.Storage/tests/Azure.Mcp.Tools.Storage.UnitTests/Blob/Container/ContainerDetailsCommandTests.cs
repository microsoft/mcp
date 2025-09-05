// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Storage.Commands.Blob.Container;
using Azure.Mcp.Tools.Storage.Services;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Storage.UnitTests.Blob.Container;

public class ContainerDetailsCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IStorageService _storageService;
    private readonly ILogger<ContainerDetailsCommand> _logger;
    private readonly ContainerDetailsCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;
    private readonly string _knownAccount = "account123";
    private readonly string _knownContainer = "container123";
    private readonly string _knownSubscription = "sub123";

    public ContainerDetailsCommandTests()
    {
        _storageService = Substitute.For<IStorageService>();
        _logger = Substitute.For<ILogger<ContainerDetailsCommand>>();

        var collection = new ServiceCollection().AddSingleton(_storageService);
        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsContainerDetails()
    {
        // Arrange
        // Use reflection to create an instance of BlobContainerProperties since it has no public constructor
        var expectedProperties = (BlobContainerProperties)Activator.CreateInstance(
            typeof(BlobContainerProperties),
            nonPublic: true
        )!;

        // Set properties using reflection
        typeof(BlobContainerProperties).GetProperty("LastModified", System.Reflection.BindingFlags.Instance
            | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)
            ?.SetValue(expectedProperties, DateTimeOffset.UtcNow);
        typeof(BlobContainerProperties).GetProperty("LeaseStatus", System.Reflection.BindingFlags.Instance
            | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)
            ?.SetValue(expectedProperties, LeaseStatus.Locked);
        typeof(BlobContainerProperties).GetProperty("LeaseState", System.Reflection.BindingFlags.Instance
            | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)
            ?.SetValue(expectedProperties, LeaseState.Leased);
        typeof(BlobContainerProperties).GetProperty("LeaseDuration", System.Reflection.BindingFlags.Instance
            | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)
            ?.SetValue(expectedProperties, LeaseDurationType.Infinite);
        typeof(BlobContainerProperties).GetProperty("PublicAccess", System.Reflection.BindingFlags.Instance
            | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)
            ?.SetValue(expectedProperties, PublicAccessType.Blob);
        typeof(BlobContainerProperties).GetProperty("HasImmutabilityPolicy", System.Reflection.BindingFlags.Instance
            | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)
            ?.SetValue(expectedProperties, true);
        typeof(BlobContainerProperties).GetProperty("HasLegalHold", System.Reflection.BindingFlags.Instance
            | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)
            ?.SetValue(expectedProperties, false);
        typeof(BlobContainerProperties).GetProperty("DefaultEncryptionScope", System.Reflection.BindingFlags.Instance
            | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)
            ?.SetValue(expectedProperties, "scope1");
        typeof(BlobContainerProperties).GetProperty("PreventEncryptionScopeOverride",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public
            | System.Reflection.BindingFlags.NonPublic)?.SetValue(expectedProperties, true);
        typeof(BlobContainerProperties).GetProperty("DeletedOn", System.Reflection.BindingFlags.Instance
            | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)
            ?.SetValue(expectedProperties, DateTimeOffset.UtcNow.AddDays(-1));
        typeof(BlobContainerProperties).GetProperty("RemainingRetentionDays", System.Reflection.BindingFlags.Instance
            | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)
            ?.SetValue(expectedProperties, 5);
        typeof(BlobContainerProperties).GetProperty("Metadata", System.Reflection.BindingFlags.Instance
            | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)
            ?.SetValue(expectedProperties, new Dictionary<string, string> { { "k", "v" } });
        typeof(BlobContainerProperties).GetProperty("HasImmutableStorageWithVersioning",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public
            | System.Reflection.BindingFlags.NonPublic)?.SetValue(expectedProperties, true);

        _storageService.GetContainerDetails(Arg.Is(_knownAccount), Arg.Is(_knownContainer),
            Arg.Is(_knownSubscription), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns(expectedProperties);

        var args = _commandDefinition.Parse([
            "--account", _knownAccount,
            "--container", _knownContainer,
            "--subscription", _knownSubscription
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<ContainerDetailsResult>(json);

        Assert.NotNull(result);
        Assert.NotNull(result.Details);
        Assert.Equal(expectedProperties.LastModified, result.Details.LastModified);
        Assert.Equal(expectedProperties.LeaseStatus, result.Details.LeaseStatus);
        Assert.Equal(expectedProperties.LeaseState, result.Details.LeaseState);
        Assert.Equal(expectedProperties.LeaseDuration, result.Details.LeaseDuration);
        Assert.Equal(expectedProperties.PublicAccess, result.Details.PublicAccess);
        Assert.Equal(expectedProperties.HasImmutabilityPolicy, result.Details.HasImmutabilityPolicy);
        Assert.Equal(expectedProperties.HasLegalHold, result.Details.HasLegalHold);
        Assert.Equal(expectedProperties.DefaultEncryptionScope, result.Details.DefaultEncryptionScope);
        Assert.Equal(expectedProperties.PreventEncryptionScopeOverride, result.Details.PreventEncryptionScopeOverride);
        Assert.Equal(expectedProperties.DeletedOn, result.Details.DeletedOn);
        Assert.Equal(expectedProperties.RemainingRetentionDays, result.Details.RemainingRetentionDays);
        Assert.Equal(expectedProperties.Metadata, result.Details.Metadata);
        Assert.Equal(expectedProperties.HasImmutableStorageWithVersioning,
            result.Details.HasImmutableStorageWithVersioning);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        var expectedError = "Test error";
        _storageService.GetContainerDetails(Arg.Is(_knownAccount), Arg.Is(_knownContainer),
            Arg.Is(_knownSubscription), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(new Exception(expectedError));

        var args = _commandDefinition.Parse([
            "--account", _knownAccount,
            "--container", _knownContainer,
            "--subscription", _knownSubscription
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(500, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }

    private class ContainerDetailsResult
    {
        [JsonPropertyName("details")]
        public JsonBlobContainerProperties Details { get; set; } = null!;
    }

    private class JsonBlobContainerProperties
    {
        [JsonPropertyName("lastModified")]
        public DateTimeOffset LastModified { get; set; }
        [JsonPropertyName("leaseStatus")]
        public LeaseStatus? LeaseStatus { get; set; }
        [JsonPropertyName("leaseState")]
        public LeaseState? LeaseState { get; set; }
        [JsonPropertyName("leaseDuration")]
        public LeaseDurationType? LeaseDuration { get; set; }
        [JsonPropertyName("publicAccess")]
        public PublicAccessType? PublicAccess { get; set; }
        [JsonPropertyName("hasImmutabilityPolicy")]
        public bool? HasImmutabilityPolicy { get; set; }
        [JsonPropertyName("hasLegalHold")]
        public bool? HasLegalHold { get; set; }
        [JsonPropertyName("defaultEncryptionScope")]
        public string DefaultEncryptionScope { get; set; } = null!;
        [JsonPropertyName("preventEncryptionScopeOverride")]
        public bool? PreventEncryptionScopeOverride { get; set; }
        [JsonPropertyName("deletedOn")]
        public DateTimeOffset? DeletedOn { get; set; }
        [JsonPropertyName("remainingRetentionDays")]
        public int? RemainingRetentionDays { get; set; }
        [JsonPropertyName("metadata")]
        public IDictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
        [JsonPropertyName("hasImmutableStorageWithVersioning")]
        public bool HasImmutableStorageWithVersioning { get; set; }
    }
}
