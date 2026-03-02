// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Services.Azure.Authentication;

/// <summary>
/// Tests for <see cref="AuthenticationServiceCollectionExtensions"/>.
/// </summary>
public class AuthenticationServiceCollectionExtensionsTests
{
    // -------------------------------------------------------------------------
    // AddHttpOnBehalfOfTokenCredentialProvider
    // -------------------------------------------------------------------------

    [Fact]
    public void AddHttpOnBehalfOfTokenCredentialProvider_RegistersIAzureCloudConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.AddSingleton<Microsoft.Extensions.Options.IOptions<Microsoft.Mcp.Core.Areas.Server.Options.ServiceStartOptions>>(
            Microsoft.Extensions.Options.Options.Create(new Microsoft.Mcp.Core.Areas.Server.Options.ServiceStartOptions()));
        services.AddLogging();

        // Act
        services.AddHttpOnBehalfOfTokenCredentialProvider();

        // Assert
        var descriptor = services.FirstOrDefault(
            d => d.ServiceType == typeof(IAzureCloudConfiguration));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
    }

    // -------------------------------------------------------------------------
    // AddSovereignCloudMicrosoftIdentityOptions — sovereign cloud auto-mapping
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("AzureChinaCloud", "https://login.chinacloudapi.cn/")]
    [InlineData("china", "https://login.chinacloudapi.cn/")]
    [InlineData("AzureUSGovernment", "https://login.microsoftonline.us/")]
    [InlineData("usgov", "https://login.microsoftonline.us/")]
    [InlineData("AzureCloud", "https://login.microsoftonline.com/")]
    [InlineData(null, "https://login.microsoftonline.com/")] // default (no cloud configured)
    public void AddSovereignCloudMicrosoftIdentityOptions_WhenInstanceNotConfigured_PostConfigure_SetsInstanceFromCloud(
        string? cloudName, string expectedInstance)
    {
        // Arrange – AzureAd section has no "Instance" key
        var configValues = new Dictionary<string, string?>();
        if (cloudName is not null)
        {
            configValues["cloud"] = cloudName;
        }

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues)
            .Build();

        var azureAdSection = config.GetSection("AzureAd"); // section exists but has no "Instance"

        var cloudConfig = new AzureCloudConfiguration(config);

        var services = new ServiceCollection();
        services.AddSingleton<IAzureCloudConfiguration>(cloudConfig);
        services.AddSovereignCloudMicrosoftIdentityOptions(azureAdSection, "Bearer");

        var sp = services.BuildServiceProvider();

        // Act – resolve and apply the PostConfigure
        var postConfigures = sp.GetServices<IPostConfigureOptions<MicrosoftIdentityApplicationOptions>>().ToList();
        Assert.Single(postConfigures);

        var options = new MicrosoftIdentityApplicationOptions();
        postConfigures[0].PostConfigure("Bearer", options);

        // Assert
        Assert.Equal(expectedInstance, options.Instance);
    }

    [Fact]
    public void AddSovereignCloudMicrosoftIdentityOptions_WhenInstanceExplicitlyConfigured_DoesNotRegisterPostConfigure()
    {
        // Arrange – AzureAd:Instance is explicitly set in config
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["cloud"] = "AzureChinaCloud",
                ["AzureAd:Instance"] = "https://login.custom.example.com/"
            })
            .Build();

        var azureAdSection = config.GetSection("AzureAd");
        var cloudConfig = new AzureCloudConfiguration(config);

        var services = new ServiceCollection();
        services.AddSingleton<IAzureCloudConfiguration>(cloudConfig);
        services.AddSovereignCloudMicrosoftIdentityOptions(azureAdSection, "Bearer");

        var sp = services.BuildServiceProvider();

        // Act
        var postConfigures = sp.GetServices<IPostConfigureOptions<MicrosoftIdentityApplicationOptions>>().ToList();

        // Assert – no PostConfigure registered because Instance was explicit
        Assert.Empty(postConfigures);
    }

    [Fact]
    public void AddSovereignCloudMicrosoftIdentityOptions_PostConfigure_OnlyAppliesToMatchingScheme()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["cloud"] = "AzureChinaCloud" })
            .Build();

        var azureAdSection = config.GetSection("AzureAd");
        var cloudConfig = new AzureCloudConfiguration(config);

        var services = new ServiceCollection();
        services.AddSingleton<IAzureCloudConfiguration>(cloudConfig);
        services.AddSovereignCloudMicrosoftIdentityOptions(azureAdSection, "Bearer");

        var sp = services.BuildServiceProvider();
        var postConfigure = sp.GetRequiredService<IPostConfigureOptions<MicrosoftIdentityApplicationOptions>>();

        // Act – apply PostConfigure with a different scheme name
        var options = new MicrosoftIdentityApplicationOptions();
        postConfigure.PostConfigure("SomeOtherScheme", options);

        // Assert – Instance should NOT be modified because scheme doesn't match
        Assert.Null(options.Instance);
    }
}
