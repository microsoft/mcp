// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Tests.Helpers;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Core.Tests.RecordingFramework;

[Collection(DirectRecordedEnvironmentCollection.Name)]
public sealed class DirectPlaybackPollingTests
{
    [Fact]
    public async Task WaitAsync_ReevaluatesPlaybackModeAfterServiceInitialization()
    {
#if !DEBUG
        Assert.Skip("Playback polling is only enabled in Debug builds.");
#endif
        using var environment = new TestEnvironmentScope(
            new Dictionary<string, string?> { ["TEST_MODE"] = "Live" });
        var token = TestContext.Current.CancellationToken;
        var response = Substitute.For<Response>();
        var live = Substitute.For<Operation>();
        live.WaitForCompletionResponseAsync(token).Returns(response);
        await TestPollingAzureService.WaitAsync(live, token);
        await live.Received(1).WaitForCompletionResponseAsync(token);

        environment.SetVariables(new Dictionary<string, string?> { ["TEST_MODE"] = "Playback" });
        var playback = Substitute.For<Operation>();
        playback.HasCompleted.Returns(true);
        playback.UpdateStatusAsync(token).Returns(response);
        await TestPollingAzureService.WaitAsync(playback, token);

        await playback.Received(1).UpdateStatusAsync(token);
        await playback.DidNotReceive().WaitForCompletionResponseAsync(token);
    }

    [Fact]
    public async Task WaitAsync_GenericOperationUsesPlaybackPolling()
    {
#if !DEBUG
        Assert.Skip("Playback polling is only enabled in Debug builds.");
#endif
        using var environment = new TestEnvironmentScope(
            new Dictionary<string, string?> { ["TEST_MODE"] = "Playback" });
        var token = TestContext.Current.CancellationToken;
        var playback = Substitute.For<Operation<string>>();
        playback.HasCompleted.Returns(true);
        playback.UpdateStatusAsync(token).Returns(Substitute.For<Response>());

        await TestPollingAzureService.WaitAsync(playback, token);

        await playback.Received(1).UpdateStatusAsync(token);
        await playback.DidNotReceive().WaitForCompletionAsync(token);
    }
}
