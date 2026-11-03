// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Azure.Mcp.Core.Areas.Subscription.Options;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Quota.Options.Usage;
using Azure.Mcp.Tools.Storage.Options.Account;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Options;
using Xunit;

namespace Azure.Mcp.Server.Tests.Infrastructure;

public sealed class TenantOptionContractTests
{
    [Fact]
    public void TenantOptions_ImplementITenantOption()
    {
        var assemblies = typeof(Program).Assembly.GetReferencedAssemblies()
            .Where(static name => name.Name?.StartsWith("Azure.Mcp.Tools.", StringComparison.Ordinal) == true)
            .Select(Assembly.Load)
            .Append(typeof(ITenantOption).Assembly);
        var tenantOptionTypes = assemblies
            .SelectMany(static assembly => assembly.GetExportedTypes())
            .Where(static type => type.IsClass &&
                type.GetProperty(nameof(ITenantOption.Tenant))?.GetCustomAttribute<OptionAttribute>() is not null)
            .ToArray();

        Assert.NotEmpty(tenantOptionTypes);
        Assert.All(tenantOptionTypes, static type =>
            Assert.True(typeof(ITenantOption).IsAssignableFrom(type),
                $"{type.FullName} exposes --tenant but does not implement {nameof(ITenantOption)}."));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("00000000-0000-0000-0000-000000000001")]
    [InlineData("Contoso Directory")]
    public void BindOptions_TenantOnly_PreservesTenant(string? tenant)
    {
        var command = CreateCommand<SubscriptionListOptions>();

        Assert.DoesNotContain(command.Options, static option => option.Name == "--subscription");
        Assert.False(typeof(ISubscriptionOption).IsAssignableFrom(typeof(SubscriptionListOptions)));
        BindTenantOptions<SubscriptionListOptions>(command, tenant);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("00000000-0000-0000-0000-000000000001")]
    [InlineData("Contoso Directory")]
    public void BindOptions_SubscriptionAndTenant_PreservesOptions(string? tenant)
    {
        var command = CreateCommand<AccountGetOptions>();
        var options = BindTenantOptions<AccountGetOptions>(command, tenant, "--subscription", "test-subscription");

        Assert.Equal("test-subscription", options.Subscription);
    }

    [Fact]
    public void SubscriptionOnlyOptions_DoNotExposeTenant()
    {
        var command = CreateCommand<CheckOptions>();

        Assert.DoesNotContain(command.Options, static option => option.Name == "--tenant");
        Assert.False(typeof(ITenantOption).IsAssignableFrom(typeof(CheckOptions)));
        Assert.False(typeof(ITenantOption).IsAssignableFrom(typeof(ISubscriptionOption)));
    }

    private static Command CreateCommand<
        [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions>()
        where TOptions : class
    {
        var command = new Command("test");
        OptionBinder.RegisterOptions<TOptions>(command);
        return command;
    }

    private static TOptions BindTenantOptions<
        [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions>(
        Command command,
        string? tenant,
        params string[] args)
        where TOptions : class, ITenantOption
    {
        var tenantOption = Assert.Single(command.Options, static option => option.Name == "--tenant");
        Assert.False(tenantOption.Required);

        string[] arguments = tenant is null ? args : [.. args, "--tenant", tenant];
        var parseResult = command.Parse(arguments);
        Assert.Empty(parseResult.Errors);

        var options = OptionBinder.BindOptions<TOptions>(parseResult);
        Assert.Equal(tenant, options.Tenant);
        return options;
    }
}
