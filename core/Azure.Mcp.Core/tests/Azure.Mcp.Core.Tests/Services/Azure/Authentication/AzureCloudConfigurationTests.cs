// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.ResourceManager;
using Microsoft.Extensions.Configuration;
using Microsoft.Mcp.Core.Areas.Server;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using Xunit;

namespace Azure.Mcp.Core.Tests.Services.Azure.Authentication;

/// <summary>
/// Tests for AzureCloudConfiguration to verify sovereign cloud support.
/// These tests verify that cloud names and custom URLs are correctly parsed to authority hosts and ARM environments.
/// </summary>
public class AzureCloudConfigurationTests
{
    /// <summary>
    /// Tests that well-known cloud names are correctly mapped to their authority hosts.
    /// </summary>
    [Theory]
    [InlineData("AzureCloud", "https://login.microsoftonline.com")]
    [InlineData("AzurePublicCloud", "https://login.microsoftonline.com")]
    [InlineData("public", "https://login.microsoftonline.com")]
    [InlineData("AzureChinaCloud", "https://login.chinacloudapi.cn")]
    [InlineData("china", "https://login.chinacloudapi.cn")]
    [InlineData("AzureUSGovernment", "https://login.microsoftonline.us")]
    [InlineData("AzureUSGovernmentCloud", "https://login.microsoftonline.us")]
    [InlineData("usgov", "https://login.microsoftonline.us")]
    [InlineData("usgovernment", "https://login.microsoftonline.us")]
    public void ParseCloudValue_WellKnownClouds_ReturnsCorrectAuthorityHost(string cloudName, string expectedHost)
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["cloud"] = cloudName })
            .Build();

        // Act
        var cloudConfig = new AzureCloudConfiguration(config);

        // Assert
        Assert.Equal(new Uri(expectedHost), cloudConfig.AuthorityHost);
    }

    /// <summary>
    /// Tests that well-known cloud names are correctly mapped to their ARM environments.
    /// </summary>
    [Theory]
    [InlineData("AzureCloud")]
    [InlineData("AzurePublicCloud")]
    [InlineData("public")]
    public void ParseCloudValue_PublicCloud_ReturnsPublicArmEnvironment(string cloudName)
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["cloud"] = cloudName })
            .Build();

        // Act
        var cloudConfig = new AzureCloudConfiguration(config);

        // Assert
        Assert.Equal(ArmEnvironment.AzurePublicCloud, cloudConfig.ArmEnvironment);
    }

    /// <summary>
    /// Tests that US Government cloud names are correctly mapped to their ARM environment.
    /// </summary>
    [Theory]
    [InlineData("AzureUSGovernment")]
    [InlineData("AzureUSGovernmentCloud")]
    [InlineData("usgov")]
    [InlineData("usgovernment")]
    public void ParseCloudValue_USGovernmentCloud_ReturnsGovernmentArmEnvironment(string cloudName)
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["cloud"] = cloudName })
            .Build();

        // Act
        var cloudConfig = new AzureCloudConfiguration(config);

        // Assert
        Assert.Equal(ArmEnvironment.AzureGovernment, cloudConfig.ArmEnvironment);
    }

    /// <summary>
    /// Tests that China cloud names are correctly mapped to their ARM environment.
    /// </summary>
    [Theory]
    [InlineData("AzureChinaCloud")]
    [InlineData("china")]
    public void ParseCloudValue_ChinaCloud_ReturnsChinaArmEnvironment(string cloudName)
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["cloud"] = cloudName })
            .Build();

        // Act
        var cloudConfig = new AzureCloudConfiguration(config);

        // Assert
        Assert.Equal(ArmEnvironment.AzureChina, cloudConfig.ArmEnvironment);
    }

    /// <summary>
    /// Tests that when no cloud configuration is provided, the default public cloud is used.
    /// </summary>
    [Fact]
    public void ParseCloudValue_NoConfiguration_ReturnsDefaultPublicCloud()
    {
        // Arrange
        var config = new ConfigurationBuilder().Build();

        // Act
        var cloudConfig = new AzureCloudConfiguration(config);

        // Assert
        Assert.Equal(new Uri("https://login.microsoftonline.com"), cloudConfig.AuthorityHost);
        Assert.Equal(ArmEnvironment.AzurePublicCloud, cloudConfig.ArmEnvironment);
    }

    /// <summary>
    /// Tests that ServerRuntimeConfiguration (command-line arguments) take priority over appsettings.json configuration.
    /// </summary>
    [Fact]
    public void ConfigurationPriority_CommandLineOverridesAppsettings()
    {
        // Arrange - ServerRuntimeConfiguration takes priority
        var options = Microsoft.Extensions.Options.Options.Create(new ServerRuntimeConfiguration { Cloud = "AzureChinaCloud" });
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["cloud"] = "AzureUSGovernment" })
            .Build();

        // Act
        var cloudConfig = new AzureCloudConfiguration(config, options);

        // Assert
        Assert.Equal(new Uri("https://login.chinacloudapi.cn"), cloudConfig.AuthorityHost);
    }

    /// <summary>
    /// Tests that appsettings.json configuration is used when ServerRuntimeConfiguration is not set.
    /// </summary>
    [Fact]
    public void ConfigurationPriority_AppsettingsUsedWhenNoCommandLine()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["cloud"] = "AzureUSGovernment" })
            .Build();

        // Act
        var cloudConfig = new AzureCloudConfiguration(config);

        // Assert
        Assert.Equal(new Uri("https://login.microsoftonline.us"), cloudConfig.AuthorityHost);
    }

    /// <summary>
    /// Tests that environment variable AZURE_CLOUD is read when other configurations are not set.
    /// </summary>
    [Fact]
    public void ConfigurationPriority_EnvironmentVariableUsedWhenNoOtherConfig()
    {
        // Arrange
        Environment.SetEnvironmentVariable("AZURE_CLOUD", "AzureChinaCloud");
        try
        {
            var config = new ConfigurationBuilder().Build();

            // Act
            var cloudConfig = new AzureCloudConfiguration(config);

            // Assert
            Assert.Equal(new Uri("https://login.chinacloudapi.cn"), cloudConfig.AuthorityHost);
        }
        finally
        {
            Environment.SetEnvironmentVariable("AZURE_CLOUD", null);
        }
    }

    /// <summary>
    /// Tests that cloud name parsing is case-insensitive.
    /// </summary>
    [Theory]
    [InlineData("AZURECHINACLOUD")]
    [InlineData("azurechinacloud")]
    [InlineData("AzUrEcHiNaClOuD")]
    [InlineData("CHINA")]
    [InlineData("China")]
    public void ParseCloudValue_CaseInsensitive_ReturnsCorrectAuthorityHost(string cloudName)
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["cloud"] = cloudName })
            .Build();

        // Act
        var cloudConfig = new AzureCloudConfiguration(config);

        // Assert
        Assert.Equal(new Uri("https://login.chinacloudapi.cn"), cloudConfig.AuthorityHost);
    }

    /// <summary>
    /// Tests that unknown cloud names throw an ArgumentException.
    /// </summary>
    [Theory]
    [InlineData("UnknownCloud")]
    [InlineData("InvalidCloudName")]
    [InlineData("https://custom.authority.host")]
    [InlineData("http://custom.authority.host")]
    public void ParseCloudValue_UnknownCloudNames_ThrowsArgumentException(string cloudName)
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["cloud"] = cloudName })
            .Build();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new AzureCloudConfiguration(config));
    }

    /// <summary>
    /// Tests that null or whitespace cloud values default to Azure Public Cloud.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ParseCloudValue_NullOrWhitespace_DefaultsToPublicCloud(string? cloudName)
    {
        // Arrange
        var configData = cloudName != null
            ? new Dictionary<string, string?> { ["cloud"] = cloudName }
            : new Dictionary<string, string?>();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();

        // Act
        var cloudConfig = new AzureCloudConfiguration(config);

        // Assert
        Assert.Equal(new Uri("https://login.microsoftonline.com"), cloudConfig.AuthorityHost);
    }

    /// <summary>
    /// Tests that configuration can read from "Cloud" key (capital C) in addition to "cloud".
    /// </summary>
    [Fact]
    public void Configuration_SupportsCapitalCloudKey()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Cloud"] = "AzureChinaCloud" })
            .Build();

        // Act
        var cloudConfig = new AzureCloudConfiguration(config);

        // Assert
        Assert.Equal(new Uri("https://login.chinacloudapi.cn"), cloudConfig.AuthorityHost);
    }

    /// <summary>
    /// Tests that configuration reads from AZURE_CLOUD environment variable key.
    /// </summary>
    [Fact]
    public void Configuration_SupportsAzureCloudKey()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["AZURE_CLOUD"] = "AzureUSGovernment" })
            .Build();

        // Act
        var cloudConfig = new AzureCloudConfiguration(config);

        // Assert
        Assert.Equal(new Uri("https://login.microsoftonline.us"), cloudConfig.AuthorityHost);
    }

    /// <summary>
    /// Tests complete priority chain: ServerRuntimeConfiguration > cloud config > AZURE_CLOUD env var > default
    /// </summary>
    [Fact]
    public void ConfigurationPriority_FullPriorityChain()
    {
        // Arrange - Set up multiple configuration sources
        var options = Microsoft.Extensions.Options.Options.Create(new ServerRuntimeConfiguration { Cloud = "AzureChinaCloud" });
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["cloud"] = "AzureUSGovernment",
                ["AZURE_CLOUD"] = "AzureGermanyCloud"
            })
            .Build();

        // Act
        var cloudConfig = new AzureCloudConfiguration(config, options);

        // Assert - Should use ServerRuntimeConfiguration (highest priority)
        Assert.Equal(new Uri("https://login.chinacloudapi.cn"), cloudConfig.AuthorityHost);
    }

    /// <summary>
    /// Tests that when ServerRuntimeConfiguration is null, configuration falls back to appsettings.
    /// </summary>
    [Fact]
    public void ConfigurationPriority_NullServerRuntimeConfiguration_FallsBackToConfig()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["cloud"] = "AzureChinaCloud" })
            .Build();

        // Act
        var cloudConfig = new AzureCloudConfiguration(config, runtimeConfiguration: null);

        // Assert
        Assert.Equal(new Uri("https://login.chinacloudapi.cn"), cloudConfig.AuthorityHost);
    }

    /// <summary>
    /// Tests that public cloud ARM environment has the correct management endpoint.
    /// </summary>
    [Fact]
    public void ArmEnvironment_PublicCloud_HasCorrectEndpoint()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["cloud"] = "AzureCloud" })
            .Build();

        // Act
        var cloudConfig = new AzureCloudConfiguration(config);

        // Assert
        Assert.Equal(new Uri("https://management.azure.com"), cloudConfig.ArmEnvironment.Endpoint);
    }

    /// <summary>
    /// Tests that China cloud ARM environment has the correct management endpoint.
    /// </summary>
    [Fact]
    public void ArmEnvironment_ChinaCloud_HasCorrectEndpoint()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["cloud"] = "AzureChinaCloud" })
            .Build();

        // Act
        var cloudConfig = new AzureCloudConfiguration(config);

        // Assert
        Assert.Equal(new Uri("https://management.chinacloudapi.cn"), cloudConfig.ArmEnvironment.Endpoint);
    }

    /// <summary>
    /// Tests that US Government cloud ARM environment has the correct management endpoint.
    /// </summary>
    [Fact]
    public void ArmEnvironment_GovernmentCloud_HasCorrectEndpoint()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["cloud"] = "AzureUSGovernment" })
            .Build();

        // Act
        var cloudConfig = new AzureCloudConfiguration(config);

        // Assert
        Assert.Equal(new Uri("https://management.usgovcloudapi.net"), cloudConfig.ArmEnvironment.Endpoint);
    }

    [Fact]
    public void ParseCloudValue_CustomCloud_LoadsConfiguredMetadata()
    {
        var path = Path.GetTempFileName();
        try
        {
            File.WriteAllText(path, """
                {
                  "authorityHost": "https://login.custom.example",
                  "armEndpoint": "https://management.custom.example",
                  "resourceManagerAudience": "https://management.custom.example/",
                  "logAnalyticsEndpoint": "https://logs.custom.example",
                  "logAnalyticsScope": "https://logs.custom.example/.default",
                  "applicationInsightsEndpoint": "https://insights.custom.example"
                }
                """);
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?> { ["cloud"] = "custom" })
                .Build();
            var options = Microsoft.Extensions.Options.Options.Create(new ServerRuntimeConfiguration { CustomCloudConfig = path });

            var cloudConfig = new AzureCloudConfiguration(config, options);

            Assert.Equal(AzureCloudConfiguration.AzureCloud.CustomCloud, cloudConfig.CloudType);
            Assert.Equal(new Uri("https://management.custom.example"), cloudConfig.ArmEnvironment.Endpoint);
            Assert.Equal(new Uri("https://logs.custom.example"), cloudConfig.LogAnalyticsEndpoint);
            Assert.Equal("https://logs.custom.example/.default", cloudConfig.LogAnalyticsScope);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void ParseCloudValue_CustomCloudWithoutConfig_ThrowsArgumentException()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["cloud"] = "custom" })
            .Build();

        Assert.Throws<ArgumentException>(() => new AzureCloudConfiguration(config));
    }

    [Fact]
    public void ParseCloudValue_CustomCloudWithoutLogAnalyticsScope_ThrowsArgumentException()
    {
        var path = Path.GetTempFileName();
        try
        {
            File.WriteAllText(path, """
                {
                  "authorityHost": "https://login.custom.example",
                  "armEndpoint": "https://management.custom.example",
                  "resourceManagerAudience": "https://management.custom.example/",
                  "logAnalyticsEndpoint": "https://logs.custom.example",
                  "applicationInsightsEndpoint": "https://insights.custom.example"
                }
                """);
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?> { ["cloud"] = "custom" })
                .Build();
            var options = Microsoft.Extensions.Options.Options.Create(new ServerRuntimeConfiguration { CustomCloudConfig = path });

            var exception = Assert.Throws<ArgumentException>(() => new AzureCloudConfiguration(config, options));

            Assert.Contains("logAnalyticsScope", exception.Message);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
