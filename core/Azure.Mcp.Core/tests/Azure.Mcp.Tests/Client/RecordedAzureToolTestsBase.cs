// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Microsoft.Mcp.Tests.Helpers;
using Xunit;

namespace Azure.Mcp.Tests.Client;

public abstract class RecordedAzureToolTestsBase<TAreaSetup>(ITestOutputHelper output, TestProxyFixture fixture)
    : RecordedToolTestsBase(output, fixture) where TAreaSetup : IAreaSetup, new()
{
    protected override IAreaSetup Area { get; } = new TAreaSetup();

    protected override void ConfigureServices(IServiceCollection services)
    {
        if (TestMode == TestMode.Playback)
        {
            services.AddSingleton<IAzureTokenCredentialProvider, PlaybackTokenCredentialProvider>();
        }
        services.AddSingleton<ISubscriptionResolver, SubscriptionResolver>();
        services.AddAzureService();
        BaseAzureService.InitializeUserAgentPolicy("stdio");
    }
}
