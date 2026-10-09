// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Mcp.Tests;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Microsoft.Mcp.Tests.Generated.Models;
using Xunit;

namespace Azure.Mcp.Tools.AzureMigrate.Tests;

public class AzureMigrateCommandTests(ITestOutputHelper output, TestProxyFixture fixture, LiveServerFixture liveServerFixture)
    : RecordedCommandTestsBase(output, fixture, liveServerFixture)
{
    // A public IP placeholder keeps endpoint validation enabled without DNS; playback routes it through the proxy.
    private const string SanitizedDownloadUrl = "https://8.8.8.8/artifacts/output.zip?sig=Sanitized";

    public override List<GeneralRegexSanitizer> GeneralRegexSanitizers { get; } =
    [
        // Match the sanitized SAS response and blob request without retaining the live account, path or signature.
        new(new()
        {
            Regex = @"(?i)https://[a-z0-9-]+\.blob\.core\.windows\.net/[^\s""\\]*",
            Value = SanitizedDownloadUrl
        }),
        new(new()
        {
            Regex = @"(?i)[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}",
            Value = "00000000-0000-0000-0000-000000000000"
        }),
        new(new()
        {
            Regex = @"/resource[gG]roups/(?<name>[^/""\s]+)/",
            GroupForReplace = "name",
            Value = "Sanitized"
        })
    ];

    public override List<BodyKeySanitizer> BodyKeySanitizers =>
    [
        .. base.BodyKeySanitizers,
        new(new("$..displayName")
        {
            Value = "Sanitized"
        }),
        new(new("$..organizationName")
        {
            Value = "Sanitized"
        }),
        new(new("$..serviceName")
        {
            Value = "Sanitized"
        }),
        new(new("$..sasUrl")
        {
            Value = SanitizedDownloadUrl
        }),
        new(new("$..versionId")
        {
            Value = "Sanitized"
        }),
        new(new("$..latestVersionId")
        {
            Value = "Sanitized"
        })
    ];

    public override List<HeaderRegexSanitizer> HeaderRegexSanitizers =>
    [
        .. base.HeaderRegexSanitizers,
        // The CLI user agent embeds the host OS and framework moniker, which differ between the machine that
        // produced the recording and the CI agents that replay it. Normalize it so matching is platform independent.
        new(new("User-Agent")
        {
            Value = "Sanitized"
        })
    ];

    public override List<string> DisabledDefaultSanitizers =>
    [
        ..base.DisabledDefaultSanitizers,
        "AZSDK2003"
    ];

    [Fact]
    public async Task Should_list_platform_landing_zones()
    {
        if (await AssertLocalToolIsUnavailableInHttpMode("azuremigrate_platformlandingzone_request"))
        {
            return;
        }

        var result = await CallToolAsync(
            "azuremigrate_platformlandingzone_request",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "migrate-project-name", Settings.ResourceBaseName },
                { "action", "list" }
            });

        var message = result.AssertProperty("message");
        Assert.Equal(JsonValueKind.String, message.ValueKind);
        var messageText = message.GetString();
        Assert.NotNull(messageText);
        Assert.True(
            messageText.Contains("Platform Landing Zones under migrate project", StringComparison.OrdinalIgnoreCase) ||
            messageText.Contains("No Platform Landing Zone exists under migrate project", StringComparison.OrdinalIgnoreCase),
            "Expected list result message");
    }

    [Fact]
    public async Task Should_get_platform_landing_zone()
    {
        if (await AssertLocalToolIsUnavailableInHttpMode("azuremigrate_platformlandingzone_request"))
        {
            return;
        }

        var result = await CallToolAsync(
            "azuremigrate_platformlandingzone_request",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "migrate-project-name", Settings.ResourceBaseName },
                { "action", "get" }
            });

        var message = result.AssertProperty("message");
        Assert.Equal(JsonValueKind.String, message.ValueKind);
        var messageText = message.GetString();
        Assert.NotNull(messageText);
        Assert.True(
            messageText.Contains("Generation status", StringComparison.OrdinalIgnoreCase) ||
            messageText.Contains("No Platform Landing Zone exists under migrate project", StringComparison.OrdinalIgnoreCase),
            "Expected get result message");
    }

    [Fact]
    public async Task Should_create_wait_and_download_platform_landing_zone()
    {
        const string ToolName = "azuremigrate_platformlandingzone_request";
        if (await AssertLocalToolIsUnavailableInHttpMode(ToolName))
        {
            return;
        }

        Dictionary<string, object?> parameters = new()
        {
            { "subscription", Settings.SubscriptionId },
            { "resource-group", Settings.ResourceGroupName },
            { "migrate-project-name", Settings.ResourceBaseName },
            { "action", "get" }
        };
        var existing = await CallToolAsync(ToolName, parameters);
        var existingMessage = existing.AssertProperty("message").GetString();
        Assert.NotNull(existingMessage);
        Assert.True(
            existingMessage.StartsWith("No Platform Landing Zone exists", StringComparison.Ordinal) ||
            GetGenerationStatus(existingMessage) is "Succeeded" or "Failed" or "Canceled",
            "Do not regenerate an in-flight or unknown landing zone.");

        // No configuration overrides: retain the existing topology and components when regenerating default.
        parameters["action"] = "create";
        var created = await CallToolAsync(ToolName, parameters);
        Assert.Contains("'default' was submitted", created.AssertProperty("message").GetString());

        parameters["action"] = "wait";
        parameters["timeout-minutes"] = 30;
        var completed = await CallToolAsync(ToolName, parameters);
        var completedMessage = completed.AssertProperty("message").GetString();
        Assert.NotNull(completedMessage);
        Assert.Equal("Succeeded", GetGenerationStatus(completedMessage));

        var path = Path.Combine(Environment.CurrentDirectory, $"{Settings.ResourceBaseName}-default-output.zip");
        Assert.False(File.Exists(path), "The test must not overwrite an existing local archive.");
        try
        {
            parameters["action"] = "download";
            parameters.Remove("timeout-minutes");
            var downloaded = await CallToolAsync(ToolName, parameters);
            Assert.Contains(path, downloaded.AssertProperty("message").GetString());
            Assert.True(File.Exists(path), "The MCP download action must write the archive.");

            using var archive = ZipFile.OpenRead(path);
            Assert.NotEmpty(archive.Entries);
            var terraformFiles = archive.Entries.Where(entry => entry.FullName.EndsWith(".tf", StringComparison.Ordinal)).ToList();
            Assert.NotEmpty(terraformFiles);
            Assert.All(terraformFiles, entry => Assert.True(entry.Length > 0));
            foreach (var entry in archive.Entries)
            {
                await using var stream = entry.Open();
                await stream.CopyToAsync(Stream.Null, TestContext.Current.CancellationToken);
            }

            var bytes = await File.ReadAllBytesAsync(path, TestContext.Current.CancellationToken);
            Output.WriteLine(
                $"Downloaded ZIP: {bytes.Length} bytes, {archive.Entries.Count} entries, {terraformFiles.Count} Terraform files, SHA256 {Convert.ToHexStringLower(SHA256.HashData(bytes))}.");
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static string? GetGenerationStatus(string message)
    {
        const string Prefix = "Generation status: ";
        var line = message.Split('\n').FirstOrDefault(line => line.StartsWith(Prefix, StringComparison.Ordinal));
        return line?[Prefix.Length..].Trim();
    }

    [Fact]
    public async Task Should_handle_invalid_action()
    {
        if (await AssertLocalToolIsUnavailableInHttpMode("azuremigrate_platformlandingzone_request"))
        {
            return;
        }

        try
        {
            await CallToolAsync(
                "azuremigrate_platformlandingzone_request",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "migrate-project-name", Settings.ResourceBaseName },
                    { "action", "invalidaction" }
                });

            Assert.Fail("Expected an exception for invalid action");
        }
        catch (Exception ex)
        {
            Assert.Contains("Invalid action", ex.Message, StringComparison.OrdinalIgnoreCase);
        }
    }

}
