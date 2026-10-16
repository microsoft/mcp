// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AIInfraOptimization.Commands.VmSku;
using Azure.Mcp.Tools.AIInfraOptimization.Configuration;
using Azure.Mcp.Tools.AIInfraOptimization.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;
using Xunit;

namespace Azure.Mcp.Tools.AIInfraOptimization.Tests.Setup;

public class AIInfraOptimizationSetupOptionsTests
{
    private static ServiceProvider BuildProvider(string? origin)
    {
        var values = new Dictionary<string, string?>();
        if (origin is not null)
        {
            values["AIInfraOptimization:ArmIngressOrigin"] = origin;
        }

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().AddInMemoryCollection(values).Build());
        new AIInfraOptimizationSetup().ConfigureServices(services);
        return services.BuildServiceProvider();
    }

    [Fact]
    public void ConfigureServices_BindsArmIngressOriginFromConfiguration()
    {
        using var provider = BuildProvider("https://eastus2euap.management.azure.com");

        Assert.Equal(
            "https://eastus2euap.management.azure.com",
            provider.GetRequiredService<IOptions<AIInfraOptimizationOptions>>().Value.ArmIngressOrigin);
    }

    [Fact]
    public void ConfigureServices_RegistersAIInfraOptimizationOptionsValidator()
    {
        using var provider = BuildProvider(null);

        Assert.Contains(
            provider.GetServices<IValidateOptions<AIInfraOptimizationOptions>>(),
            validator => validator is AIInfraOptimizationOptionsValidator);
    }

    [Theory]
    [InlineData("https://management.azure.com")]
    [InlineData("https://eastus2euap.management.azure.com")]
    public void StartupValidation_Succeeds_ForApprovedOrigin(string origin)
    {
        using var provider = BuildProvider(origin);

        provider.GetRequiredService<IStartupValidator>().Validate();
    }

    [Fact]
    public void StartupValidation_Succeeds_WhenOriginMissing_AndSelectsAutomaticRouting()
    {
        using var provider = BuildProvider(null);

        provider.GetRequiredService<IStartupValidator>().Validate();
        Assert.Null(provider.GetRequiredService<IOptions<AIInfraOptimizationOptions>>().Value.ArmIngressOrigin);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ConfigureServices_MapsBlankOriginToAutomaticRouting(string blank)
    {
        using var provider = BuildProvider(blank);

        provider.GetRequiredService<IStartupValidator>().Validate();
        Assert.Null(provider.GetRequiredService<IOptions<AIInfraOptimizationOptions>>().Value.ArmIngressOrigin);
    }

    [Fact]
    public void ConfigureServices_BindsOriginFromEnvironmentVariableKey()
    {
        const string prefix = "AIINFRAOPTIMIZATION_UNIT_TEST_";
        const string variable = prefix + "AIInfraOptimization__ArmIngressOrigin";
        Environment.SetEnvironmentVariable(variable, "https://eastus2euap.management.azure.com/");
        try
        {
            var configuration = new ConfigurationBuilder().AddEnvironmentVariables(prefix).Build();
            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(configuration);
            new AIInfraOptimizationSetup().ConfigureServices(services);
            using var provider = services.BuildServiceProvider();

            Assert.Equal(
                "https://eastus2euap.management.azure.com/",
                provider.GetRequiredService<IOptions<AIInfraOptimizationOptions>>().Value.ArmIngressOrigin);
        }
        finally
        {
            Environment.SetEnvironmentVariable(variable, null);
        }
    }

    [Theory]
    [InlineData("http://eastus2euap.management.azure.com")]
    [InlineData("https://evil.example.com")]
    [InlineData("https://eastus2euap.management.azure.com/path")]
    [InlineData(" https://eastus2euap.management.azure.com")]
    public void StartupValidation_Fails_WhenConfiguredOriginInvalid(string origin)
    {
        using var provider = BuildProvider(origin);

        Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IStartupValidator>().Validate());
    }

    [Fact]
    public void ConfigureServices_RegistersServicesAndCommand()
    {
        var services = new ServiceCollection();
        new AIInfraOptimizationSetup().ConfigureServices(services);

        Assert.Contains(services, d => d.ServiceType == typeof(IAIInfraOptimizationService) && d.ImplementationType == typeof(AIInfraOptimizationService));
        Assert.Contains(services, d => d.ServiceType == typeof(IAIInfraOptimizationArmClient) && d.ImplementationType == typeof(AIInfraOptimizationArmClient));
        Assert.Contains(services, d => d.ServiceType == typeof(VmSkuRecommendCommand));
    }

    [Fact]
    public void NamedHttpClient_UsesTimeoutAboveServiceDeadline()
    {
        using var provider = BuildProvider(null);

        var client = provider.GetRequiredService<IHttpClientFactory>().CreateClient(AIInfraOptimizationArmClient.HttpClientName);

        Assert.True(client.Timeout > TimeSpan.FromSeconds(55));
    }

    [Fact]
    public void NamedHttpClient_DoesNotReplaceCorePrimaryHandler()
    {
        // Core's default handler chain supplies AntiSSRF, the recording proxy, and disabled redirects;
        // a tool-level primary handler would silently replace that chain.
        using var provider = BuildProvider(null);

        var options = provider.GetRequiredService<IOptionsMonitor<HttpClientFactoryOptions>>()
            .Get(AIInfraOptimizationArmClient.HttpClientName);

        Assert.Empty(options.HttpMessageHandlerBuilderActions);
    }
}
