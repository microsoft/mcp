// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.InfraIq.Commands.VmSku;
using Azure.Mcp.Tools.InfraIq.Configuration;
using Azure.Mcp.Tools.InfraIq.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Azure.Mcp.Tools.InfraIq.Tests.Setup;

public class InfraIqSetupOptionsTests
{
    private static ServiceProvider BuildProvider(string? origin)
    {
        var values = new Dictionary<string, string?>();
        if (origin is not null)
        {
            values["InfraIq:ArmIngressOrigin"] = origin;
        }

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().AddInMemoryCollection(values).Build());
        new InfraIqSetup().ConfigureServices(services);
        return services.BuildServiceProvider();
    }

    [Fact]
    public void ConfigureServices_BindsArmIngressOriginFromConfiguration()
    {
        using var provider = BuildProvider("https://eastus2euap.management.azure.com");

        Assert.Equal(
            "https://eastus2euap.management.azure.com",
            provider.GetRequiredService<IOptions<InfraIqOptions>>().Value.ArmIngressOrigin);
    }

    [Fact]
    public void ConfigureServices_RegistersInfraIqOptionsValidator()
    {
        using var provider = BuildProvider(null);

        Assert.Contains(
            provider.GetServices<IValidateOptions<InfraIqOptions>>(),
            validator => validator is InfraIqOptionsValidator);
    }

    [Fact]
    public void StartupValidation_Succeeds_ForApprovedLocalOrigin()
    {
        using var provider = BuildProvider("https://eastus2euap.management.azure.com");

        provider.GetRequiredService<IStartupValidator>().Validate();
    }

    [Fact]
    public void StartupValidation_Succeeds_WhenOriginMissing_AndDefaultsToApprovedOrigin()
    {
        using var provider = BuildProvider(null);

        provider.GetRequiredService<IStartupValidator>().Validate();
        Assert.Equal(
            InfraIqArmIngress.DevelopmentOrigin,
            provider.GetRequiredService<IOptions<InfraIqOptions>>().Value.ArmIngressOrigin);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ConfigureServices_DefaultsBlankOriginToApprovedOrigin(string blank)
    {
        using var provider = BuildProvider(blank);

        provider.GetRequiredService<IStartupValidator>().Validate();
        Assert.Equal(
            "https://eastus2euap.management.azure.com",
            provider.GetRequiredService<IOptions<InfraIqOptions>>().Value.ArmIngressOrigin);
    }

    [Fact]
    public void ConfigureServices_BindsOriginFromEnvironmentVariableKey()
    {
        const string prefix = "INFRAIQ_UNIT_TEST_";
        const string variable = prefix + "InfraIq__ArmIngressOrigin";
        Environment.SetEnvironmentVariable(variable, "https://eastus2euap.management.azure.com/");
        try
        {
            var configuration = new ConfigurationBuilder().AddEnvironmentVariables(prefix).Build();
            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(configuration);
            new InfraIqSetup().ConfigureServices(services);
            using var provider = services.BuildServiceProvider();

            Assert.Equal(
                "https://eastus2euap.management.azure.com/",
                provider.GetRequiredService<IOptions<InfraIqOptions>>().Value.ArmIngressOrigin);
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
        new InfraIqSetup().ConfigureServices(services);

        Assert.Contains(services, d => d.ServiceType == typeof(IInfraIqService) && d.ImplementationType == typeof(InfraIqService));
        Assert.Contains(services, d => d.ServiceType == typeof(IInfraIqArmClient) && d.ImplementationType == typeof(InfraIqArmClient));
        Assert.Contains(services, d => d.ServiceType == typeof(VmSkuRecommendCommand));
    }

    [Fact]
    public void NamedHttpClient_UsesTimeoutAboveServiceDeadline()
    {
        using var provider = BuildProvider(null);

        var client = provider.GetRequiredService<IHttpClientFactory>().CreateClient(InfraIqArmClient.HttpClientName);

        Assert.True(client.Timeout > TimeSpan.FromSeconds(55));
    }

    [Fact]
    public void CreateNoRedirectHandler_DisablesAutomaticRedirects()
    {
        using var handler = Assert.IsType<HttpClientHandler>(InfraIqSetup.CreateNoRedirectHandler());

        Assert.False(handler.AllowAutoRedirect);
    }

    [Fact]
    public async Task NoRedirectHandler_DoesNotFollowRedirectsOrForwardAuthorization()
    {
        using var listener = new HttpListener();
        var port = GetFreePort();
        listener.Prefixes.Add($"http://127.0.0.1:{port}/");
        listener.Start();

        var observed = new List<(string Path, string? Authorization)>();
        var serverTask = Task.Run(async () =>
        {
            while (listener.IsListening)
            {
                HttpListenerContext context;
                try
                {
                    context = await listener.GetContextAsync();
                }
                catch (Exception ex) when (ex is HttpListenerException or ObjectDisposedException)
                {
                    return;
                }

                lock (observed)
                {
                    observed.Add((context.Request.Url!.AbsolutePath, context.Request.Headers["Authorization"]));
                }

                context.Response.StatusCode = 302;
                context.Response.RedirectLocation = $"http://127.0.0.1:{port}/redirected";
                context.Response.Close();
            }
        }, TestContext.Current.CancellationToken);

        using var client = new HttpClient(InfraIqSetup.CreateNoRedirectHandler());
        using var request = new HttpRequestMessage(HttpMethod.Post, $"http://127.0.0.1:{port}/start");
        request.Headers.Authorization = new("Bearer", "secret-token");

        using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);
        listener.Stop();
        await serverTask;

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        var single = Assert.Single(observed);
        Assert.Equal("/start", single.Path);
    }

    private static int GetFreePort()
    {
        var tcpListener = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
        tcpListener.Start();
        var port = ((IPEndPoint)tcpListener.LocalEndpoint).Port;
        tcpListener.Stop();
        return port;
    }
}
