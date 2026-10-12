// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Xunit;

namespace Microsoft.Mcp.Core.Tests.Commands;

public sealed class CommandContextAccessorTests
{
    [Fact]
    public async Task BeginScope_FlowsAcrossAwaits()
    {
        var accessor = new CommandContextAccessor();
        var context = new CommandContext { ToolNamespaceName = "storage" };
        Assert.Null(accessor.CurrentContext);

        using (accessor.BeginScope(context))
        {
            await Task.Yield();
            Assert.Same(context, accessor.CurrentContext);
            await Task.Run(() => Assert.Same(context, accessor.CurrentContext), TestContext.Current.CancellationToken);
        }

        Assert.Null(accessor.CurrentContext);
    }

    [Fact]
    public async Task BeginScope_RejectsNestedScopeWithoutReplacingActiveContext()
    {
        var accessor = new CommandContextAccessor();
        var context = new CommandContext { ToolNamespaceName = "storage" };
        using (accessor.BeginScope(context))
        {
            await Task.Yield();
            Assert.Throws<InvalidOperationException>(() => accessor.BeginScope(new CommandContext()));
            Assert.Same(context, accessor.CurrentContext);
            await Task.Run(() =>
            {
                Assert.Throws<InvalidOperationException>(() => accessor.BeginScope(new CommandContext()));
                Assert.Same(context, accessor.CurrentContext);
            }, TestContext.Current.CancellationToken);
        }

        Assert.Null(accessor.CurrentContext);
    }

    [Fact]
    public async Task BeginScope_IsolatesConcurrentInvocations()
    {
        var accessor = new CommandContextAccessor();
        var bothEntered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        int entered = 0;

        async Task ExecuteAsync(string namespaceName)
        {
            using (accessor.BeginScope(new CommandContext { ToolNamespaceName = namespaceName }))
            {
                if (Interlocked.Increment(ref entered) == 2)
                {
                    bothEntered.SetResult();
                }

                await bothEntered.Task.WaitAsync(TestContext.Current.CancellationToken);
                Assert.Equal(namespaceName, accessor.CurrentContext?.ToolNamespaceName);
            }

            Assert.Null(accessor.CurrentContext);
        }

        await Task.WhenAll(ExecuteAsync("storage"), ExecuteAsync("compute"));
        Assert.Null(accessor.CurrentContext);
    }

    [Fact]
    public async Task Dispose_ClearsContextInheritedByOutlivingChild()
    {
        var accessor = new CommandContextAccessor();
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        Task child;
        using (accessor.BeginScope(new CommandContext { ToolNamespaceName = "storage" }))
        {
            child = Task.Run(async () =>
            {
                await release.Task.WaitAsync(TestContext.Current.CancellationToken);
                Assert.Null(accessor.CurrentContext);
                var context = new CommandContext { ToolNamespaceName = "compute" };
                using (accessor.BeginScope(context))
                {
                    await Task.Yield();
                    Assert.Same(context, accessor.CurrentContext);
                }

                Assert.Null(accessor.CurrentContext);
            }, TestContext.Current.CancellationToken);
        }

        release.SetResult();
        await child;
        Assert.Null(accessor.CurrentContext);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Dispose_CleansUpExceptionalExecution(bool cancellation)
    {
        var accessor = new CommandContextAccessor();
        async Task ExecuteAsync()
        {
            using (accessor.BeginScope(new CommandContext()))
            {
                await Task.Yield();
                Assert.NotNull(accessor.CurrentContext);
                if (cancellation)
                {
                    throw new OperationCanceledException();
                }

                throw new InvalidOperationException();
            }
        }

        if (cancellation)
        {
            await Assert.ThrowsAsync<OperationCanceledException>(ExecuteAsync);
        }
        else
        {
            await Assert.ThrowsAsync<InvalidOperationException>(ExecuteAsync);
        }
        Assert.Null(accessor.CurrentContext);
    }

    [Fact]
    public void Dispose_AllowsSuccessiveScopesAndRepeatedDisposal()
    {
        var accessor = new CommandContextAccessor();
        IDisposable first = accessor.BeginScope(new CommandContext());
        first.Dispose();
        Assert.Null(accessor.CurrentContext);
        var context = new CommandContext { ToolNamespaceName = "compute" };
        using (accessor.BeginScope(context))
        {
            first.Dispose();
            Assert.Same(context, accessor.CurrentContext);
        }

        Assert.Null(accessor.CurrentContext);
        Assert.Throws<ArgumentNullException>(() => accessor.BeginScope(null!));
    }

    [Fact]
    public async Task Dispose_RejectsScopeThatIsNotCurrentWithoutPreventingCleanup()
    {
        var accessor = new CommandContextAccessor();
        var context = new CommandContext { ToolNamespaceName = "storage" };
        using (IDisposable scope = accessor.BeginScope(context))
        {
            Task otherFlow;
            using (ExecutionContext.SuppressFlow())
            {
                otherFlow = Task.Run(() => Assert.Throws<InvalidOperationException>(scope.Dispose),
                    TestContext.Current.CancellationToken);
            }

            await otherFlow;
            Assert.Same(context, accessor.CurrentContext);
        }

        Assert.Null(accessor.CurrentContext);
    }

    [Fact]
    public void Registration_PreservesOverridesAndIsolatesServiceProviders()
    {
        var services = new ServiceCollection();
        services.AddCommandContextAccessor().AddCommandContextAccessor();
        using ServiceProvider first = services.BuildServiceProvider();
        using ServiceProvider second = services.BuildServiceProvider();
        ICommandContextAccessor firstAccessor = first.GetRequiredService<ICommandContextAccessor>();
        ICommandContextAccessor secondAccessor = second.GetRequiredService<ICommandContextAccessor>();
        Assert.NotSame(firstAccessor, secondAccessor);
        using (firstAccessor.BeginScope(new CommandContext()))
        {
            Assert.Null(secondAccessor.CurrentContext);
        }

        var custom = new CommandContextAccessor();
        services.AddSingleton<ICommandContextAccessor>(custom);
        services.AddCommandContextAccessor();
        using ServiceProvider overridden = services.BuildServiceProvider();
        Assert.Same(custom, overridden.GetRequiredService<ICommandContextAccessor>());
    }
}
