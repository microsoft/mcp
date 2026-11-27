// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using Azure.Core;
using Azure.Identity;
using Fabric.Mcp.Tools.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Areas.Server;
using Microsoft.Mcp.Core.Areas.Server.Options;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using NSubstitute;
using Xunit;

namespace Fabric.Mcp.Server.Tests.Infrastructure;

public sealed class ItemMoveAuthenticationTests()
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public static async Task CoreOnlyMove_UsesTheFullHostsPerRequestOboCredentialWithoutFallback(bool denyToken)
    {
        const string workspace = "aaaaaaaa-0000-1111-2222-bbbbbbbbbbbb";
        var callerItems = Enumerable.Range(0, 8).ToDictionary(index => $"caller-{index}", _ => Guid.NewGuid());
        var tokenRequests = new ConcurrentBag<string>();
        var connections = new ConcurrentBag<string>();
        var services = new ServiceCollection();
        services.AddLogging();
        // Match ServerStartCommand: install the outgoing provider before the actual host callback.
        services.AddHttpOnBehalfOfTokenCredentialProvider();
        ConfigureActualHost(services);
        services.Configure<ServerRuntimeConfiguration>(options =>
        {
            options.Namespace = ["core"];
            options.Transport = TransportTypes.Http;
        });
        var scheme = Substitute.For<Microsoft.Identity.Abstractions.IAuthenticationSchemeInformationProvider>();
        scheme.GetEffectiveAuthenticationScheme(Arg.Any<string>()).Returns("Bearer");
        services.AddSingleton(scheme);
        services.AddScoped<ITokenAcquirerFactory>(provider =>
        {
            var caller = provider.GetRequiredService<IHttpContextAccessor>().HttpContext!.User.FindFirst("sub")!.Value;
            var acquirer = Substitute.For<ITokenAcquirer>();
            acquirer.GetTokenForUserAsync(
                Arg.Any<IEnumerable<string>>(), Arg.Any<AcquireTokenOptions>(),
                Arg.Any<ClaimsPrincipal>(), Arg.Any<CancellationToken>()).Returns(call =>
                {
                    Assert.Equal(["https://api.fabric.microsoft.com/.default"], call.Arg<IEnumerable<string>>());
                    Assert.Equal(TestContext.Current.CancellationToken, call.Arg<CancellationToken>());
                    tokenRequests.Add(caller);
                    return denyToken
                        ? Task.FromException<AcquireTokenResult>(new AuthenticationFailedException("private-" + caller))
                        : Task.FromResult(new AcquireTokenResult(
                            caller, DateTimeOffset.UtcNow.AddHours(1), workspace, "",
                            ["https://api.fabric.microsoft.com/.default"], Guid.Empty, "Bearer"));
                });
            acquirer.GetTokenForAppAsync(Arg.Any<string>(), Arg.Any<AcquireTokenOptions>(), Arg.Any<CancellationToken>())
                .Returns<Task<AcquireTokenResult>>(_ => throw new InvalidOperationException("App identity must not be used."));
            var factory = Substitute.For<ITokenAcquirerFactory>();
            factory.GetTokenAcquirer(Arg.Any<string>()).Returns(acquirer);
            return factory;
        });
        services.ConfigureHttpClientDefaults(builder => builder.ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
        {
            UseProxy = false,
            ConnectCallback = async (context, _) =>
            {
                await Task.Yield();
                var request = context.InitialRequestMessage;
                var caller = Assert.IsType<string>(request.Headers.Authorization?.Parameter);
                Assert.Equal(HttpMethod.Post, request.Method);
                Assert.Equal($"https://api.fabric.microsoft.com/v1/workspaces/{workspace}/items/{callerItems[caller]:D}/move",
                    request.RequestUri?.AbsoluteUri);
                connections.Add(caller);
                // Intercept before DNS, TCP, or TLS: this test must never contact Fabric.
                throw new IOException("Offline transport interception.");
            }
        }));
        await using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
        Assert.IsType<HttpOnBehalfOfTokenCredentialProvider>(provider.GetRequiredService<IAzureTokenCredentialProvider>());
        Assert.Contains(provider.GetServices<IAreaSetup>(), area => area.Name == "datafactory");
        Assert.Equal("Fabric.Mcp.Tools.DataFactory.ProviderDelegatingCredential",
            provider.GetRequiredService<TokenCredential>().GetType().FullName);
        var namespaces = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<ServerRuntimeConfiguration>>().Value.Namespace;
        Assert.NotNull(namespaces);
        Assert.Equal(["core"], namespaces);
        var accessor = provider.GetRequiredService<IHttpContextAccessor>();
        var service = provider.GetRequiredService<IFabricCoreService>();

        await Task.WhenAll(callerItems.Select(async caller =>
        {
            await using var scope = provider.CreateAsyncScope();
            accessor.HttpContext = new DefaultHttpContext
            {
                RequestServices = scope.ServiceProvider,
                User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", caller.Key)], "offline-test"))
            };
            try
            {
                Assert.IsType<MicrosoftIdentityTokenCredential>(
                    await provider.GetRequiredService<IAzureTokenCredentialProvider>()
                        .GetTokenCredentialAsync(null, TestContext.Current.CancellationToken));
                var error = await Assert.ThrowsAnyAsync<HttpRequestException>(() =>
                    service.MoveItemAsync(workspace, caller.Value.ToString(), cancellationToken: TestContext.Current.CancellationToken));
                Assert.Equal(denyToken ? HttpStatusCode.Unauthorized : HttpStatusCode.ServiceUnavailable, error.StatusCode);
                Assert.DoesNotContain("private", error.Message);
            }
            finally
            {
                accessor.HttpContext = null;
            }
        }));

        Assert.Equal(callerItems.Keys.Order(), tokenRequests.Order());
        Assert.Equal(denyToken ? [] : callerItems.Keys.Order().ToArray(), connections.Order());
    }

    private static void ConfigureActualHost(IServiceCollection services)
    {
        // Use the real callback, not a copied area list that could miss an authentication bridge.
        var program = Assembly.Load("fabmcp").GetType("Program", throwOnError: true)!;
        var configure = program.GetMethod("ConfigureServices", BindingFlags.Static | BindingFlags.NonPublic);
        Assert.NotNull(configure);
        configure.Invoke(null, [services]);
    }
}
