using Azure.Mcp.Core.Services.Azure.Subscription;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;

namespace Azure.Mcp.Tests.Commands;

public abstract class SubscriptionCommandUnitTestsBase<TCommand, TService> : CommandUnitTestsBase<TCommand, TService>
    where TCommand : class, IBaseCommand
    where TService : class
{
    protected ISubscriptionResolver SubscriptionResolver { get; }

    public SubscriptionCommandUnitTestsBase()
    {
        SubscriptionResolver = Substitute.For<ISubscriptionResolver>();
        SubscriptionResolver.ResolveSubscription(Arg.Any<string?>()).Returns(args => args[0]);
        Services.AddSingleton(SubscriptionResolver);
    }
}
