// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Caching;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Services.Caching;

public class CacheServiceTests
{
    private readonly ICacheService _cacheService;
    private readonly IMemoryCache _memoryCache;

    public CacheServiceTests()
    {
        _memoryCache = new MemoryCache(Microsoft.Extensions.Options.Options.Create(new MemoryCacheOptions()));
        _cacheService = new SingleUserCliCacheService(_memoryCache);
    }

    [Fact]
    public async Task SetAndGet_WithoutGroup_ShouldWorkAsExpected()
    {
        // Arrange
        string group = "test-group";
        string key = "test-key";
        string value = "test-value";

        // Clear any existing cache data
        await _cacheService.ClearAsync(TestContext.Current.CancellationToken);

        // Act
        await _cacheService.SetAsync(group, key, value, cancellationToken: TestContext.Current.CancellationToken);
        var result = await _cacheService.GetAsync<string>(group, key, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public async Task SetAndGet_WithGroup_ShouldWorkAsExpected()
    {
        // Arrange
        string group = "test-group";
        string key = "test-key";
        string value = "test-value";

        // Clear any existing cache data
        await _cacheService.ClearAsync(TestContext.Current.CancellationToken);

        // Act
        await _cacheService.SetAsync(group, key, value, cancellationToken: TestContext.Current.CancellationToken);
        var result = await _cacheService.GetAsync<string>(group, key, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public async Task GetGroupKeysAsync_ShouldReturnKeysInGroup()
    {
        // Arrange
        string group = "test-group";
        string key1 = "test-key1";
        string key2 = "test-key2";
        string value1 = "test-value1";
        string value2 = "test-value2";

        // Clear any existing cache data
        await _cacheService.ClearAsync(TestContext.Current.CancellationToken);

        // Act
        await _cacheService.SetAsync(group, key1, value1, cancellationToken: TestContext.Current.CancellationToken);
        await _cacheService.SetAsync(group, key2, value2, cancellationToken: TestContext.Current.CancellationToken);
        var groupKeys = await _cacheService.GetGroupKeysAsync(group, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, groupKeys.Count());
        Assert.Contains(key1, groupKeys);
        Assert.Contains(key2, groupKeys);
    }

    [Fact]
    public async Task DeleteAsync_WithGroup_ShouldRemoveKeyFromGroup()
    {
        // Arrange
        string group = "test-group";
        string key1 = "test-key1";
        string key2 = "test-key2";
        string value1 = "test-value1";
        string value2 = "test-value2";

        // Clear any existing cache data
        await _cacheService.ClearAsync(TestContext.Current.CancellationToken);

        // Act
        await _cacheService.SetAsync(group, key1, value1, cancellationToken: TestContext.Current.CancellationToken);
        await _cacheService.SetAsync(group, key2, value2, cancellationToken: TestContext.Current.CancellationToken);
        await _cacheService.DeleteAsync(group, key1, TestContext.Current.CancellationToken);

        var groupKeys = await _cacheService.GetGroupKeysAsync(group, TestContext.Current.CancellationToken);
        var result1 = await _cacheService.GetAsync<string>(group, key1, cancellationToken: TestContext.Current.CancellationToken);
        var result2 = await _cacheService.GetAsync<string>(group, key2, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Single(groupKeys);
        Assert.Contains(key2, groupKeys);
        Assert.Null(result1);
        Assert.Equal(value2, result2);
    }
    [Fact]
    public async Task ClearAsync_ShouldRemoveAllCachedItems()
    {
        // Arrange
        string group1 = "test-group1";
        string group2 = "test-group2";
        string key1 = "test-key1";
        string key2 = "test-key2";
        string value1 = "test-value1";
        string value2 = "test-value2";

        // Clear any existing cache data first
        await _cacheService.ClearAsync(TestContext.Current.CancellationToken);

        await _cacheService.SetAsync(group1, key1, value1, cancellationToken: TestContext.Current.CancellationToken);
        await _cacheService.SetAsync(group2, key2, value2, cancellationToken: TestContext.Current.CancellationToken);

        // Act
        await _cacheService.ClearAsync(TestContext.Current.CancellationToken);

        // Assert
        var group1Keys = await _cacheService.GetGroupKeysAsync(group1, TestContext.Current.CancellationToken);
        var group2Keys = await _cacheService.GetGroupKeysAsync(group2, TestContext.Current.CancellationToken);
        var result1 = await _cacheService.GetAsync<string>(group1, key1, cancellationToken: TestContext.Current.CancellationToken);
        var result2 = await _cacheService.GetAsync<string>(group2, key2, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Empty(group1Keys);
        Assert.Empty(group2Keys);
        Assert.Null(result1);
        Assert.Null(result2);
    }

    [Fact]
    public async Task ClearGroupAsync_ShouldRemoveOnlySpecificGroup()
    {
        // Arrange
        string group1 = "test-group1";
        string group2 = "test-group2";
        string key1 = "test-key1";
        string key2 = "test-key2";
        string value1 = "test-value1";
        string value2 = "test-value2";

        // Clear any existing cache data first
        await _cacheService.ClearAsync(TestContext.Current.CancellationToken);

        await _cacheService.SetAsync(group1, key1, value1, cancellationToken: TestContext.Current.CancellationToken);
        await _cacheService.SetAsync(group2, key2, value2, cancellationToken: TestContext.Current.CancellationToken);

        // Act
        await _cacheService.ClearGroupAsync(group1, TestContext.Current.CancellationToken);

        // Assert
        var group1Keys = await _cacheService.GetGroupKeysAsync(group1, TestContext.Current.CancellationToken);
        var group2Keys = await _cacheService.GetGroupKeysAsync(group2, TestContext.Current.CancellationToken);
        var result1 = await _cacheService.GetAsync<string>(group1, key1, cancellationToken: TestContext.Current.CancellationToken);
        var result2 = await _cacheService.GetAsync<string>(group2, key2, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Empty(group1Keys);
        Assert.Single(group2Keys);
        Assert.Null(result1);
        Assert.Equal(value2, result2);
        Assert.Equal(value2, result2);
    }
}
