// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Runtime.InteropServices;
using System.Text.Json;
using Azure.Core;
using Azure.Identity;
using Azure.Identity.Broker;
using Azure.Mcp.Core.Services.Azure.Authentication;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Core.Services.Caching;
using Azure.Mcp.Tests;
using Azure.ResourceManager.Resources;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Core.LiveTests.Services.Azure.Authentication;

public class AuthenticationIntegrationTests : IAsyncLifetime
{
    private readonly ServiceProvider _serviceProvider;
    private readonly ISubscriptionService _subscriptionService;
    private readonly ITestOutputHelper _output;

    public AuthenticationIntegrationTests(ITestOutputHelper output)
    {
        _output = output;

        // Set up real service dependencies for integration test
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(Substitute.For<ICacheService>());
        services.AddSingleton(Substitute.For<ITenantService>());
        services.AddSingleton<ISubscriptionService, SubscriptionService>();

        _serviceProvider = services.BuildServiceProvider();
        _subscriptionService = _serviceProvider.GetRequiredService<ISubscriptionService>();
    }

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync()
    {
        await _serviceProvider.DisposeAsync();
    }

    [Fact]
    public async Task LoginWithIdentityBroker_ThenListSubscriptions_ShouldSucceed()
    {
        Assert.SkipWhen(TestExtensions.IsRunningFromDotnetTest(), TestExtensions.RunningFromDotnetTestReason);
        Assert.SkipWhen(RuntimeInformation.IsOSPlatform(OSPlatform.OSX), "Identity broker is not supported on MacOS");

        _output.WriteLine("Testing InteractiveBrowserCredential with identity broker...");

        await AuthenticateWithBrokerAsync();
        _output.WriteLine("Successfully authenticated with identity broker");

        // Step 2: Now test the subscription service which will use our CustomChainedCredential internally
        _output.WriteLine("Testing subscription listing with authenticated credential...");

        var subscriptions = await _subscriptionService.GetSubscriptions();
        ValidateAndLogSubscriptions(subscriptions);
    }

    private static async Task<TokenCredential> AuthenticateWithBrokerAsync()
    {
        var browserCredential = new InteractiveBrowserCredential(
            new InteractiveBrowserCredentialBrokerOptions(WindowHandleProvider.GetWindowHandle())
        );

        // Verify the credential works by requesting a token
        var armScope = "https://management.azure.com/.default";
        var context = new TokenRequestContext([armScope]);
        var token = await browserCredential.GetTokenAsync(context);

        Assert.NotNull(token.Token);
        Assert.NotEqual(default, token.ExpiresOn);

        return browserCredential;
    }
    private void ValidateAndLogSubscriptions(List<SubscriptionData> subscriptions)
    {
        // Validate subscriptions
        Assert.NotNull(subscriptions);
        Assert.NotEmpty(subscriptions);

        // Verify subscription data structure
        foreach (var subscription in subscriptions)
        {
            Assert.NotNull(subscription.SubscriptionId);
            Assert.NotEmpty(subscription.SubscriptionId);
            Assert.NotNull(subscription.DisplayName);
            Assert.NotEmpty(subscription.DisplayName);
        }

        // Output subscriptions for manual verification
        var jsonString = JsonSerializer.Serialize(subscriptions, s_writeIndentedOptions);
        _output.WriteLine($"Retrieved {subscriptions.Count} subscriptions:");
        _output.WriteLine(jsonString);
    }

    private static readonly JsonSerializerOptions s_writeIndentedOptions = new() { WriteIndented = true };
}
