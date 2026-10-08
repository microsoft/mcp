// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas.Server.Commands;
using Microsoft.Mcp.Core.Areas.Server.Options;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Helpers;
using Microsoft.Mcp.Core.Services.Http;
using Microsoft.Mcp.Core.Services.Telemetry;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Core.Tests.Areas.Server;

/// <summary>
/// Verifies that startup activities report configuration without disclosing raw SSRF settings.
/// </summary>
public sealed class ServerStartSsrfTelemetryTests
{
    [Fact]
    public void ServerRegistration_UsesCopiedPolicyInsteadOfLaterOptionChanges()
    {
        string[] namespaces = ["acr"];
        var options = new ServerStartOptions { DangerouslyDisableSsrfProtectionsByNamespace = namespaces };
        var services = new ServiceCollection();
        services.AddAzureMcpServer(options);
        services.ConfigureDefaultHttpClient();
        namespaces[0] = "ALL";
        using ServiceProvider provider = services.BuildServiceProvider();
        SsrfProtectionPolicy policy = provider.GetRequiredService<SsrfProtectionPolicy>();
        Assert.False(policy.AreSsrfProtectionsEnabled("ACR"));
        Assert.True(policy.AreSsrfProtectionsEnabled("storage"));

        var activity = new Activity("startup");
        ITelemetryService telemetry = Substitute.For<ITelemetryService>();
        telemetry.StartActivity(ActivityName.ServerStarted).Returns(activity);
        ServerStartCommand.LogStartTelemetry(telemetry, options, provider);
        Assert.Equal("selected", activity.GetTagItem(TagName.SsrfNamespaceOverrideScope));
        Assert.Equal(1, activity.GetTagItem(TagName.SsrfNamespaceOverrideCount));
    }

    /// <summary>
    /// Provides override lists and their case-insensitive, non-blank configuration summaries.
    /// </summary>
    public static TheoryData<string[]?, string, int> NamespaceConfigurations => new()
    {
        { null, "none", 0 },
        { [], "none", 0 },
        { ["", " ", "\t"], "none", 0 },
        { ["Storage", "STORAGE", "keyvault", ""], "selected", 2 },
        { ["all", "ALL"], "all", 1 },
        { ["storage", "aLl", " ", "STORAGE"], "all", 2 },
        { [" ALL "], "selected", 1 }
    };

