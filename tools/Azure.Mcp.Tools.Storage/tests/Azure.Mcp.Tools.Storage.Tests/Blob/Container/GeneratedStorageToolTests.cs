// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Storage.Commands;
using Azure.Mcp.Tools.Storage.Commands.Account;
using Azure.Mcp.Tools.Storage.Commands.Blob.Container;
using Azure.Mcp.Tools.Storage.GeneratedMcp;
using Azure.Mcp.Tools.Storage.Models;
using Azure.Mcp.Tools.Storage.Options.Blob.Container;
using Azure.Mcp.Tools.Storage.Services;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Storage.Tests.Blob.Container;

public sealed class GeneratedStorageToolTests
{
    [Fact]
    public void SelectedBuildReallyUsesTheExpectedOptionsType()
    {
        var options = typeof(ContainerCreateCommand).BaseType!.GenericTypeArguments[0];
#if STORAGE_GENERATED_MCP
        Assert.Equal(typeof(GeneratedContainerCreateOptions), options);
#else
        Assert.Equal(typeof(ContainerCreateOptions), options);
#endif
    }

    [Fact]
    public void GeneratedOptionsPreserveRequiredAndAmbientParameters()
    {
        var properties = typeof(GeneratedContainerCreateOptions).GetProperties();
        Assert.Equal(["Account", "Container", "Tenant"], properties.Select(p => p.Name).Order());
        foreach (var property in properties)
        {
            Assert.Equal(property.Name != "Tenant", property.IsDefined(typeof(RequiredMemberAttribute)));
            Assert.Equal(property.Name == "Tenant" ? NullabilityState.Nullable : NullabilityState.NotNull,
                new NullabilityInfoContext().Create(property).WriteState);
        }
    }

    [Fact]
    public void CatalogKeepsReviewedSafetyAndHumanDescription()
    {
        Assert.Equal("storage blob container create", ContainerCreateCatalog.Path);
        Assert.Equal(ContainerCreateCommand.CustomizedDescription, ContainerCreateCatalog.Description);
        Assert.NotEqual(ContainerCreateCatalog.DraftDescription, ContainerCreateCatalog.Description);
        Assert.Contains("Creates a private blob container", ContainerCreateCatalog.DraftDescription);
        Assert.True(ContainerCreateCatalog.Destructive);
        Assert.False(ContainerCreateCatalog.Idempotent);
        Assert.False(ContainerCreateCatalog.ReadOnly);
        Assert.False(ContainerCreateCatalog.OpenWorld);
        Assert.Contains("HTTP 429", ContainerCreateCatalog.GetDefaultError(429, "Retry later"));
        Assert.Contains("Retry later", ContainerCreateCatalog.GetDefaultError(429, "Retry later"));
    }

    [Fact]
    public void SetupRegistersCreateOnceAndKeepsHandwrittenComposites()
    {
        var setup = new StorageSetup();
        var services = new ServiceCollection().AddLogging();
        setup.ConfigureServices(services);
        services.AddSingleton(Substitute.For<IStorageService>());
        services.AddSingleton(Substitute.For<ISubscriptionResolver>());
        using var provider = services.BuildServiceProvider();
        var root = setup.RegisterCommands(provider);
        Assert.IsType<ContainerCreateCommand>(root.GetCommand("blob.container.create"));
        Assert.IsType<ContainerGetCommand>(root.GetCommand("blob.container.get"));
        Assert.IsType<AccountGetCommand>(root.GetCommand("account.get"));
        var container = root.SubGroup.Single(g => g.Name == "blob").SubGroup.Single(g => g.Name == "container");
        Assert.Equal(["create", "get"], container.Commands.Keys.Order());
    }

    [Fact]
    public async Task BindingCallsExistingSdkWithPrivateAccessAndExactCancellation()
    {
        var service = Substitute.For<BlobServiceClient>();
        var container = Substitute.For<BlobContainerClient>();
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        service.GetBlobContainerClient("test-container").Returns(container);
        container.CreateAsync(PublicAccessType.None, cancellationToken: token)
            .Returns(Substitute.For<Response<BlobContainerInfo>>());

        Assert.Same(container, await ContainerCreateBinding.CreateAsync(service, "test-container", token));

        service.Received(1).GetBlobContainerClient("test-container");
        await container.Received(1).CreateAsync(PublicAccessType.None, cancellationToken: token);
        await container.DidNotReceive().GetPropertiesAsync(cancellationToken: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task BindingPropagatesSdkErrorsRatherThanConvertingThemToSuccess()
    {
        var service = Substitute.For<BlobServiceClient>();
        var container = Substitute.For<BlobContainerClient>();
        service.GetBlobContainerClient("existing").Returns(container);
        var error = new RequestFailedException(409, "already exists");
        container.CreateAsync(PublicAccessType.None, cancellationToken: Arg.Any<CancellationToken>()).ThrowsAsync(error);
        Assert.Same(error, await Assert.ThrowsAsync<RequestFailedException>(() =>
            ContainerCreateBinding.CreateAsync(service, "existing", TestContext.Current.CancellationToken)));
    }

    [Fact]
    public async Task BindingPropagatesCancellation()
    {
        var service = Substitute.For<BlobServiceClient>();
        var container = Substitute.For<BlobContainerClient>();
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();
        var token = cancellation.Token;
        service.GetBlobContainerClient("cancelled").Returns(container);
        container.CreateAsync(PublicAccessType.None, cancellationToken: token)
            .Returns(Task.FromCanceled<Response<BlobContainerInfo>>(token));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            ContainerCreateBinding.CreateAsync(service, "cancelled", token));
    }

    [Fact]
    public void GeneratedSerializationMatchesHandwrittenOutputExactly()
    {
        var info = new ContainerInfo("example", DateTimeOffset.Parse("2026-01-01T00:00:00Z"), "etag",
            new Dictionary<string, string> { ["purpose"] = "pilot" }, "unlocked", "available", null,
            "private", false, false, null, null, false);
        var expected = JsonSerializer.Serialize(new ContainerCreateCommand.ContainerCreateCommandResult(info),
            StorageJsonContext.Default.ContainerCreateCommandResult);
        var generated = JsonSerializer.Serialize(new GeneratedContainerCreateResult { Container = info },
            GeneratedStorageJsonContext.Default.GeneratedContainerCreateResult);
        Assert.Equal(expected, generated);
    }
}
