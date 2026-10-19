// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas.Server.Commands;
using Microsoft.Mcp.Core.Areas.Server.Options;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Helpers;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Services.Telemetry;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Core.Tests.Areas.Server;

/// <summary>
/// Verifies that startup activities report namespace override configuration without
/// disclosing raw SSRF settings.
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
        namespaces[0] = "ALL";
        using ServiceProvider provider = services.BuildServiceProvider();
        SsrfProtectionPolicy policy = provider.GetRequiredService<SsrfProtectionPolicy>();
        ICommandContextAccessor contextAccessor = provider.GetRequiredService<ICommandContextAccessor>();
        Assert.False(AreSsrfProtectionsEnabled(policy, contextAccessor, "ACR"));
        Assert.True(AreSsrfProtectionsEnabled(policy, contextAccessor, "storage"));

        var activity = new Activity("startup");
        ITelemetryService telemetry = Substitute.For<ITelemetryService>();
        telemetry.StartActivity(ActivityName.ServerStarted).Returns(activity);
        ServerStartCommand.LogStartTelemetry(telemetry, options, policy);
        Assert.Equal("selected", activity.GetTagItem(TagName.SsrfNamespaceOverrideScope));
        Assert.Equal(1, activity.GetTagItem(TagName.SsrfNamespaceOverrideCount));
    }

    private static bool AreSsrfProtectionsEnabled(
        SsrfProtectionPolicy policy,
        ICommandContextAccessor contextAccessor,
        string executingToolNamespaceName)
    {
        using (contextAccessor.BeginScope(new CommandContext { ToolNamespaceName = executingToolNamespaceName }))
        {
            return policy.AreSsrfProtectionsEnabled(contextAccessor);
        }
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
        var policy = new SsrfProtectionPolicy(overrides);
        var activity = new Activity("startup");
        ITelemetryService telemetry = Substitute.For<ITelemetryService>();
        telemetry.StartActivity(ActivityName.ServerStarted).Returns(activity);
        var options = new ServerStartOptions { DangerouslyDisableSsrfProtectionsByNamespace = overrides };

        ServerStartCommand.LogStartTelemetry(telemetry, options, policy);

        Assert.Equal(scope, activity.GetTagItem(TagName.SsrfNamespaceOverrideScope));
        Assert.Equal(count, activity.GetTagItem(TagName.SsrfNamespaceOverrideCount));
        Assert.DoesNotContain(activity.TagObjects, tag => tag.Key != TagName.SsrfNamespaceOverrideScope &&
            tag.Value is string value && overrides is not null && overrides.Contains(value));
    }
}