    [Theory]
    [MemberData(nameof(NamespaceConfigurations))]
    public void StartupActivity_ReportsConfiguredNamespaceScopeAndCount(
        string[]? overrides, string scope, int count)
    {
        var services = new ServiceCollection();
        services.AddSingleton(new SsrfProtectionPolicy(overrides));
        services.ConfigureDefaultHttpClient();
        using ServiceProvider provider = services.BuildServiceProvider();
        var activity = new Activity("startup");
        ITelemetryService telemetry = Substitute.For<ITelemetryService>();
        telemetry.StartActivity(ActivityName.ServerStarted).Returns(activity);
        var options = new ServerStartOptions { DangerouslyDisableSsrfProtectionsByNamespace = overrides };

        ServerStartCommand.LogStartTelemetry(telemetry, options, provider);

        Assert.Equal("external_only_latest", activity.GetTagItem(TagName.SsrfTransportMode));
        Assert.Equal(scope, activity.GetTagItem(TagName.SsrfNamespaceOverrideScope));
        Assert.Equal(count, activity.GetTagItem(TagName.SsrfNamespaceOverrideCount));
        Assert.DoesNotContain(activity.TagObjects, tag => tag.Key != TagName.SsrfNamespaceOverrideScope && tag.Value is string value &&
            overrides is not null && overrides.Contains(value));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void StartupActivity_SeparatesProxyAndNamespaceOverridesWithoutExportingSettings(bool all)
    {
        var services = new ServiceCollection();
        services.AddSingleton(new SsrfProtectionPolicy(all ? ["ALL"] : ["private-namespace"]));
        services.Configure<HttpClientOptions>(options =>
        {
            options.AllProxy = "http://private-user:private-password@proxy.example.invalid:9000";
            options.NoProxy = "*";
        });
        services.ConfigureDefaultHttpClient();
        using ServiceProvider provider = services.BuildServiceProvider();
        var activity = new Activity("startup");
        ITelemetryService telemetry = Substitute.For<ITelemetryService>();
        telemetry.StartActivity(ActivityName.ServerStarted).Returns(activity);
        var options = new ServerStartOptions
        {
            DangerouslyDisableSsrfProtectionsByNamespace = all ? ["ALL"] : ["private-namespace"]
        };

        ServerStartCommand.LogStartTelemetry(telemetry, options, provider);

        Assert.Equal("http_proxy_override", activity.GetTagItem(TagName.SsrfTransportMode));
        Assert.Equal(all ? "all" : "selected", activity.GetTagItem(TagName.SsrfNamespaceOverrideScope));
        Assert.Equal(1, activity.GetTagItem(TagName.SsrfNamespaceOverrideCount));
        string values = string.Join(",", activity.TagObjects.Select(tag => tag.Value));
        Assert.DoesNotContain("private-", values);
        Assert.DoesNotContain("proxy.example.invalid", values);
        Assert.DoesNotContain("*", values);
    }

    [Fact]
    public void StartupActivity_ReportsDeferredRecordingResolverWithoutInvokingIt()
    {
        var services = new ServiceCollection();
        services.ConfigureDefaultHttpClient(() => throw new InvalidOperationException("Must remain deferred."));
        using ServiceProvider provider = services.BuildServiceProvider();
        var activity = new Activity("startup");
        ITelemetryService telemetry = Substitute.For<ITelemetryService>();
        telemetry.StartActivity(ActivityName.ServerStarted).Returns(activity);

        ServerStartCommand.LogStartTelemetry(telemetry, new ServerStartOptions(), provider);

#if DEBUG
        Assert.Equal("deferred", activity.GetTagItem(TagName.SsrfTransportMode));
#else
        Assert.Equal("external_only_latest", activity.GetTagItem(TagName.SsrfTransportMode));
#endif
    }

    [Fact]
    public void StartupActivity_InvalidProxyReportsFailureWithoutChangingStartupBehavior()
    {
        var services = new ServiceCollection();
        services.Configure<HttpClientOptions>(options => options.AllProxy = "not-an-absolute-uri");
        services.ConfigureDefaultHttpClient();
        using ServiceProvider provider = services.BuildServiceProvider();
        var activity = new Activity("startup");
        ITelemetryService telemetry = Substitute.For<ITelemetryService>();
        telemetry.StartActivity(ActivityName.ServerStarted).Returns(activity);

        ServerStartCommand.LogStartTelemetry(telemetry, new ServerStartOptions(), provider);

        Assert.Equal("invalid_configuration", activity.GetTagItem(TagName.SsrfTransportMode));
        Assert.Equal("none", activity.GetTagItem(TagName.SsrfNamespaceOverrideScope));
        Assert.Equal(0, activity.GetTagItem(TagName.SsrfNamespaceOverrideCount));
        Assert.DoesNotContain(activity.TagObjects, tag => Equals(tag.Value, "not-an-absolute-uri"));
    }

    [Fact]
    public void StartupActivity_DisabledTelemetryDoesNotInspectTransportSettings()
    {
        ITelemetryService telemetry = Substitute.For<ITelemetryService>();
        IServiceProvider provider = Substitute.For<IServiceProvider>();
        provider.GetService(Arg.Any<Type>()).Returns(_ => throw new InvalidOperationException("Must not inspect settings."));

        ServerStartCommand.LogStartTelemetry(telemetry, new ServerStartOptions(), provider);

        telemetry.Received(1).StartActivity(ActivityName.ServerStarted);
        provider.DidNotReceive().GetService(Arg.Any<Type>());
    }
}
