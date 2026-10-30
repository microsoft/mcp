// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Tests.Helpers;
using Azure.Mcp.Tools.ManagedLustre.Services;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.StorageCache;
using Azure.ResourceManager.StorageCache.Mocking;
using Azure.ResourceManager.StorageCache.Models;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.ManagedLustre.Tests.Services;

public class ManagedLustreServiceTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CreateExpansionJobAsync_StartsExpansionJobAndRetriesCapacityReservations(bool retry)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var (service, jobs) = CreateService(cancellationToken);
        var job = Substitute.For<AmlFileSystemExpansionJobResource>();
        job.Data.Returns(ArmStorageCacheModelFactory.AmlFileSystemExpansionJobData(name: "expansion-job"));
        var operation = Substitute.For<ArmOperation<AmlFileSystemExpansionJobResource>>();
        operation.Value.Returns(job);
        operation.HasCompleted.Returns(true);
        int attempts = 0;
        jobs.CreateOrUpdateAsync(
            WaitUntil.Started, "expansion-job", Arg.Any<AmlFileSystemExpansionJobData>(), cancellationToken)
            .Returns(_ =>
            {
                if (retry && attempts++ == 0)
                {
                    throw new RequestFailedException(400, "The filesystem is finalizing capacity reservations.");
                }

                return operation;
            });

        var result = await service.CreateExpansionJobAsync(
            "subscription", "resource-group", "filesystem", 16, "expansion-job", cancellationToken: cancellationToken);

        Assert.Equal("expansion-job", result);
        await jobs.Received(retry ? 2 : 1).CreateOrUpdateAsync(
            WaitUntil.Started,
            "expansion-job",
            Arg.Is<AmlFileSystemExpansionJobData>(data => data.NewStorageCapacityTiB == 16 && data.Location == AzureLocation.WestUS),
            cancellationToken);
    }

    [Theory]
    [InlineData(400, "Invalid expansion size.")]
    [InlineData(403, "Access denied.")]
    [InlineData(429, "Too many requests.")]
    public async Task CreateExpansionJobAsync_OtherFailures_PreservesExceptionWithoutRetry(int status, string message)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var (service, jobs) = CreateService(cancellationToken);
        var expected = new RequestFailedException(status, message);
        jobs.CreateOrUpdateAsync(
            Arg.Any<WaitUntil>(), Arg.Any<string>(), Arg.Any<AmlFileSystemExpansionJobData>(), cancellationToken)
            .Returns(Task.FromException<ArmOperation<AmlFileSystemExpansionJobResource>>(expected));

        var actual = await Assert.ThrowsAsync<RequestFailedException>(() =>
            service.CreateExpansionJobAsync(
                "subscription", "resource-group", "filesystem", 16, "expansion-job", cancellationToken: cancellationToken));

        Assert.Same(expected, actual);
        await jobs.Received(1).CreateOrUpdateAsync(
            WaitUntil.Started, "expansion-job", Arg.Any<AmlFileSystemExpansionJobData>(), cancellationToken);
    }

    [Fact]
    public async Task GetExpansionJobAsync_MissingResourceGroup_ThrowsKeyNotFoundException()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var azureService = AzureServiceTestHelpers.CreateAzureService();
        azureService.GetResourceGroupResource("subscription", "resource-group", null, cancellationToken)
            .Returns((ResourceGroupResource?)null);
        var service = new ManagedLustreService(azureService, NullLogger<ManagedLustreService>.Instance);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.GetExpansionJobAsync(
                "subscription", "resource-group", "filesystem", "expansion-job", cancellationToken: cancellationToken));

        Assert.Contains("resource-group", exception.Message);
    }

    private static (ManagedLustreService Service, AmlFileSystemExpansionJobCollection Jobs) CreateService(
        CancellationToken cancellationToken)
    {
        var azureService = AzureServiceTestHelpers.CreateAzureService();
        var resourceGroup = Substitute.For<ResourceGroupResource>();
        var storageCache = Substitute.For<MockableStorageCacheResourceGroupResource>();
        var filesystem = Substitute.For<AmlFileSystemResource>();
        var jobs = Substitute.For<AmlFileSystemExpansionJobCollection>();
        azureService.GetResourceGroupResource("subscription", "resource-group", null, cancellationToken)
            .Returns(resourceGroup);
        resourceGroup.GetCachedClient(Arg.Any<Func<ArmClient, MockableStorageCacheResourceGroupResource>>())
            .Returns(storageCache);
        storageCache.GetAmlFileSystemAsync("filesystem", cancellationToken)
            .Returns(Response.FromValue(filesystem, Substitute.For<Response>()));
        filesystem.Data.Returns(new AmlFileSystemData(AzureLocation.WestUS));
        filesystem.GetAmlFileSystemExpansionJobs().Returns(jobs);

        return (new ManagedLustreService(azureService, NullLogger<ManagedLustreService>.Instance), jobs);
    }
}
