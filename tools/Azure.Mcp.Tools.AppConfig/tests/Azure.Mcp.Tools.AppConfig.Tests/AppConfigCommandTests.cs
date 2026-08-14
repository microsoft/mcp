// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.Mcp.Tests;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Microsoft.Mcp.Tests.Generated.Models;
using Xunit;

namespace Azure.Mcp.Tools.AppConfig.Tests;

public class AppConfigCommandTests(ITestOutputHelper output, TestProxyFixture fixture, LiveServerFixture liveServerFixture)
    : RecordedCommandTestsBase(output, fixture, liveServerFixture)
{
    private const string AccountsKey = "accounts";
    private const string SettingsKey = "settings";

    /// <summary>
    /// AZSDK3493 = $..name
    /// AZSDK3447 = $..key
    /// </summary>
    public override List<string> DisabledDefaultSanitizers => [.. base.DisabledDefaultSanitizers, "AZSDK3493", "AZSDK3447"];

    public override List<BodyRegexSanitizer> BodyRegexSanitizers =>
    [
        new(new() {
          Regex = "\"domains\"\\s*:\\s*\\[(?s)(?<domains>.*?)\\]",
          GroupForReplace = "domains",
          Value = "\"contoso.com\""
        })
    ];

    [Fact]
    public async Task Should_list_appconfig_accounts()
    {
        // act
        var result = await CallToolAsync(
            "appconfig_account_list",
            new()
            {
                { "subscription", Settings.SubscriptionId }
            });

        // assert
        var accountsArray = result.AssertProperty(AccountsKey);
        Assert.Equal(JsonValueKind.Array, accountsArray.ValueKind);
        Assert.NotEmpty(accountsArray.EnumerateArray());
        Assert.Contains(accountsArray.EnumerateArray(), acc => acc.GetProperty("name").GetString() == Settings.ResourceBaseName);
    }

    [Fact]
    public async Task Should_list_appconfig_kvs()
    {
        // arrange
        const string key0 = "foo";
        const string value0 = "fo-value";
        const string key1 = "bar";
        const string value1 = "bar-value";

        await SetKey(key0, value0);
        await SetKey(key1, value1);

        // act
        var result = await GetKey();

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

        await SetKey(key, value, label: label);

        // act
        var result = await GetKey(keyFilter: key, labelFilter: label);

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

        // if it exists, unlock it
        await SetLock(key, false, label: label);

        // make sure it exists
        await SetKey(key, value, label: label);

        // act
        await SetLock(key, true, label: label);
        await SetKey(key, newValue, label: label);

        // assert
        var getResult = await GetKey(key: key, label: label);

        var settings = getResult.AssertProperty("settings");
        Assert.Equal(JsonValueKind.Array, settings.ValueKind);
        Assert.Single(settings.EnumerateArray());

        var setting = settings.EnumerateArray().First();
        Assert.Equal(JsonValueKind.Object, setting.ValueKind);

        var valueRead = setting.AssertProperty("value");
        Assert.Equal(JsonValueKind.String, valueRead.ValueKind);
        Assert.Equal(value, valueRead.GetString());
    }

    [Fact]
    public async Task Should_lock_appconfig_kv_with_key()
    {
        // arrange
        const string key = "foo3";
        const string value = "foo-value";
        const string newValue = "new-value";

        // if it exists, unlock it
        await SetLock(key, false);

        // make sure it exists
        await SetKey(key, value);

        // act
        await SetLock(key, true);
        await SetKey(key, newValue);

        // assert
        var getResult = await GetKey(key: key);

        var settings = getResult.AssertProperty("settings");
        Assert.Equal(JsonValueKind.Array, settings.ValueKind);
        Assert.Single(settings.EnumerateArray());

        var setting = settings.EnumerateArray().First();
        Assert.Equal(JsonValueKind.Object, setting.ValueKind);

        var valueRead = setting.AssertProperty("value");
        Assert.Equal(JsonValueKind.String, valueRead.ValueKind);
        Assert.Equal(value, valueRead.GetString());
    }

    [Fact]
    public async Task Should_unlock_appconfig_kv_with_key_and_label()
    {
        // arrange
        const string key = "foo4";
        const string value = "foo-value";
        const string label = "staging";
        const string newValue = "new-value";

        // if it exists, unlock it
        await SetLock(key, false, label: label);

        // make sure it exists
        await SetKey(key, value, label: label);
        await SetLock(key, true, label: label);

        // act
        await SetLock(key, false, label: label);
        await SetKey(key, newValue, label: label);

        // assert
        var getResult = await GetKey(key: key, label: label);

        var settings = getResult.AssertProperty("settings");
        Assert.Equal(JsonValueKind.Array, settings.ValueKind);
        Assert.Single(settings.EnumerateArray());

        var setting = settings.EnumerateArray().First();
        Assert.Equal(JsonValueKind.Object, setting.ValueKind);

        var valueRead = setting.AssertProperty("value");
        Assert.Equal(JsonValueKind.String, valueRead.ValueKind);
        Assert.Equal(newValue, valueRead.GetString());
    }

    [Fact]
    public async Task Should_unlock_appconfig_kv_with_key()
    {
        // arrange
        const string key = "foo5";
        const string value = "foo-value";
        const string newValue = "new-value";

        // if it exists, unlock it
        await SetLock(key, false);

        // make sure it exists
        await SetKey(key, value);
        await SetLock(key, true);

        // act
        await SetLock(key, false);
        await SetKey(key, newValue);

        // assert
        var getResult = await GetKey(key: key);

        var settings = getResult.AssertProperty("settings");
        Assert.Equal(JsonValueKind.Array, settings.ValueKind);
        Assert.Single(settings.EnumerateArray());

        var setting = settings.EnumerateArray().First();
        Assert.Equal(JsonValueKind.Object, setting.ValueKind);

        var valueRead = setting.AssertProperty("value");
        Assert.Equal(JsonValueKind.String, valueRead.ValueKind);
        Assert.Equal(newValue, valueRead.GetString());
    }

    [Fact]
    public async Task Should_show_appconfig_kv()
    {
        // arrange
        const string key = "foo6";
        const string value = "foo-value";
        const string label = "staging";
        await SetKey(key, value, label: label);

        // act
        var result = await GetKey(key: key, label: label);

        // assert
        var settings = result.AssertProperty("settings");
        Assert.Equal(JsonValueKind.Array, settings.ValueKind);
        Assert.Single(settings.EnumerateArray());

        var setting = settings.EnumerateArray().First();
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
        var result = await SetKey(key, value);

        var valueRead = result.AssertProperty("value");
        Assert.Equal(value, valueRead.GetString());

        result = await CallToolAsync(
            "appconfig_kv_delete",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "account", Settings.ResourceBaseName },
                { "key", key }
            });

        var keyProperty = result.AssertProperty("key");
        Assert.Equal(key, keyProperty.GetString());
    }

    [Fact]
    public async Task Should_set_and_get_appconfig_kv_with_content_type()
    {
        // arrange
        const string key = "config-with-content-type";
        const string value = @"{""property"":""value""}";
        const string contentType = "application/json";

        // act - set key-value with content type
        var setResult = await SetKey(key, value, contentType: contentType);

        // assert - verify the set result
        var valueRead = setResult.AssertProperty("value");
        Assert.Equal(value, valueRead.GetString());

        var contentTypeRead = setResult.AssertProperty("contentType");
        Assert.Equal(JsonValueKind.String, contentTypeRead.ValueKind);
        Assert.Equal(contentType, contentTypeRead.GetString());

        // act - get the key-value to verify content type was stored
        var getResult = await GetKey(key: key);

        // assert - verify the get result
        var settings = getResult.AssertProperty("settings");
        Assert.Equal(JsonValueKind.Array, settings.ValueKind);
        Assert.Single(settings.EnumerateArray());

        var setting = settings.EnumerateArray().First();
        Assert.Equal(JsonValueKind.Object, setting.ValueKind);

        valueRead = setting.AssertProperty("value");
        Assert.Equal(JsonValueKind.String, valueRead.ValueKind);
        Assert.Equal(value, valueRead.GetString());

        contentTypeRead = setting.AssertProperty("contentType");
        Assert.Equal(JsonValueKind.String, contentTypeRead.ValueKind);
        Assert.Equal(contentType, contentTypeRead.GetString());
    }

    [Fact]
    public async Task Should_set_and_get_content_type_directly_using_service()
    {
        // arrange
        const string key = "service-content-type-test";
        const string value = @"{""name"":""testValue"",""enabled"":true}";
        const string contentType = "application/json; charset=utf-8";

        // act - set key-value with content type
        await SetKey(key, value, contentType: contentType);

        // act - get key-value to verify content type was preserved
        var getResult = await GetKey(key: key);

        // assert - verify content type was properly set and retrieved
        var settings = getResult.AssertProperty("settings");
        Assert.Equal(JsonValueKind.Array, settings.ValueKind);
        Assert.Single(settings.EnumerateArray());

        var setting = settings.EnumerateArray().First();
        Assert.Equal(JsonValueKind.Object, setting.ValueKind);

        var valueRead = setting.AssertProperty("value");
        Assert.Equal(JsonValueKind.String, valueRead.ValueKind);
        Assert.Equal(value, valueRead.GetString());

        var contentTypeRead = setting.AssertProperty("contentType");
        Assert.Equal(JsonValueKind.String, contentTypeRead.ValueKind);
        Assert.Equal(contentType, contentTypeRead.GetString());
    }

    [Fact]
    public async Task Should_set_kv_with_single_tag()
    {
        // arrange
        const string key = "tag-test-single";
        const string value = "tag-test-value";
        const string tagKey = "environment";
        const string tagValue = "production";

        // act - set key-value with a single tag
        var setResult = await SetKey(key, value, tags: $"{tagKey}={tagValue}");

        // assert - verify the set result
        var valueRead = setResult.AssertProperty("value");
        Assert.Equal(value, valueRead.GetString());

        var tagsRead = setResult.AssertProperty("tags");
        Assert.Equal(JsonValueKind.Array, tagsRead.ValueKind);
        Assert.Single(tagsRead.EnumerateArray());
        Assert.Equal($"{tagKey}={tagValue}", tagsRead.EnumerateArray().First().GetString());
    }

    [Fact]
    public async Task Should_set_kv_with_multiple_tags()
    {
        // arrange
        const string key = "tag-test-multiple";
        const string value = "tag-test-value-multiple";
        var tags = new string[] { "environment=staging", "version=1.0.0", "region=westus2" };

        // act - set key-value with multiple tags
        var setResult = await SetKey(key, value, tags: tags);

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
        var setResult = await SetKey(key, value, tags: tags);

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

    private async Task<JsonElement?> SetKey(string key, string value, string? label = null, string? contentType = null, object? tags = null)
    {
        var parameters = new Dictionary<string, object?>
        {
            { "subscription", Settings.SubscriptionId },
            { "account", Settings.ResourceBaseName },
            { "key", key },
            { "value", value }
        };
        AddParameterIfNotNull(parameters, "label", label);
        AddParameterIfNotNull(parameters, "content-type", contentType);
        AddParameterIfNotNull(parameters, "tags", tags);

        return await CallToolAsync("appconfig_kv_set", parameters);
    }

    private async Task<JsonElement?> GetKey(string? key = null, string? label = null, string? keyFilter = null, string? labelFilter = null)
    {
        var parameters = new Dictionary<string, object?>
        {
            { "subscription", Settings.SubscriptionId },
            { "account", Settings.ResourceBaseName }
        };
        AddParameterIfNotNull(parameters, "key", key);
        AddParameterIfNotNull(parameters, "label", label);
        AddParameterIfNotNull(parameters, "key-filter", keyFilter);
        AddParameterIfNotNull(parameters, "label-filter", labelFilter);

        return await CallToolAsync("appconfig_kv_get", parameters);
    }

    private async Task<JsonElement?> SetLock(string key, bool locked, string? label = null)
    {
        var parameters = new Dictionary<string, object?>
        {
            { "subscription", Settings.SubscriptionId },
            { "account", Settings.ResourceBaseName },
            { "key", key },
            { "lock", locked }
        };
        AddParameterIfNotNull(parameters, "label", label);

        return await CallToolAsync("appconfig_kv_lock_set", parameters);
    }

    private static void AddParameterIfNotNull(Dictionary<string, object?> parameters, string key, object? value)
    {
        if (value is not null)
        {
            parameters.Add(key, value);
        }
    }
}
