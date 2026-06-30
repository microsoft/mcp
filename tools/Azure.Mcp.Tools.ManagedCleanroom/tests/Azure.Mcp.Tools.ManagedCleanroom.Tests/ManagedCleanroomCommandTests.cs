// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.Mcp.Tests;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Microsoft.Mcp.Tests.Generated.Models;
using Microsoft.Mcp.Tests.Helpers;
using Xunit;

namespace Azure.Mcp.Tools.ManagedCleanroom.Tests;

[Trait("Command", "CollaborationsListCommand")]
[Trait("Command", "CollaborationCreateCommand")]
public class ManagedCleanroomCommandTests(ITestOutputHelper output, TestProxyFixture fixture, LiveServerFixture liveServerFixture)
    : RecordedCommandTestsBase(output, fixture, liveServerFixture)
{
    [Fact]
    public async Task Should_list_collaborations()
    {
        var endpoint = RegisterOrRetrieveVariable(
            "cleanroomEndpoint",
            Settings.DeploymentOutputs.GetValueOrDefault("CLEANROOM_ENDPOINT", string.Empty));

        if (Settings.TestMode != Microsoft.Mcp.Tests.Helpers.TestMode.Playback)
        {
            Assert.SkipWhen(
                string.IsNullOrWhiteSpace(endpoint),
                "CLEANROOM_ENDPOINT deployment output is missing; cannot run data-plane collaboration list test.");
        }

        var result = await CallToolAsync(
            "managedcleanroom_collaborations_list",
            new()
            {
                { "endpoint", endpoint },
                { "active-only", true }
            });

        Assert.NotNull(result);
        Assert.Equal(JsonValueKind.Object, result.Value.ValueKind);

        if (result.Value.TryGetProperty("collaborations", out var collaborations))
        {
            Assert.Equal(JsonValueKind.Array, collaborations.ValueKind);
        }
    }

    [Fact]
    public async Task Should_create_collaboration_arm_resource()
    {
        var location = RegisterOrRetrieveVariable(
            "location",
            Settings.DeploymentOutputs.GetValueOrDefault("CLEANROOM_LOCATION", 
                Settings.DeploymentOutputs.GetValueOrDefault("LOCATION", string.Empty)));

        if (string.IsNullOrWhiteSpace(location))
        {
            location = Settings.DeploymentOutputs.GetValueOrDefault("LOCATION", string.Empty);
        }

        if (Settings.TestMode != Microsoft.Mcp.Tests.Helpers.TestMode.Playback)
        {
            Assert.SkipWhen(
                string.IsNullOrWhiteSpace(location),
                "LOCATION deployment output is missing; cannot run collaboration ARM create test.");
        }

        var recordedCollaborationName = RegisterOrRetrieveVariable(
            "collaborationName",
            $"{Settings.ResourceBaseName}-collab-{Guid.NewGuid().ToString("N")[..8]}");

        var collaborationName = recordedCollaborationName;
        if (Settings.TestMode == Microsoft.Mcp.Tests.Helpers.TestMode.Playback)
        {
            var marker = "-collab-";
            var markerIndex = recordedCollaborationName.IndexOf(marker, StringComparison.Ordinal);
            if (markerIndex >= 0)
            {
                var suffix = recordedCollaborationName[(markerIndex + marker.Length)..];
                collaborationName = $"Sanitized-collab-{suffix}";
            }
        }

        var result = await CallToolAsync(
            "managedcleanroom_collaborationarm_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "name", collaborationName },
                { "location", location }
            });

        Assert.NotNull(result);
        Assert.Equal(JsonValueKind.Object, result.Value.ValueKind);

        var name = result.Value.AssertProperty("name");
        Assert.Equal(JsonValueKind.String, name.ValueKind);
        if (Settings.TestMode == Microsoft.Mcp.Tests.Helpers.TestMode.Playback)
        {
            Assert.False(string.IsNullOrWhiteSpace(name.GetString()));
        }
        else
        {
            Assert.Equal(collaborationName, name.GetString());
        }

        var provisioningState = result.Value.AssertProperty("provisioningState");
        Assert.Equal(JsonValueKind.String, provisioningState.ValueKind);
        Assert.Equal("Accepted", provisioningState.GetString());
    }
}
