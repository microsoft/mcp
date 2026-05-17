// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using NSubstitute;

namespace Fabric.Mcp.Tools.DataFactory.UnitTests;

public class ProviderDelegatingCredentialTests
{
    [Fact]
    public async Task GetTokenAsync_DelegatesToProviderOnEachCall()
    {
        // Arrange — two distinct credentials to prove no caching
        var credential1 = Substitute.For<TokenCredential>();
        credential1.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new AccessToken("token1", DateTimeOffset.UtcNow.AddHours(1)));

        var credential2 = Substitute.For<TokenCredential>();
        credential2.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new AccessToken("token2", DateTimeOffset.UtcNow.AddHours(1)));

        var provider = Substitute.For<IAzureTokenCredentialProvider>();
        provider.GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(credential1, credential2);

        var services = new ServiceCollection();
        services.AddSingleton(provider);
        services.AddSingleton<TokenCredential, ProviderDelegatingCredential>();

        await using var sp = services.BuildServiceProvider();
        var resolved = sp.GetRequiredService<TokenCredential>();

        var context = new TokenRequestContext(["https://api.fabric.microsoft.com/.default"]);

        // Act
        var token1 = await resolved.GetTokenAsync(context, CancellationToken.None);
        var token2 = await resolved.GetTokenAsync(context, CancellationToken.None);

        // Assert — each call got a fresh credential from the provider (not cached)
        Assert.Equal("token1", token1.Token);
        Assert.Equal("token2", token2.Token);
        await provider.Received(2).GetTokenCredentialAsync(null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetTokenAsync_PassesRequestContextToCredential()
    {
        // Arrange
        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new AccessToken("test-token", DateTimeOffset.UtcNow.AddHours(1)));

        var provider = Substitute.For<IAzureTokenCredentialProvider>();
        provider.GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(credential);

        var delegating = new ProviderDelegatingCredential(provider);
        var scopes = new[] { "https://analysis.windows.net/powerbi/api/.default" };
        var context = new TokenRequestContext(scopes);

        // Act
        var result = await delegating.GetTokenAsync(context, CancellationToken.None);

        // Assert
        Assert.Equal("test-token", result.Token);
        await credential.Received(1).GetTokenAsync(
            Arg.Is<TokenRequestContext>(ctx => ctx.Scopes[0] == scopes[0]),
            Arg.Any<CancellationToken>());
    }
}
