// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Tools.Redis.Options.CacheForRedis;

namespace Azure.Mcp.Tools.Redis.Commands.CacheForRedis;

public abstract class BaseCacheCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] T>
    : SubscriptionCommand<T> where T : BaseCacheOptions, new()
{
    protected readonly Option<string> _cacheOption = RedisOptionDefinitions.Cache;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(_cacheOption);
        RequireResourceGroup();
    }

    protected override T BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Cache = parseResult.GetValue(_cacheOption);
        return options;
    }
}
