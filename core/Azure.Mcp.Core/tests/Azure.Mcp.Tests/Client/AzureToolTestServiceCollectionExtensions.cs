// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using Microsoft.Mcp.Tests.Helpers;

namespace Azure.Mcp.Tests.Client;

internal static class AzureToolTestServiceCollectionExtensions
{
    public static void AddAzureToolTestServices(this IServiceCollection services, TestMode mode)
    {
        if (mode == TestMode.Playback)
        {
            services.AddSingleton<IAzureTokenCredentialProvider, PlaybackTokenCredentialProvider>();
        }
        services.AddSingleton<ISubscriptionResolver, SubscriptionResolver>();
        services.AddAzureService();
        BaseAzureService.InitializeUserAgentPolicy("stdio");
    }
}
