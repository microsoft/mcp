// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Microsoft.Mcp.Tests.Helpers;
using NSubstitute;
using Xunit;
using Xunit.v3;

namespace Azure.Mcp.Core.Tests.RecordingFramework;

[Collection(DirectRecordedEnvironmentCollection.Name)]
public sealed class TestEnvironmentScopeTests
{
    [Fact]
    public void Dispose_RestoresOriginalAndNewValues()
    {
        string name = $"MCP_SCOPE_TEST_{Guid.NewGuid():N}";
        using var outer = new TestEnvironmentScope(new Dictionary<string, string?> { [name] = "original" });
        var scope = new TestEnvironmentScope(new Dictionary<string, string?> { [name] = "changed" });
        scope.SetVariables(new Dictionary<string, string?> { [name] = "changed-again" });
        Assert.Equal("changed-again", Environment.GetEnvironmentVariable(name));

        scope.Dispose();
        scope.Dispose();

        Assert.Equal("original", Environment.GetEnvironmentVariable(name));
    }

    [Fact]
    public void Dispose_RemovesVariablesIntroducedAfterConstruction()
    {
        string name = $"MCP_SCOPE_TEST_{Guid.NewGuid():N}";
        var scope = new TestEnvironmentScope(new Dictionary<string, string?>());
        scope.SetVariables(new Dictionary<string, string?> { [name] = "temporary" });

        scope.Dispose();

        Assert.Null(Environment.GetEnvironmentVariable(name));
        Assert.Throws<ObjectDisposedException>(() =>
            scope.SetVariables(new Dictionary<string, string?> { [name] = "invalid" }));
    }

    [Fact]
    public void EnvironmentClearing_DoesNotRemoveDirectFixtureConfiguration()
    {
        using var scope = new TestEnvironmentScope(
            new Dictionary<string, string?> { ["AZURE_TOKEN_CREDENTIALS"] = "test-credential" });
        var method = typeof(RecordedToolTestsBase).GetMethod(nameof(RecordedToolTestsBase.InitializeAsync))!;

        new ClearEnvironmentVariablesBeforeTestAttribute().Before(method, Substitute.For<IXunitTest>());

        Assert.Equal("test-credential", Environment.GetEnvironmentVariable("AZURE_TOKEN_CREDENTIALS"));
    }

    [Fact]
    public async Task FailedInitialization_RestoresEnvironmentWithoutStartingServer()
    {
        string name = $"MCP_SCOPE_TEST_{Guid.NewGuid():N}";
        using var outer = new TestEnvironmentScope(new Dictionary<string, string?> { [name] = "original" });
        string? originalMode = Environment.GetEnvironmentVariable("TEST_MODE");
        var fixture = new TestProxyFixture();
        var harness = new FailingRecordedToolTests(Substitute.For<ITestOutputHelper>(), fixture, name);

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => harness.InitializeAsync().AsTask());

        Assert.Equal("test configuration failure", error.Message);
        Assert.Equal("original", Environment.GetEnvironmentVariable(name));
        Assert.Equal(originalMode, Environment.GetEnvironmentVariable("TEST_MODE"));
        Assert.Null(fixture.Proxy);
        await harness.DisposeAsync();
        await fixture.DisposeAsync();
    }

    [Fact]
    public async Task FailedLiveInitialization_RestoresEnvironmentWithoutStartingServer()
    {
        string name = $"MCP_SCOPE_TEST_{Guid.NewGuid():N}";
        using var outer = new TestEnvironmentScope(new Dictionary<string, string?> { [name] = "original" });
        string? originalMode = Environment.GetEnvironmentVariable("TEST_MODE");
        var harness = new FailingDirectToolTests(Substitute.For<ITestOutputHelper>(), name);

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => harness.InitializeAsync().AsTask());

        Assert.Equal("test configuration failure", error.Message);
        Assert.Equal("original", Environment.GetEnvironmentVariable(name));
        Assert.Equal(originalMode, Environment.GetEnvironmentVariable("TEST_MODE"));
        await harness.DisposeAsync();
    }

    [Fact]
    public void EnvironmentClearing_DoesNotRemoveLiveFixtureConfiguration()
    {
        using var scope = new TestEnvironmentScope(
            new Dictionary<string, string?> { ["AZURE_TOKEN_CREDENTIALS"] = "test-credential" });
        var method = typeof(DirectToolTestsBase).GetMethod(nameof(DirectToolTestsBase.InitializeAsync))!;

        new ClearEnvironmentVariablesBeforeTestAttribute().Before(method, Substitute.For<IXunitTest>());

        Assert.Equal("test-credential", Environment.GetEnvironmentVariable("AZURE_TOKEN_CREDENTIALS"));
    }
}
