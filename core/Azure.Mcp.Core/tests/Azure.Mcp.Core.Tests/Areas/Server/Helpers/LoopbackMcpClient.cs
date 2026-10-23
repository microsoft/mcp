// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Nodes;
using System.Threading.Channels;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace Azure.Mcp.Core.Tests.Areas.Server.Helpers;

/// <summary>
/// Test helper that builds a real <see cref="McpClient"/> backed by an in-process loopback
/// transport.
/// <para>
/// The MCP 2026-07-28 beta SDK (<c>2.0.0-preview.1</c>) added a <c>private protected abstract</c>
/// member (<c>ResolveInputRequestsAsync</c>) to <see cref="McpClient"/>. This makes the type
/// impossible to proxy with NSubstitute/Castle and impossible to subclass from a test assembly,
/// so <c>Substitute.For&lt;McpClient&gt;()</c> throws a <see cref="TypeLoadException"/> at
/// mock-creation time. Instead, tests create a genuine client over this loopback transport and
/// supply a responder that answers <c>tools/list</c>, <c>tools/call</c>, etc.
/// </para>
/// <para>
/// The client is forced onto the legacy <c>2025-11-25</c> protocol version so the connection
/// completes with the stable <c>initialize</c> handshake instead of the <c>server/discover</c>
/// negotiation path.
/// </para>
/// </summary>
internal static class LoopbackMcpClient
{
    private const string LegacyProtocolVersion = "2025-11-25";

    /// <summary>
    /// Creates a real <see cref="McpClient"/> whose requests are answered in-process by the
    /// supplied <paramref name="responder"/>. The <c>initialize</c> handshake is answered
    /// automatically; the responder only needs to handle protocol methods such as
    /// <c>tools/list</c> and <c>tools/call</c>.
    /// </summary>
    /// <param name="responder">
    /// A function that maps an incoming <see cref="JsonRpcRequest"/> to a
    /// <see cref="JsonRpcResponse"/>. Return <see langword="null"/> to fall back to an empty
    /// success result for unrecognized methods.
    /// </param>
    public static McpClient Create(Func<JsonRpcRequest, JsonRpcResponse?> responder)
        => Create(responder, throwOnDispose: false, out _);

    /// <summary>
    /// Creates a real <see cref="McpClient"/> and returns a <see cref="LoopbackDisposeTracker"/>
    /// that observes when the client disposes its underlying transport. Because the beta SDK
    /// makes <see cref="McpClient"/> impossible to proxy, disposal must be verified through the
    /// transport the client owns rather than via a mock.
    /// </summary>
    public static McpClient Create(Func<JsonRpcRequest, JsonRpcResponse?> responder, out LoopbackDisposeTracker tracker)
        => Create(responder, throwOnDispose: false, out tracker);

    /// <summary>
    /// Creates a real <see cref="McpClient"/> whose transport throws on disposal, to exercise
    /// disposal-exception handling paths. A <see cref="LoopbackDisposeTracker"/> is returned to
    /// observe the disposal attempt.
    /// </summary>
    public static McpClient CreateThrowingOnDispose(Func<JsonRpcRequest, JsonRpcResponse?> responder, out LoopbackDisposeTracker tracker)
        => Create(responder, throwOnDispose: true, out tracker);

    private static McpClient Create(Func<JsonRpcRequest, JsonRpcResponse?> responder, bool throwOnDispose, out LoopbackDisposeTracker tracker)
    {
        var disposeTracker = new LoopbackDisposeTracker();
        var transport = new LoopbackClientTransport(request => Respond(request, responder), disposeTracker, throwOnDispose);
        var options = new McpClientOptions
        {
            ProtocolVersion = LegacyProtocolVersion,
            ClientInfo = new Implementation { Name = "loopback-test-client", Version = "1.0.0" }
        };

        // The loopback transport answers synchronously, so this completes without blocking on I/O.
        var client = McpClient.CreateAsync(transport, options).GetAwaiter().GetResult();
        tracker = disposeTracker;
        return client;
    }

    private static JsonRpcMessage? Respond(JsonRpcRequest request, Func<JsonRpcRequest, JsonRpcResponse?> responder)
    {
        if (string.Equals(request.Method, RequestMethods.Initialize, StringComparison.Ordinal))
        {
            return new JsonRpcResponse
            {
                Id = request.Id,
                Result = new JsonObject
                {
                    ["protocolVersion"] = LegacyProtocolVersion,
                    ["capabilities"] = new JsonObject { ["tools"] = new JsonObject() },
                    ["serverInfo"] = new JsonObject
                    {
                        ["name"] = "loopback-test-server",
                        ["version"] = "1.0.0"
                    }
                }
            };
        }

        var response = responder(request);
        if (response is not null)
        {
            response.Id = request.Id;
            return response;
        }

        // Unhandled request: return an empty success result so the client does not fault.
        return new JsonRpcResponse { Id = request.Id, Result = new JsonObject() };
    }

    private sealed class LoopbackClientTransport(
        Func<JsonRpcRequest, JsonRpcMessage?> responder,
        LoopbackDisposeTracker tracker,
        bool throwOnDispose) : IClientTransport
    {
        public string Name => "loopback";

        public Task<ITransport> ConnectAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<ITransport>(new LoopbackTransport(responder, tracker, throwOnDispose));
    }

    private sealed class LoopbackTransport(
        Func<JsonRpcRequest, JsonRpcMessage?> responder,
        LoopbackDisposeTracker tracker,
        bool throwOnDispose) : ITransport
    {
        private readonly Channel<JsonRpcMessage> _incoming = Channel.CreateUnbounded<JsonRpcMessage>();

        public string? SessionId => null;

        public ChannelReader<JsonRpcMessage> MessageReader => _incoming.Reader;

        public async Task SendMessageAsync(JsonRpcMessage message, CancellationToken cancellationToken = default)
        {
            // Only requests expect a correlated response; notifications are fire-and-forget.
            if (message is JsonRpcRequest request)
            {
                var response = responder(request);
                if (response is not null)
                {
                    await _incoming.Writer.WriteAsync(response, cancellationToken);
                }
            }
        }

        public ValueTask DisposeAsync()
        {
            tracker.RecordDispose();
            _incoming.Writer.TryComplete();
            if (throwOnDispose)
            {
                throw new InvalidOperationException("Loopback transport disposal failed.");
            }
            return ValueTask.CompletedTask;
        }
    }
}

/// <summary>
/// Observes disposal of a loopback-backed <see cref="McpClient"/>. Because the beta SDK makes
/// <see cref="McpClient"/> impossible to mock, disposal is verified via the transport the client
/// owns and disposes.
/// </summary>
internal sealed class LoopbackDisposeTracker
{
    private int _disposeCount;

    /// <summary>Number of times the underlying transport was disposed.</summary>
    public int DisposeCount => Volatile.Read(ref _disposeCount);

    /// <summary>Whether the underlying transport has been disposed at least once.</summary>
    public bool WasDisposed => DisposeCount > 0;

    internal void RecordDispose() => Interlocked.Increment(ref _disposeCount);
}
