// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.WellArchitectedFramework.Services.UsageGuide;
using Xunit;

namespace Azure.Mcp.Tools.WellArchitectedFramework.UnitTests.Services.UsageGuide;

public class UsageGuideServiceTests
{
    private readonly IUsageGuideService _service;

    public UsageGuideServiceTests()
    {
        _service = new UsageGuideService();
    }

    [Fact]
    public void GetUsageGuide_ReturnsNonNullContent()
    {
        // Act
        var result = _service.GetUsageGuide();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        // Verify it contains expected markdown content (basic sanity check)
        Assert.True(result.Length > 100, "Usage guide should contain substantial content");
    }

    [Fact]
    public void GetUsageGuide_MultipleCallsReturnSameResult()
    {
        // Act
        var result1 = _service.GetUsageGuide();
        var result2 = _service.GetUsageGuide();
        var result3 = _service.GetUsageGuide();

        // Assert
        Assert.Equal(result1, result2);
        Assert.Equal(result2, result3);
    }

    [Fact]
    public void GetUsageGuide_DifferentInstances_ReturnSameResults()
    {
        // Arrange - Create multiple service instances
        var service1 = new UsageGuideService();
        var service2 = new UsageGuideService();
        var service3 = new UsageGuideService();

        // Act
        var result1 = service1.GetUsageGuide();
        var result2 = service2.GetUsageGuide();
        var result3 = service3.GetUsageGuide();

        // Assert - All instances should return the same result (using static cache)
        Assert.Equal(result1, result2);
        Assert.Equal(result2, result3);
    }

    [Fact]
    public async Task GetUsageGuide_ThreadSafety_MultipleConcurrentCalls()
    {
        // Arrange
        var tasks = new List<Task<string>>();

        // Act - Make concurrent calls to ensure thread safety of static cache initialization
        for (int i = 0; i < 50; i++)
        {
            tasks.Add(Task.Run(() => _service.GetUsageGuide()));
        }

        var results = await Task.WhenAll(tasks);

        // Assert - All results should be non-null and identical
        Assert.All(results, result => Assert.NotNull(result));
        Assert.All(results, result => Assert.NotEmpty(result));
        
        // Verify all results are the same (cache works correctly)
        var firstResult = results[0];
        Assert.All(results, result => Assert.Equal(firstResult, result));
    }

    [Fact]
    public async Task GetUsageGuide_AsyncThreadSafety_MultipleConcurrentAsyncCalls()
    {
        // Arrange
        var tasks = new List<Task<string>>();

        // Act - Make concurrent async calls
        for (int i = 0; i < 100; i++)
        {
            tasks.Add(Task.Run(() => _service.GetUsageGuide()));
        }

        var results = await Task.WhenAll(tasks);

        // Assert - All calls should return the same result
        var distinctResults = results.Distinct().ToList();
        Assert.Single(distinctResults); // All calls should return the same cached content
        Assert.NotNull(distinctResults[0]);
        Assert.NotEmpty(distinctResults[0]);
    }

    [Fact]
    public void GetUsageGuide_ReturnsConsistentContentAcrossMultipleThreads()
    {
        // Arrange
        var results = new System.Collections.Concurrent.ConcurrentBag<string>();
        var threads = new List<Thread>();

        // Act - Create multiple threads that call GetUsageGuide
        for (int i = 0; i < 10; i++)
        {
            var thread = new Thread(() =>
            {
                for (int j = 0; j < 10; j++)
                {
                    results.Add(_service.GetUsageGuide());
                }
            });
            threads.Add(thread);
            thread.Start();
        }

        // Wait for all threads to complete
        foreach (var thread in threads)
        {
            thread.Join();
        }

        // Assert - All results should be identical
        var distinctResults = results.Distinct().ToList();
        Assert.Single(distinctResults);
        Assert.NotNull(distinctResults[0]);
        Assert.NotEmpty(distinctResults[0]);
    }
}
