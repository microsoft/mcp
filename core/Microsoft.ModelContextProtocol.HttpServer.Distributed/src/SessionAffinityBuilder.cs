// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.ModelContextProtocol.HttpServer.Distributed.Abstractions;

namespace Microsoft.ModelContextProtocol.HttpServer.Distributed;

internal sealed class SessionAffinityBuilder(IServiceCollection services) : ISessionAffinityBuilder
{
    public IServiceCollection Services { get; } = services;
}
