using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.ModelContextProtocol.HttpServer.Distributed.Abstractions;

/// <summary>
/// A builder for configuring MCP session affinity.
/// </summary>
public interface ISessionAffinityBuilder
{
    /// <summary>
    /// Gets the host application builder.
    /// </summary>
    IServiceCollection Services { get; }
}
