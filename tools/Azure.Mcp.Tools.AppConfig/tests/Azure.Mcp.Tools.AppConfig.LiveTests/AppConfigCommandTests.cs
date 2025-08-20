// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Core.Services.Caching;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Azure.Mcp.Tests.Client.Helpers;
using Azure.Mcp.Tools.AppConfig.Services;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace Azure.Mcp.Tools.AppConfig.LiveTests;

public class AppConfigCommandTests : CommandTestsBase,
    IClassFixture<LiveTestFixture>
{
    private const string AccountsKey = "accounts";
    private const string SettingsKey = "settings";
    private readonly AppConfigService _appConfigService;
    private readonly string _subscriptionId;
    private readonly string _accountName;

    public AppConfigCommandTests(LiveTestFixture liveTestFixture, ITestOutputHelper output) : base(liveTestFixture, output)
    {
        var memoryCache = new MemoryCache(Microsoft.Extensions.Options.Options.Create(new MemoryCacheOptions()));
        var cacheService = new CacheService(memoryCache);
        var tenantService = new TenantService(cacheService);
        var subscriptionService = new SubscriptionService(cacheService, tenantService);
        _appConfigService = new AppConfigService(subscriptionService, tenantService);
        _subscriptionId = Settings.SubscriptionId;
        _accountName = Settings.ResourceBaseName;
    }

    [Fact]
    public async Task Should_list_appconfig_accounts()
    {
        // act
        var result = await CallToolAsync(
            "azmcp_appconfig_account_list",
            new()
            {
                { "subscription", _subscriptionId }
            });

        // assert
        var accountsArray = result.AssertProperty(AccountsKey);
        Assert.Equal(JsonValueKind.Array, accountsArray.ValueKind);
        Assert.NotEmpty(accountsArray.EnumerateArray());
        Assert.Contains(accountsArray.EnumerateArray(), acc => acc.GetProperty("name").GetString() == _accountName);
    }

    [Fact]
    public async Task Should_list_appconfig_kvs()
    {
        // arrange
        const string key0 = "foo";
        const string value0 = "fo-value";
        const string key1 = "bar";
        const string value1 = "bar-value";

        await _appConfigService.SetKeyValue(_accountName, key0, value0, _subscriptionId);
        await _appConfigService.SetKeyValue(_accountName, key1, value1, _subscriptionId);

        // act
        var result = await CallToolAsync(
            "azmcp_appconfig_kv_list",
            new()
            {
                { "subscription", _subscriptionId },
                { "account", _accountName }
            });

        // assert
        var kvsArray = result.AssertProperty(SettingsKey);
        Assert.Equal(JsonValueKind.Array, kvsArray.ValueKind);
        Assert.NotEmpty(kvsArray.EnumerateArray());

        var foo = kvsArray.EnumerateArray().FirstOrDefault(kv => kv.GetProperty("key").GetString() == key0);
        var bar = kvsArray.EnumerateArray().FirstOrDefault(kv => kv.GetProperty("key").GetString() == key1);
        Assert.Equal(JsonValueKind.Object, foo.ValueKind);
        Assert.Equal(value0, foo.GetProperty("value").GetString());
        Assert.Equal(JsonValueKind.Object, bar.ValueKind);
        Assert.Equal(value1, bar.GetProperty("value").GetString());
    }

    [Fact]
    public async Task Should_list_appconfig_kvs_with_key_and_label()
    {
        // arrange
        const string key = "foo1";
        const string value = "foo-value";
        const string label = "foobar";
        await _appConfigService.SetKeyValue(_accountName, key, value, _subscriptionId, label: label);

        // act
        var result = await CallToolAsync(
            "azmcp_appconfig_kv_list",
            new()
            {
                { "subscription", _subscriptionId },
                { "account", _accountName },
                { "key", key },
                { "label", label }
            });

        // assert
        var kvsArray = result.AssertProperty(SettingsKey);
        Assert.Equal(JsonValueKind.Array, kvsArray.ValueKind);
        Assert.NotEmpty(kvsArray.EnumerateArray());

        var found = kvsArray.EnumerateArray().FirstOrDefault(kv => kv.GetProperty("key").GetString() == key && kv.GetProperty("label").GetString() == label);
        Assert.Equal(JsonValueKind.Object, found.ValueKind);
        Assert.Equal(value, found.GetProperty("value").GetString());
    }

    [Fact]
    public async Task Should_lock_appconfig_kv_with_key_and_label()
    {
        // arrange
        const string key = "foo2";
        const string value = "foo-value";
        const string label = "staging";
        const string newValue = "new-value";
        try
        {
            // if it exists, unlock it
            await _appConfigService.UnlockKeyValue(_accountName, key, _subscriptionId, label: label);
        }
        catch
        {
        }
        // make sure it exists
        await _appConfigService.SetKeyValue(_accountName, key, value, _subscriptionId, label: label);

        // act
        var result = await CallToolAsync(
            "azmcp_appconfig_kv_lock",
            new()
            {
                { "subscription", _subscriptionId },
                { "account", _accountName },
                { "key", key },
                { "label", label }
            });

        // assert
        await Assert.ThrowsAnyAsync<Exception>(() => _appConfigService.SetKeyValue(_accountName, key, newValue, _subscriptionId, label: label));
    }

    [Fact]
    public async Task Should_lock_appconfig_kv_with_key()
    {
        // arrange
        const string key = "foo3";
        const string value = "foo-value";
        const string newValue = "new-value";
        try
        {
            // if it exists, unlock it
            await _appConfigService.UnlockKeyValue(_accountName, key, _subscriptionId);
        }
        catch
        {
        }
        // make sure it exists
        await _appConfigService.SetKeyValue(_accountName, key, value, _subscriptionId);

        // act
        var result = await CallToolAsync(
            "azmcp_appconfig_kv_lock",
            new()
            {
                { "subscription", _subscriptionId },
                { "account", _accountName },
                { "key", key }
            });

        // assert
        await Assert.ThrowsAnyAsync<Exception>(() => _appConfigService.SetKeyValue(_accountName, key, newValue, _subscriptionId));
    }

    [Fact]
    public async Task Should_unlock_appconfig_kv_with_key_and_label()
    {
        // arrange
        const string key = "foo4";
        const string value = "foo-value";
        const string label = "staging";
        const string newValue = "new-value";
        try
        {
            // if it exists, unlock it
            await _appConfigService.UnlockKeyValue(_accountName, key, _subscriptionId, label: label);
        }
        catch
        {
        }
        // make sure it exists
        await _appConfigService.SetKeyValue(_accountName, key, value, _subscriptionId, label: label);
        await _appConfigService.LockKeyValue(_accountName, key, _subscriptionId, label: label);

        // act
        _ = await CallToolAsync(
            "azmcp_appconfig_kv_unlock",
            new()
            {
                { "subscription", _subscriptionId },
                { "account", _accountName },
                { "key", key },
                { "label", "staging" }
            });

        // assert
        try
        {
            await _appConfigService.SetKeyValue(_accountName, key, newValue, _subscriptionId, label: label);
        }
        catch (Exception ex)
        {
            Assert.Fail($"Failed to set value after unlock: {ex.Message}");
        }
    }

    [Fact]
    public async Task Should_unlock_appconfig_kv_with_key()
    {
        // arrange
        const string key = "foo5";
        const string value = "foo-value";
        const string newValue = "new-value";
        try
        {
            // if it exists, unlock it
            await _appConfigService.UnlockKeyValue(_accountName, key, _subscriptionId);
        }
        catch
        {
        }
        // make sure it exists
        await _appConfigService.SetKeyValue(_accountName, key, value, _subscriptionId);
        await _appConfigService.LockKeyValue(_accountName, key, _subscriptionId);

        // act
        _ = await CallToolAsync(
            "azmcp_appconfig_kv_unlock",
            new()
            {
                { "subscription", _subscriptionId },
                { "account", _accountName },
                { "key", key }
            });

        // assert
        try
        {
            await _appConfigService.SetKeyValue(_accountName, key, newValue, _subscriptionId);
        }
        catch (Exception ex)
        {
            Assert.Fail($"Failed to set value after unlock: {ex.Message}");
        }
    }

    [Fact]
    public async Task Should_show_appconfig_kv()
    {
        // arrange
        const string key = "foo6";
        const string value = "foo-value";
        const string label = "staging";
        await _appConfigService.SetKeyValue(_accountName, key, value, _subscriptionId, label: label);

        // act
        var result = await CallToolAsync(
            "azmcp_appconfig_kv_show",
            new()
            {
                { "subscription", _subscriptionId },
                { "account", _accountName },
                { "key", key },
                { "label", label }
            });

        // assert
        var setting = result.AssertProperty("setting");
        Assert.Equal(JsonValueKind.Object, setting.ValueKind);
        var valueRead = setting.AssertProperty("value");
        Assert.Equal(JsonValueKind.String, valueRead.ValueKind);
        Assert.Equal(value, valueRead.GetString());
    }

    [Fact]
    public async Task Should_set_and_delete_appconfig_kv()
    {
        // arrange
        const string key = "foo7";
        const string value = "funkyfoo";

        // act and assert
        var result = await CallToolAsync(
            "azmcp_appconfig_kv_set",
            new()
            {
                { "subscription", _subscriptionId },
                { "account", _accountName },
                { "key", key },
                { "value", value }
            });

        var valueRead = result.AssertProperty("value");
        Assert.Equal(value, valueRead.GetString());

        result = await CallToolAsync(
            "azmcp_appconfig_kv_delete",
            new()
            {
                { "subscription", _subscriptionId },
                { "account", _accountName },
                { "key", key }
            });

        var keyProperty = result.AssertProperty("key");
        Assert.Equal(key, keyProperty.GetString());
    }

    [Fact]
    [Trait("Category", "Live")]
    public async Task Should_set_and_get_appconfig_kv_with_content_type()
    {
        // arrange
        const string key = "config-with-content-type";
        const string value = @"{""property"":""value""}";
        const string contentType = "application/json";

        // act - set key-value with content type
        var setResult = await CallToolAsync(
            "azmcp_appconfig_kv_set",
            new()
            {
                { "subscription", _subscriptionId },
                { "account", _accountName },
                { "key", key },
                { "value", value },
                { "content-type", contentType }
            });

        // assert - verify the set result
        var valueRead = setResult.AssertProperty("value");
        Assert.Equal(value, valueRead.GetString());

        var contentTypeRead = setResult.AssertProperty("contentType");
        Assert.Equal(JsonValueKind.String, contentTypeRead.ValueKind);
        Assert.Equal(contentType, contentTypeRead.GetString());

        // act - get the key-value to verify content type was stored
        var getResult = await CallToolAsync(
            "azmcp_appconfig_kv_show",
            new()
            {
                { "subscription", _subscriptionId },
                { "account", _accountName },
                { "key", key }
            });

        // assert - verify the get result
        var setting = getResult.AssertProperty("setting");
        Assert.Equal(JsonValueKind.Object, setting.ValueKind);

        valueRead = setting.AssertProperty("value");
        Assert.Equal(JsonValueKind.String, valueRead.ValueKind);
        Assert.Equal(value, valueRead.GetString());

        contentTypeRead = setting.AssertProperty("contentType");
        Assert.Equal(JsonValueKind.String, contentTypeRead.ValueKind);
        Assert.Equal(contentType, contentTypeRead.GetString());
    }

    [Fact]
    [Trait("Category", "Live")]
    public async Task Should_set_and_get_content_type_directly_using_service()
    {
        // arrange
        const string key = "service-content-type-test";
        const string value = @"{""name"":""testValue"",""enabled"":true}";
        const string contentType = "application/json; charset=utf-8";

        // act - set key-value with content type
        await _appConfigService.SetKeyValue(
           _accountName,
           key,
           value,
           _subscriptionId,
           contentType: contentType);

        // act - get key-value to verify content type was preserved
        var setting = await _appConfigService.GetKeyValue(
            _accountName,
            key,
            _subscriptionId);

        // assert - verify content type was properly set and retrieved
        Assert.Equal(key, setting.Key);
        Assert.Equal(value, setting.Value);
        Assert.Equal(contentType, setting.ContentType);

    }

    [Fact]
    [Trait("Category", "Live")]
    public async Task Should_set_kv_with_single_tag()
    {
        // arrange
        const string key = "tag-test-single";
        const string value = "tag-test-value";
        const string tagKey = "environment";
        const string tagValue = "production";

        // act - set key-value with a single tag
        var setResult = await CallToolAsync(
            "azmcp_appconfig_kv_set",
            new()
            {
                    { "subscription", _subscriptionId },
                    { "account", _accountName },
                    { "key", key },
                    { "value", value },
                    { "tags", $"{tagKey}={tagValue}" }
            });

        // assert - verify the set result
        var valueRead = setResult.AssertProperty("value");
        Assert.Equal(value, valueRead.GetString());

        var tagsRead = setResult.AssertProperty("tags");
        Assert.Equal(JsonValueKind.Array, tagsRead.ValueKind);
        Assert.Single(tagsRead.EnumerateArray());
        Assert.Equal($"{tagKey}={tagValue}", tagsRead.EnumerateArray().First().GetString());
    }

    [Fact]
    [Trait("Category", "Live")]
    public async Task Should_set_kv_with_multiple_tags()
    {
        // arrange
        const string key = "tag-test-multiple";
        const string value = "tag-test-value-multiple";
        var tags = new string[] { "environment=staging", "version=1.0.0", "region=westus2" };

        // act - set key-value with multiple tags
        var setResult = await CallToolAsync(
            "azmcp_appconfig_kv_set",
            new()
            {
                    { "subscription", _subscriptionId },
                    { "account", _accountName },
                    { "key", key },
                    { "value", value },
                    { "tags", tags }
            });

        // assert - verify the set result
        var valueRead = setResult.AssertProperty("value");
        Assert.Equal(value, valueRead.GetString());

        var tagsRead = setResult.AssertProperty("tags");
        Assert.Equal(JsonValueKind.Array, tagsRead.ValueKind);

        var tagArray = tagsRead.EnumerateArray().ToArray();
        Assert.Equal(tags.Length, tagArray.Length);

        foreach (var tag in tags)
        {
            Assert.Contains(tagArray, t => t.GetString() == tag);
        }
    }

    [Fact]
    [Trait("Category", "Live")]
    public async Task Should_set_kv_with_tags_containing_spaces()
    {
        // arrange
        const string key = "tag-test-spaces";
        const string value = "tag-test-value-spaces";
        var tags = new string[]
        {
            "complex key=complex value with spaces",
            "deployment environment=Production US West",
            "created by=Azure MCP Test"
        };

        // act - set key-value with tags containing spaces
        var setResult = await CallToolAsync(
            "azmcp_appconfig_kv_set",
            new()
            {
                    { "subscription", _subscriptionId },
                    { "account", _accountName },
                    { "key", key },
                    { "value", value },
                    { "tags", tags }
            });

        // assert - verify the set result
        var valueRead = setResult.AssertProperty("value");
        Assert.Equal(value, valueRead.GetString());

        var tagsRead = setResult.AssertProperty("tags");
        Assert.Equal(JsonValueKind.Array, tagsRead.ValueKind);

        var tagArray = tagsRead.EnumerateArray().ToArray();
        Assert.Equal(3, tagArray.Length);

        foreach (var tag in tags)
        {
            Assert.Contains(tagArray, t => t.GetString() == tag);
        }
    }
}
