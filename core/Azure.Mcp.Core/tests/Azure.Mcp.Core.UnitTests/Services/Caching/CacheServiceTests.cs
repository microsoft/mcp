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
        _cacheService = new CacheService(_memoryCache);
    }

    [Fact]
    public async Task SetAndGet_WithoutGroup_ShouldWorkAsExpected()
    {
        // Arrange
        string group = "test-group";
        string key = "test-key";
        string value = "test-value";

        // Clear any existing cache data
        await _cacheService.ClearAsync();

        // Act
        await _cacheService.SetAsync(group, key, value);
        var result = await _cacheService.GetAsync<string>(group, key);

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
        await _cacheService.ClearAsync();

        // Act
        await _cacheService.SetAsync(group, key, value);
        var result = await _cacheService.GetAsync<string>(group, key);

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
        await _cacheService.ClearAsync();

        // Act
        await _cacheService.SetAsync(group, key1, value1);
        await _cacheService.SetAsync(group, key2, value2);
        var groupKeys = await _cacheService.GetGroupKeysAsync(group);

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
        await _cacheService.ClearAsync();

        // Act
        await _cacheService.SetAsync(group, key1, value1);
        await _cacheService.SetAsync(group, key2, value2);
        await _cacheService.DeleteAsync(group, key1);

        var groupKeys = await _cacheService.GetGroupKeysAsync(group);
        var result1 = await _cacheService.GetAsync<string>(group, key1);
        var result2 = await _cacheService.GetAsync<string>(group, key2);        // Assert
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
        await _cacheService.ClearAsync();

        await _cacheService.SetAsync(group1, key1, value1);
        await _cacheService.SetAsync(group2, key2, value2);

        // Act
        await _cacheService.ClearAsync();

        // Assert
        var group1Keys = await _cacheService.GetGroupKeysAsync(group1);
        var group2Keys = await _cacheService.GetGroupKeysAsync(group2);
        var result1 = await _cacheService.GetAsync<string>(group1, key1);
        var result2 = await _cacheService.GetAsync<string>(group2, key2);

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
        await _cacheService.ClearAsync();

        await _cacheService.SetAsync(group1, key1, value1);
        await _cacheService.SetAsync(group2, key2, value2);

        // Act
        await _cacheService.ClearGroupAsync(group1);

        // Assert
        var group1Keys = await _cacheService.GetGroupKeysAsync(group1);
        var group2Keys = await _cacheService.GetGroupKeysAsync(group2);
        var result1 = await _cacheService.GetAsync<string>(group1, key1);
        var result2 = await _cacheService.GetAsync<string>(group2, key2);

        Assert.Empty(group1Keys);
        Assert.Single(group2Keys);
        Assert.Null(result1);
        Assert.Equal(value2, result2);
        Assert.Equal(value2, result2);
    }
}
