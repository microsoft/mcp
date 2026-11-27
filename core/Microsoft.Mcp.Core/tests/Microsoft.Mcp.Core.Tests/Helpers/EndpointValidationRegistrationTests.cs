// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Security;
using Azure.ResourceManager;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Helpers;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using Xunit;

namespace Microsoft.Mcp.Core.Tests.Helpers;

public sealed class EndpointValidationRegistrationTests
{
    [Fact]
    public async Task IndependentProviders_KeepImmutablePoliciesDuringConcurrentValidation()
    {
        string[] namespaces = ["ACR"];
        SsrfProtectionPolicy selectedPolicy = new(namespaces);
        namespaces[0] = "storage";
        using ServiceProvider selected = new ServiceCollection()
            .AddSingleton(selectedPolicy).AddEndpointValidation().AddEndpointValidation().BuildServiceProvider();
        using ServiceProvider all = new ServiceCollection()
            .AddSingleton(new SsrfProtectionPolicy(["all"])).AddEndpointValidation().BuildServiceProvider();
        using ServiceProvider protectedProvider = new ServiceCollection().AddEndpointValidation().BuildServiceProvider();

        Assert.Same(selectedPolicy, selected.GetRequiredService<SsrfProtectionPolicy>());
        Assert.Same(selected.GetRequiredService<IEndpointValidator>(), selected.GetRequiredService<IEndpointValidator>());
        Assert.NotSame(selected.GetRequiredService<ICommandContextAccessor>(), all.GetRequiredService<ICommandContextAccessor>());

        await Task.WhenAll(
            VerifyAsync(selected, "acr", bypass: true),
            VerifyAsync(selected, "storage", bypass: false),
            VerifyAsync(all, "storage", bypass: true),
            VerifyAsync(all, null, bypass: false),
            VerifyAsync(all, "", bypass: false),
            VerifyAsync(all, " ", bypass: false),
            VerifyAsync(protectedProvider, "acr", bypass: false));

        foreach (ServiceProvider provider in new[] { selected, all, protectedProvider })
        {
            Assert.Null(provider.GetRequiredService<ICommandContextAccessor>().CurrentContext);
            Assert.Throws<SecurityException>(() => provider.GetRequiredService<IEndpointValidator>()
                .ValidateAzureServiceEndpoint("http://127.0.0.1", "acr", ArmEnvironment.AzurePublicCloud));
            Assert.Throws<SecurityException>(() => provider.GetRequiredService<IEndpointValidator>()
                .ValidatePublicTargetUrl("http://127.0.0.1"));
        }

        static async Task VerifyAsync(ServiceProvider provider, string? namespaceName, bool bypass)
        {
            ICommandContextAccessor accessor = provider.GetRequiredService<ICommandContextAccessor>();
            IEndpointValidator validator = provider.GetRequiredService<IEndpointValidator>();
            using IDisposable scope = accessor.BeginScope(new CommandContext { ToolNamespaceName = namespaceName });
            await Task.Yield();
            Assert.Equal(namespaceName, accessor.CurrentContext?.ToolNamespaceName);
            if (bypass)
            {
                validator.ValidateAzureServiceEndpoint("http://127.0.0.1", "acr", ArmEnvironment.AzurePublicCloud);
                validator.ValidatePublicTargetUrl("http://127.0.0.1");
            }
            else
            {
                Assert.Throws<SecurityException>(() =>
                    validator.ValidateAzureServiceEndpoint("http://127.0.0.1", "acr", ArmEnvironment.AzurePublicCloud));
                Assert.Throws<SecurityException>(() =>
                    validator.ValidatePublicTargetUrl("http://127.0.0.1"));
            }
        }
    }

    [Fact]
    public void RepeatedRegistration_PreservesSuppliedValidatorAndAccessor()
    {
        IEndpointValidator validator = Substitute.For<IEndpointValidator>();
        var accessor = new CommandContextAccessor();
        using ServiceProvider provider = new ServiceCollection()
            .AddSingleton(validator)
            .AddSingleton<ICommandContextAccessor>(accessor)
            .AddEndpointValidation()
            .AddEndpointValidation()
            .BuildServiceProvider();

        Assert.Same(validator, provider.GetRequiredService<IEndpointValidator>());
        Assert.Same(accessor, provider.GetRequiredService<ICommandContextAccessor>());
    }

    [Fact]
    public void DefaultValidator_UsesSuppliedAccessorAndRegisteredCommandNamespaceRatherThanServiceType()
    {
        var accessor = new CommandContextAccessor();
        using ServiceProvider provider = new ServiceCollection()
            .AddSingleton(new SsrfProtectionPolicy(["compute"]))
            .AddSingleton<ICommandContextAccessor>(accessor)
            .AddEndpointValidation()
            .AddEndpointValidation()
            .BuildServiceProvider();
        IEndpointValidator validator;

        using (accessor.BeginScope(new CommandContext { ToolNamespaceName = "compute" }))
        {
            validator = provider.GetRequiredService<IEndpointValidator>();
            validator.ValidateAzureServiceEndpoint("http://127.0.0.1", "storage-blob", ArmEnvironment.AzurePublicCloud);
            validator.ValidatePublicTargetUrl("http://127.0.0.1");
        }
        using (accessor.BeginScope(new CommandContext { ToolNamespaceName = "storage-blob" }))
        {
            Assert.Throws<SecurityException>(() =>
                validator.ValidateAzureServiceEndpoint("http://127.0.0.1", "compute", ArmEnvironment.AzurePublicCloud));
            Assert.Throws<SecurityException>(() => validator.ValidatePublicTargetUrl("http://127.0.0.1"));
        }
        using (accessor.BeginScope(new CommandContext { ToolNamespaceName = "compute" }))
        {
            validator.ValidateAzureServiceEndpoint("http://127.0.0.1", "storage-blob", ArmEnvironment.AzurePublicCloud);
        }

        Assert.Null(accessor.CurrentContext);
        Assert.Throws<SecurityException>(() =>
            validator.ValidateAzureServiceEndpoint("http://127.0.0.1", "storage-blob", ArmEnvironment.AzurePublicCloud));
        Assert.Throws<SecurityException>(() => validator.ValidatePublicTargetUrl("http://127.0.0.1"));
    }
}
